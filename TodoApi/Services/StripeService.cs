using Stripe;
using TodoApi.Dto.Payment;
using TodoApi.Models;
using TodoApi.Models.Stripe;

namespace TodoApi.Services;

public class StripeService : IStripeService
{
    private readonly ChargeService _chargeService;
    private readonly CustomerService _customerService;
    private readonly TokenService _tokenService;

    public StripeService(ChargeService chargeService, CustomerService customerService, TokenService tokenService)
    {
        _chargeService = chargeService;
        _customerService = customerService;
        _tokenService = tokenService;
    }

    public async Task<StripeCustomer> AddStripeCustomerAsync(StripeCustomerRequest customerRequest, CancellationToken token)
    {
        var tokenOptions = new TokenCreateOptions()
        {
            Card = new TokenCardOptions()
            {
                Name = customerRequest.Name,
                Number = customerRequest.CreditCard.CardNumber,
                ExpYear = customerRequest.CreditCard.ExpirationYear,
                ExpMonth = customerRequest.CreditCard.ExpirationMonth,
                Cvc = customerRequest.CreditCard.Cvc
            }
        };

        var stripeToken = await _tokenService.CreateAsync(tokenOptions, null, token);
        var customerOptions = new CustomerCreateOptions()
        {
            Name = customerRequest.Name,
            Email = customerRequest.Email,
            Source = stripeToken.Id,
            Balance = 1000000,
        };
        var newCustomer = await _customerService.CreateAsync(customerOptions, null, token);
        if (newCustomer is not null)
        {
            return new StripeCustomer()
            {
                Name = newCustomer.Name,
                Email = newCustomer.Email,
                CustomerId = newCustomer.Id
            };
        }

        return null;
    }

    public async Task<StripePayment> AddStripePaymentAsync(StripePaymentRequest paymentRequest, CancellationToken token)
    {
        var cus = await _customerService.GetAsync(paymentRequest.CustomerId);
        if (cus.Balance < paymentRequest.Amount)
        {
            return null;
        }
        var paymentOptions = new ChargeCreateOptions()
        {
            Customer = paymentRequest.CustomerId,
            ReceiptEmail = paymentRequest.ReceiptEmail,
            Description = paymentRequest.Description,
            Currency = paymentRequest.Currency,
            Amount = paymentRequest.Amount
        };

        var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, token);
        
        if (createdPayment is not null)
        {
            var updateBalanceOfCustomerOption = new CustomerUpdateOptions()
            {
                Balance = cus.Balance - paymentRequest.Amount,
                Description = createdPayment.Description,
                
            };
            var res = await _customerService.UpdateAsync(paymentRequest.CustomerId, updateBalanceOfCustomerOption);
            if (res is not null)
            {
                return new StripePayment()
                {
                    CustomerId = createdPayment.CustomerId,
                    ReceiptEmail = createdPayment.ReceiptEmail,
                    Description = createdPayment.Description,
                    Currency = createdPayment.Currency,
                    Amount = createdPayment.Amount,
                    PaymentId = createdPayment.Id
                };
            }
        }

        return null;
    }
}
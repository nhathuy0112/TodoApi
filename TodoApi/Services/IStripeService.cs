using TodoApi.Dto.Payment;
using TodoApi.Models;
using TodoApi.Models.Stripe;

namespace TodoApi.Services;

public interface IStripeService
{
    Task<StripeCustomer> AddStripeCustomerAsync(StripeCustomerRequest customerRequest, CancellationToken token);
    Task<StripePayment> AddStripePaymentAsync(StripePaymentRequest paymentRequest, CancellationToken token);
}
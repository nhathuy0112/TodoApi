using Microsoft.AspNetCore.Mvc;
using TodoApi.Dto.Payment;
using TodoApi.Models.Stripe;
using TodoApi.Services;

namespace TodoApi.Controllers;

public class PaymentController : BaseController
{
    private readonly IStripeService _stripeService;

    public PaymentController(IStripeService stripeService)
    {
        _stripeService = stripeService;
    }

    [HttpPost("payment/customer")]
    public async Task<ActionResult<StripeCustomer>> AddCustomer([FromBody] StripeCustomerRequest customerRequest,
        CancellationToken token)
    {
        var newCustomer = await _stripeService.AddStripeCustomerAsync(customerRequest, token);
        if (newCustomer is not null)
        {
            return Ok(newCustomer);
        }

        return BadRequest();
    }
    
    [HttpPost("payment/add")]
    public async Task<ActionResult<StripePayment>> AddStripePayment(
        [FromBody] StripePaymentRequest payment,
        CancellationToken token)
    {
        var newPayment = await _stripeService.AddStripePaymentAsync(
            payment,
            token);
        if (newPayment is not null)
        {
            return Ok(newPayment);
        }

        return BadRequest();
    }
}
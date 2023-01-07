using TodoApi.Models;
using TodoApi.Models.Stripe;

namespace TodoApi.Dto.Payment;

public class StripeCustomerRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
    public StripeCard CreditCard { get; set; }
}
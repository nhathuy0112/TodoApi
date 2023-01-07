namespace TodoApi.Models.Stripe;

public class StripeCard
{
    public string Name { get; set; }
    public string CardNumber { get; set; }
    public string ExpirationYear { get; set; }
    public string ExpirationMonth { get; set; }
    public string Cvc { get; set; }
}
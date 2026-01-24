namespace DigitalBank.Application.Dtos.Stripe
{
    public class TopUpRequestDto
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "azn";
    }
}

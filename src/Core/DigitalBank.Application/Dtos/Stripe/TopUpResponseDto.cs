namespace DigitalBank.Application.Dtos.Stripe
{
    public class TopUpResponseDto
    {
        public string CheckoutUrl { get; set; } = null!;
        public string SessionId { get; set; } = null!;
    }
}

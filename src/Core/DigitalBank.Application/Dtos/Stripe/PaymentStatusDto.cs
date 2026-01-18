namespace DigitalBank.Application.Dtos.Stripe
{
    public class PaymentStatusDto
    {
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
    }
}

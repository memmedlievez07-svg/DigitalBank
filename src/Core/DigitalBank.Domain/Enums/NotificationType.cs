namespace DigitalBank.Domain.Enums;

public enum NotificationType
{
    IncomingTransfer = 1,
    OutgoingTransfer = 2,
    System = 3,
    Security = 4,
    ChatMessage = 5,
    TopUp = 6  // ← YENİ (Stripe top-up üçün)
}

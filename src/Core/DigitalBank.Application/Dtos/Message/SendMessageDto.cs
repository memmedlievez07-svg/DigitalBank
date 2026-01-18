namespace DigitalBank.Application.Dtos.Message
{
    public class SendMessageDto
    {
        public string ReceiverUserId { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}

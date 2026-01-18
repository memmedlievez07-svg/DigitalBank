namespace DigitalBank.Application.Dtos.Message
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string SenderUserId { get; set; } = null!;
        public string ReceiverUserId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}

using DigitalBank.Domain.Enums;
namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class NotificationListItemDto
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? RelatedTransactionId { get; set; }
    }
}

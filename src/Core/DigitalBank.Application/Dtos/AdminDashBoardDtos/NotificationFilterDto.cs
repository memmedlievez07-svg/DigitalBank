using DigitalBank.Domain.Enums;

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class NotificationFilterDto
    {

        public NotificationType? Type { get; set; }
        public bool? IsRead { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

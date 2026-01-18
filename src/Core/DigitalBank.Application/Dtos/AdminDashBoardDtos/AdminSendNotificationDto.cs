using DigitalBank.Domain.Enums;

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public  class AdminSendNotificationDto
    {
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public NotificationType Type { get; set; } = NotificationType.System;

        // optional: hansı transaction-la bağlıdır
        public int? RelatedTransactionId { get; set; }
    }
}

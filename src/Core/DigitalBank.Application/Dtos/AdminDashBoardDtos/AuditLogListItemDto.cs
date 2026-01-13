using DigitalBank.Domain.Enums;
namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class AuditLogListItemDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public AuditActionType ActionType { get; set; }
        public bool IsSuccess { get; set; }

        public int? RelatedTransactionId { get; set; }
        public string? Description { get; set; }
        public string? DetailsJson { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? CorrelationId { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

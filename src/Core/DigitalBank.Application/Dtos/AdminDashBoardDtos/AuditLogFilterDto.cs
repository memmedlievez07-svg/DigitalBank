using DigitalBank.Domain.Enums;
namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class AuditLogFilterDto
    {
        public string? Search { get; set; }
        public string? UserId { get; set; }
        public AuditActionType? ActionType { get; set; }
        public bool? IsSuccess { get; set; }

        public string? CorrelationId { get; set; }
        public string? IpAddress { get; set; }
        public int? RelatedTransactionId { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}

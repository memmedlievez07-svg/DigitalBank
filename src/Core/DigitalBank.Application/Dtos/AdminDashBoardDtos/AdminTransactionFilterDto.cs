using DigitalBank.Domain.Enums;
namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class AdminTransactionFilterDto
    {
        public TransactionType? Type { get; set; }
        public TransactionStatus? Status { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }

        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        public string? SenderUserId { get; set; }
        public string? ReceiverUserId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

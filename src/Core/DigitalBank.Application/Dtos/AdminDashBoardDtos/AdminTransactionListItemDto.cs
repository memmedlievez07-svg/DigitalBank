namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class AdminTransactionListItemDto
    {
        public int Id { get; set; }
        public string ReferenceNo { get; set; } = null!;

        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }

        public int Type { get; set; }
        public int Status { get; set; }

        public int? SenderWalletId { get; set; }
        public string? SenderUserId { get; set; }

        public int? ReceiverWalletId { get; set; }
        public string? ReceiverUserId { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

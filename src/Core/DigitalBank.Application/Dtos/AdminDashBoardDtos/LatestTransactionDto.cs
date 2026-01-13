namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class LatestTransactionDto
    {
        public int Id { get; set; }
        public string ReferenceNo { get; set; } = null!;

        public int? SenderWalletId { get; set; }
        public int? ReceiverWalletId { get; set; }

        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }

        public int Type { get; set; }
        public int Status { get; set; }

        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

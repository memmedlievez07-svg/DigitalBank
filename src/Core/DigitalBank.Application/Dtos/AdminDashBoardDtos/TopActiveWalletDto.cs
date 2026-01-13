namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class TopActiveWalletDto
    {
        public int WalletId { get; set; }
        public string UserId { get; set; } = null!;
        public decimal Balance { get; set; }
        public int TxCountLast7Days { get; set; }
        public decimal VolumeLast7Days { get; set; }
    }
}

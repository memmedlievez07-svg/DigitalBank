namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class WalletsKpiDto
    {
        public int TotalWallets { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal AvgBalance { get; set; }
        public WalletStatusBreakdownDto StatusBreakdown { get; set; } = new();
    }
}

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class DashboardLatestDto
    {
        public List<LatestTransactionDto> Transactions { get; set; } = new();
        public List<LatestAuditDto> Audits { get; set; } = new();
        public List<LatestNotificationDto> Notifications { get; set; } = new();
        public List<TopActiveWalletDto> TopActiveWalletsLast7Days { get; set; } = new();
    }
}

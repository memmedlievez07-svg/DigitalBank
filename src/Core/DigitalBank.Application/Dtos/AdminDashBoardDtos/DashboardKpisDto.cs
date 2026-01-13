
namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class DashboardKpisDto
    {
        public UsersKpiDto Users { get; set; } = new();
        public WalletsKpiDto Wallets { get; set; } = new();
        public TransactionsKpiDto Transactions { get; set; } = new();
        public NotificationsKpiDto Notifications { get; set; } = new();
        public ChatKpiDto Chat { get; set; } = new();
        public AuditKpiDto Audit { get; set; } = new();
    }
}

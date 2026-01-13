namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class DashboardTrendsDto
    {
        public List<DailyTxTrendDto> TxDailyLast7Days { get; set; } = new();
        public List<DailyCountTrendDto> NewUsersLast7Days { get; set; } = new();
    }
}

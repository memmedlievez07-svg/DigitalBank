namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class AdminDashboardDto
    {
        public DashboardKpisDto Kpis { get; set; } = new();
        public DashboardTrendsDto Trends { get; set; } = new();
        public DashboardLatestDto Latest { get; set; } = new();
    }
}

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class UsersKpiDto
    {
        public int Total { get; set; }
        public int Locked { get; set; }
        public int EmailConfirmed { get; set; }
        public double EmailConfirmedRate { get; set; }
        public int NewLast7Days { get; set; }
    }
}

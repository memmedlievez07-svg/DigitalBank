namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class DailyTxTrendDto
    {
        public DateTime DayUtc { get; set; }
        public int Count { get; set; }
        public decimal Volume { get; set; }
    }
}

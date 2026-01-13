namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class NotificationTypeBreakdownDto
    {
        public int IncomingTransfer { get; set; }
        public int OutgoingTransfer { get; set; }
        public int System { get; set; }
        public int Security { get; set; }
    }
}

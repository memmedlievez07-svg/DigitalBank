namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class TransactionStatusBreakdownDto
    {
        public int Pending { get; set; }
        public int Completed { get; set; }
        public int Failed { get; set; }
        public int Reversed { get; set; }
    }
}

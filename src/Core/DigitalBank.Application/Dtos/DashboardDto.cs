namespace DigitalBank.Application.Dtos
{
    public class DashboardDto
    {
        public decimal TotalBalance { get; set; }
        public string Currency { get; set; } = "AZN";
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyExpense { get; set; }

        // Sənin mövcud DTO-ların:
        public List<TransactionListItemDto> RecentTransactions { get; set; } = new();
        public List<RecentTransferDto> QuickTransfers { get; set; } = new();
    }
}
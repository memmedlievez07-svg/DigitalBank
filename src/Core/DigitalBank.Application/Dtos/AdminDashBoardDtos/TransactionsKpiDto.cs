using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class TransactionsKpiDto
    {
        public int Total { get; set; }
        public int Last24HoursCount { get; set; }
        public decimal Last30DaysVolume { get; set; }
        public double Last30DaysSuccessRate { get; set; }
        public TransactionStatusBreakdownDto StatusBreakdownLast30Days { get; set; } = new();
        public int FailedLast24Hours { get; set; }
    }
}

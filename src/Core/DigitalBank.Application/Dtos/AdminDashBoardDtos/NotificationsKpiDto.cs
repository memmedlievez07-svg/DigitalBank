using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class NotificationsKpiDto
    {
        public int Total { get; set; }
        public int Unread { get; set; }
        public int Last7Days { get; set; }
        public NotificationTypeBreakdownDto TypeBreakdownLast7Days { get; set; } = new();
    }
}

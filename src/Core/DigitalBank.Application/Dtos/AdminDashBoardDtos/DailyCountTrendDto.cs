using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class DailyCountTrendDto
    {
        public DateTime DayUtc { get; set; }
        public int Count { get; set; }
    }
}

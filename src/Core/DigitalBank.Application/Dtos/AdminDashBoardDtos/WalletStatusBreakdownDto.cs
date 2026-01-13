using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class WalletStatusBreakdownDto
    {
        public int Active { get; set; }
        public int Blocked { get; set; }
        public int Closed { get; set; }
    }
}

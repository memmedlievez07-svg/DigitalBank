using DigitalBank.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos.Wallet
{
    public class AdminWalletFilterDto
    {
        public string? UserId { get; set; }
        public string? CardNumber { get; set; }
        public WalletStatus? Status { get; set; }
        
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    
    }
}

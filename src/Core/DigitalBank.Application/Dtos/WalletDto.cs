using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos
{
    public class WalletDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = null!;
        public int Status { get; set; } // enum int kimi göndəririk (istəsən string edərik)
    }
}

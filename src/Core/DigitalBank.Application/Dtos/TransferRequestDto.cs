using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos
{
    public class TransferRequestDto
    {
        // receiver üçün ya WalletId, ya da UserName/Email seçmək olar.
        public string ReceiverCardNumber { get; set; } = null!;

        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}

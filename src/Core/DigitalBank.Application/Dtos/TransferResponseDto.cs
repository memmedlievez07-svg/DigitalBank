using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos
{
    public class TransferResponseDto
    {
        public string ReferenceNo { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }
        public DateTime CreatedDateUtc { get; set; }

        public int SenderWalletId { get; set; }
        public int ReceiverWalletId { get; set; }
    }
}

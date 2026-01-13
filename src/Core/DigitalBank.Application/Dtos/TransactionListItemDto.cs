using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos
{
    public class TransactionListItemDto
    {
        public int Id { get; set; }
        public string ReferenceNo { get; set; } = null!;

        public int? SenderWalletId { get; set; }
        public int? ReceiverWalletId { get; set; }

        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }

        public int Type { get; set; }   // enum int
        public int Status { get; set; } // enum int

        public string? Description { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }
}

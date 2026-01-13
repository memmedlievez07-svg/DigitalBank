using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos
{
    public class AdminTransactionDetailsDto
    {
        public int Id { get; set; }
        public string ReferenceNo { get; set; } = null!;

        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }

        public int Type { get; set; }
        public int Status { get; set; }

        public int? SenderWalletId { get; set; }
        public string? SenderUserId { get; set; }

        public int? ReceiverWalletId { get; set; }
        public string? ReceiverUserId { get; set; }

        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}

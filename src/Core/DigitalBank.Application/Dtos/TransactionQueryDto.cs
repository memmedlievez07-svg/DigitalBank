using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos
{
    public class TransactionQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public int? Type { get; set; }
        public int? Status { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
    }
}

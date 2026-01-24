using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Dtos
{
    public class TransactionDetailDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Type { get; set; } // 0: Mədaxil, 1: Məxaric

        // Əlavə məlumatlar
        public string CounterpartyName { get; set; } // Pul göndərilən/alınan şəxsin adı
        public string CounterpartyCard { get; set; } // Kart nömrəsi
        public decimal BalanceAfter { get; set; } // Əməliyyatdan sonrakı qalıq (əlavə etmək istəsən)
    }
}

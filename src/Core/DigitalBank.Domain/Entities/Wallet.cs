using DigitalBank.Domain.Entities.Base;
using DigitalBank.Domain.Entities.Identity;
using DigitalBank.Domain.Enums;
using System.Collections.Generic;
using System.Transactions;

namespace DigitalBank.Domain.Entities
{
    public class Wallet : BaseEntity
    {
        public string UserId { get; set; }
        public string CardNumber { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        public decimal Balance { get; set; }
        public string Currency { get; set; } = "AZN";

        public WalletStatus Status { get; set; } = WalletStatus.Active;

        public ICollection<BankTransaction> SentTransactions { get; set; } = new List<BankTransaction>();
        public ICollection<BankTransaction> ReceivedTransactions { get; set; } = new List<BankTransaction>();
        public decimal DailyLimit { get; set; } = 1000;   // Günlük standart limit
        public decimal MonthlyLimit { get; set; } = 10000; // Aylıq standart limit
    }
}

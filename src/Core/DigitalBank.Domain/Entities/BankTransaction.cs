using DigitalBank.Domain.Entities.Base;
using DigitalBank.Domain.Enums;
namespace DigitalBank.Domain.Entities
{
    public class BankTransaction :BaseEntity
    {
        public int? SenderWalletId { get; set; }
        public Wallet? SenderWallet { get; set; }

        public int? ReceiverWalletId { get; set; }
        public Wallet? ReceiverWallet { get; set; }

        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; } = 0m;

        public TransactionType Type { get; set; } = TransactionType.Transfer;
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        // Unique olacaq (Fluent config + index)
        public string ReferenceNo { get; set; } = null!;

        public string? Description { get; set; }
    }
}

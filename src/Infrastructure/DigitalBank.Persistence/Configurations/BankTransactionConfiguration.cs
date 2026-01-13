//using DigitalBank.Domain.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace DigitalBank.Persistence.Configurations
//{
//    public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
//    {
//        public void Configure(EntityTypeBuilder<BankTransaction> builder)
//        {
//            builder.ToTable("BankTransactions");

//            builder.HasKey(x => x.Id);

//            builder.Property(x => x.Amount)
//                   .HasPrecision(18, 2)
//                   .IsRequired();

//            builder.Property(x => x.FeeAmount)
//                   .HasPrecision(18, 2)
//                   .IsRequired();

//            builder.Property(x => x.ReferenceNo)
//                   .HasMaxLength(64)
//                   .IsRequired();

//            builder.HasIndex(x => x.ReferenceNo)
//                   .IsUnique();

//            builder.Property(x => x.Type).IsRequired();
//            builder.Property(x => x.Status).IsRequired();

//            // Sender FK (Wallet -> SentTransactions)
//            builder.HasOne(x => x.SenderWallet)
//                   .WithMany(w => w.SentTransactions)
//                   .HasForeignKey(x => x.SenderWalletId)
//                   .OnDelete(DeleteBehavior.Restrict);

//            // Receiver FK (Wallet -> ReceivedTransactions)
//            builder.HasOne(x => x.ReceiverWallet)
//                   .WithMany(w => w.ReceivedTransactions)
//                   .HasForeignKey(x => x.ReceiverWalletId)
//                   .OnDelete(DeleteBehavior.Restrict);

//            // Description optional
//            builder.Property(x => x.Description)
//                   .HasMaxLength(500);
//        }
//    }
//}
using DigitalBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalBank.Persistence.Configurations
{
    public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
    {
        public void Configure(EntityTypeBuilder<BankTransaction> builder)
        {
            builder.Property(x => x.ReferenceNo)
                   .HasMaxLength(64)
                   .IsRequired();

            builder.HasIndex(x => x.ReferenceNo).IsUnique();

            builder.Property(x => x.Amount)
                   .HasPrecision(18, 2);

            builder.Property(x => x.FeeAmount)
                   .HasPrecision(18, 2);

            // Sender/Receiver FK-lar burada da görünə bilər, amma WalletConfiguration-da artıq verdik.
        }
    }
}


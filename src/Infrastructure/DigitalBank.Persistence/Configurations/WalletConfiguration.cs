using DigitalBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalBank.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.Property(x => x.Currency)
                   .HasMaxLength(3)
                   .IsRequired();

            builder.HasOne(w => w.User)
                   .WithOne(u => u.Wallet)
                   .HasForeignKey<Wallet>(w => w.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1 wallet -> many sent tx
            builder.HasMany(w => w.SentTransactions)
                   .WithOne(t => t.SenderWallet)
                   .HasForeignKey(t => t.SenderWalletId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1 wallet -> many received tx
            builder.HasMany(w => w.ReceivedTransactions)
                   .WithOne(t => t.ReceiverWallet)
                   .HasForeignKey(t => t.ReceiverWalletId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UserId).IsUnique(); // 1 user = 1 wallet
        }
    }
}

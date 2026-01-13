using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalBank.Persistence.Configurations.Identity
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // Identity default: AspNetUsers
            // İstəsən ad dəyiş:
            builder.ToTable("Users");

            builder.Property(x => x.FirstName).HasMaxLength(50);
            builder.Property(x => x.LastName).HasMaxLength(50);
            builder.Property(x => x.FatherName).HasMaxLength(50);

            builder.Property(x => x.Address).HasMaxLength(250);

            // Age int? -> DB-də int olacaq, əlavə config şərt deyil.
            // Amma istəsən check constraint verə bilərsən (PostgreSQL-də ayrıca yazılır)

            builder.Property(x => x.AvatarUrl).HasMaxLength(500);

            // 1-1 Wallet
            builder.HasOne(x => x.Wallet)
                   .WithOne(w => w.User)
                   .HasForeignKey<Wallet>(w => w.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1-N RefreshTokens
            builder.HasMany(x => x.RefreshTokens)
                   .WithOne(rt => rt.User)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1-N Notifications
            builder.HasMany(x => x.Notifications)
                   .WithOne(n => n.User)
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ChatMessage: 2 relation (Sender/Receiver)
            // Burada Cascade etmə, Restrict/NoAction daha təhlükəsizdir
            builder.HasMany(x => x.SentMessages)
                   .WithOne(m => m.SenderUser)
                   .HasForeignKey(m => m.SenderUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ReceivedMessages)
                   .WithOne(m => m.ReceiverUser)
                   .HasForeignKey(m => m.ReceiverUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

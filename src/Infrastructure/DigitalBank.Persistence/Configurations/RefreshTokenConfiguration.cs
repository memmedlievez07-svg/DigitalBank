using DigitalBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalBank.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash)
                   .HasMaxLength(256)
                   .IsRequired();

            builder.HasIndex(x => x.TokenHash)
                   .IsUnique();

            builder.Property(x => x.ExpiresAt)
                   .IsRequired();

            builder.Property(x => x.ReplacedByTokenHash)
                   .HasMaxLength(256);

            builder.Property(x => x.CreatedByIp)
                   .HasMaxLength(45);

            builder.Property(x => x.UserAgent)
                   .HasMaxLength(256);

            // User 1-N RefreshTokens
            builder.HasOne(x => x.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

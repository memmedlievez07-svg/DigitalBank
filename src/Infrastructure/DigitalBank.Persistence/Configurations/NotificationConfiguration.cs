using DigitalBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalBank.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(x => x.Title)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(x => x.Body)
                   .HasMaxLength(4000)
                   .IsRequired();

            builder.HasOne(x => x.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.RelatedTransaction)
                   .WithMany()
                   .HasForeignKey(x => x.RelatedTransactionId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedDate });
        }
    }
}

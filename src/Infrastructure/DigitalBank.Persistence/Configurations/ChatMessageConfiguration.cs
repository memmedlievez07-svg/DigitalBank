using DigitalBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalBank.Persistence.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.Property(x => x.Message)
                   .HasMaxLength(4000)
                   .IsRequired();

            builder.HasOne(x => x.SenderUser)
                   .WithMany(u => u.SentMessages)
                   .HasForeignKey(x => x.SenderUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReceiverUser)
                   .WithMany(u => u.ReceivedMessages)
                   .HasForeignKey(x => x.ReceiverUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.SenderUserId, x.ReceiverUserId, x.CreatedDate });
            builder.HasIndex(x => new { x.ReceiverUserId, x.IsRead, x.CreatedDate });
        }
    }
}

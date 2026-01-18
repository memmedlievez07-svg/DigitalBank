using DigitalBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalBank.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActionType)
               .IsRequired();
        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.IsSuccess)
               .IsRequired();

        builder.Property(x => x.Description)
               .HasMaxLength(500);

        builder.Property(x => x.IpAddress)
               .HasMaxLength(64);

        builder.Property(x => x.UserAgent)
               .HasMaxLength(512);

        builder.Property(x => x.DetailsJson)
               .HasColumnType("text"); // PostgreSQL-də text, SQL Server-da da işləyir

        // User relation (anonim log ola bilər deyə nullable)
        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.SetNull);

        // Transaction relation (optional)
        builder.HasOne(x => x.RelatedTransaction)
               .WithMany()
               .HasForeignKey(x => x.RelatedTransactionId)
               .OnDelete(DeleteBehavior.SetNull);

        // index-lər (axtarış üçün)
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.RelatedTransactionId);
        builder.HasIndex(x => x.ActionType);
        builder.HasIndex(x => x.CreatedDate);
    }
}

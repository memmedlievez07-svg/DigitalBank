using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Entities.Base;
using DigitalBank.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Dal
{
    public class DigitalBankDbContext : IdentityDbContext<AppUser>
    {
        public DigitalBankDbContext(DbContextOptions<DigitalBankDbContext> options)
            : base(options)
        {
        }

        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<AppUser> AppUsers => Set<AppUser>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Bütün config-ları avtomatik yükləyir
            builder.ApplyConfigurationsFromAssembly(typeof(DigitalBankDbContext).Assembly);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow.AddHours(4);
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = DateTime.UtcNow.AddHours(4);
                        break;
                }
            }
            foreach (var entry in ChangeTracker.Entries<AppUser>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedDate = DateTime.UtcNow.AddHours(4);
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}


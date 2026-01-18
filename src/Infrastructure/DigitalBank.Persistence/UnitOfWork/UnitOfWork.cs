using DigitalBank.Application.Repositories.AuditLog;
using DigitalBank.Application.Repositories.BankTransaction;
using DigitalBank.Application.Repositories.ChatMessage;
using DigitalBank.Application.Repositories.Notification;
using DigitalBank.Application.Repositories.RefreshToken;
using DigitalBank.Application.Repositories.Wallet;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Persistence.Dal;
using DigitalBank.Persistence.Repositories.AuditLog;
using DigitalBank.Persistence.Repositories.BankTransaction;
using DigitalBank.Persistence.Repositories.ChatMessage;
using DigitalBank.Persistence.Repositories.Notification;
using DigitalBank.Persistence.Repositories.RefreshToken;
using DigitalBank.Persistence.Repositories.Wallet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace DigitalBank.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private IBankTransactionWriteRepository _bankTransactionWriteRepository;
        private IBankTransactionReadRepository _bankTransactionReadRepository;

        private IRefreshTokenWriteRepository _refreshTokenWriteRepository;
        private IRefreshTokenReadRepository _refreshTokenReadRepository;

        private IWalletReadRepository _walletReadRepository;
        private IWalletWriteRepository _walletWriteRepository;

        private IAuditLogReadRepository _auditLogReadRepository;
        private IAuditLogWriteRepository _auditLogWriteRepository;

        private INotificationReadRepository _notificationReadRepository;
        private INotificationWriteRepository _notificationWriteRepository;

        private IChatMessageReadRepository _chatMessageReadRepository;
        private IChatMessageWriteRepository _chatMessageWriteRepository;

        private readonly DigitalBankDbContext _context;

        public UnitOfWork(DigitalBankDbContext context)
        {
            _context = context;
        }

        public IBankTransactionReadRepository BankTransactionReadRepository
            => _bankTransactionReadRepository ??= new BankTransactionReadRepository(_context);

        public IBankTransactionWriteRepository BankTransactionWriteRepository
            => _bankTransactionWriteRepository ??= new BankTransactionWriteRepository(_context);

        public IRefreshTokenReadRepository RefreshTokenReadRepository
            => _refreshTokenReadRepository ??= new RefreshTokenReadRepository(_context);

        public IRefreshTokenWriteRepository RefreshTokenWriteRepository
            => _refreshTokenWriteRepository ??= new RefreshTokenWriteRepository(_context);

        public IWalletReadRepository WalletReadRepository
            => _walletReadRepository ??= new WalletReadRepository(_context);

        public IWalletWriteRepository WalletWriteRepository
            => _walletWriteRepository ??= new WalletWriteRepository(_context);

        public IAuditLogReadRepository AuditLogReadRepository
            => _auditLogReadRepository ??= new AuditLogReadRepository(_context);

        public IAuditLogWriteRepository AuditLogWriteRepository
            => _auditLogWriteRepository ??= new AuditLogWriteRepository(_context);

        public IChatMessageReadRepository ChatMessageReadRepository
            => _chatMessageReadRepository ??= new ChatMessageReadRepository(_context);

        public IChatMessageWriteRepository ChatMessageWriteRepository
            => _chatMessageWriteRepository ??= new ChatMessageWriteRepository(_context);

        public INotificationReadRepository NotificationReadRepository
            => _notificationReadRepository ??= new NotificationReadRepository(_context);

        public INotificationWriteRepository NotificationWriteRepository
            => _notificationWriteRepository ??= new NotificationWriteRepository(_context);

        public int Commit() => _context.SaveChanges();

        public Task<int> CommitAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);

        public Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken ct = default)
            => _context.Database.BeginTransactionAsync(isolationLevel, ct);
    }
}

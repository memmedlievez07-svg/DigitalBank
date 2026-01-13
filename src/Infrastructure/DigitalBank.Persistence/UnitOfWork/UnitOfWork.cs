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
using DigitalBank.Persistence.Repositories.RefreshToken;
using DigitalBank.Persistence.Repositories.Wallet;
using DigitalBank.Persistence.Repositories.Notification;

namespace DigitalBank.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        IBankTransactionWriteRepository _bankTransactionWriteRepository;
        IBankTransactionReadRepository _bankTransactionReadRepository;
        IRefreshTokenWriteRepository _refreshTokenWriteRepository;
        IRefreshTokenReadRepository _refreshTokenReadRepository;
        IWalletReadRepository _walletReadRepository;
        IWalletWriteRepository _walletWriteRepository;
        IAuditLogReadRepository _auditLogReadRepository;
        IAuditLogWriteRepository _auditLogWriteRepository;
        INotificationReadRepository _notificationReadRepository;
        INotificationWriteRepository _notificationWriteRepository;
        IChatMessageReadRepository _chatMessageReadRepository;
        IChatMessageWriteRepository _chatMessageWriteRepository;

        private readonly DigitalBankDbContext _context;
        public UnitOfWork(DigitalBankDbContext context)
        {
            _context = context;
        }
        public IBankTransactionReadRepository BankTransactionReadRepository => _bankTransactionReadRepository ?? new BankTransactionReadRepository(_context);

        public IBankTransactionWriteRepository BankTransactionWriteRepository => _bankTransactionWriteRepository ?? new BankTransactionWriteRepository(_context);

        public IRefreshTokenReadRepository RefreshTokenReadRepository => _refreshTokenReadRepository ?? new RefreshTokenReadRepository(_context);

        public IRefreshTokenWriteRepository RefreshTokenWriteRepository => _refreshTokenWriteRepository ?? new RefreshTokenWriteRepository(_context);

        public IWalletReadRepository WalletReadRepository => _walletReadRepository ?? new WalletReadRepository(_context);

        public IWalletWriteRepository WalletWriteRepository => _walletWriteRepository ?? new WalletWriteRepository(_context);

        public IAuditLogReadRepository AuditLogReadRepository => _auditLogReadRepository ?? new AuditLogReadRepository(_context);

        public IAuditLogWriteRepository AuditLogWriteRepository => _auditLogWriteRepository ?? new AuditLogWriteRepository(_context);

        public IChatMessageReadRepository ChatMessageReadRepository => _chatMessageReadRepository ?? new ChatMessageReadRepository(_context);

        public IChatMessageWriteRepository ChatMessageWriteRepository => _chatMessageWriteRepository ?? new ChatMessageWriteRepository(_context);

        public INotificationReadRepository NotificationReadRepository => _notificationReadRepository ?? new NotificationReadRepository(_context);

        public INotificationWriteRepository NotificationWriteRepository => _notificationWriteRepository ?? new NotificationWriteRepository(_context);
    }
}

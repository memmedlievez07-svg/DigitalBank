using DigitalBank.Application.Repositories.AuditLog;
using DigitalBank.Application.Repositories.BankTransaction;
using DigitalBank.Application.Repositories.ChatMessage;
using DigitalBank.Application.Repositories.Notification;
using DigitalBank.Application.Repositories.RefreshToken;
using DigitalBank.Application.Repositories.Wallet;

namespace DigitalBank.Application.UnitOfWork
{
    public interface IUnitOfWork
    {
        IBankTransactionReadRepository BankTransactionReadRepository { get; }
        IBankTransactionWriteRepository BankTransactionWriteRepository { get; }
        IRefreshTokenReadRepository RefreshTokenReadRepository { get; }
        IRefreshTokenWriteRepository RefreshTokenWriteRepository { get; }
        IWalletReadRepository WalletReadRepository { get; }
        IWalletWriteRepository WalletWriteRepository { get; }
        IAuditLogReadRepository AuditLogReadRepository { get; }
        IAuditLogWriteRepository AuditLogWriteRepository { get; }
        IChatMessageReadRepository ChatMessageReadRepository { get; }
        IChatMessageWriteRepository ChatMessageWriteRepository { get; }
        INotificationReadRepository NotificationReadRepository { get; }
        INotificationWriteRepository NotificationWriteRepository
        {
            get;
        }
    }
}
using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Domain.Enums;
using DigitalBank.Persistence.Dal;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services.AdminDashboard
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly DigitalBankDbContext _db;

        public AdminDashboardService(DigitalBankDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResult<AdminDashboardDto>> GetAsync()
        {
            var now = DateTime.UtcNow;
            var last24 = now.AddHours(-24);
            var last7 = now.AddDays(-7);
            var last30 = now.AddDays(-30);

            // =========================
            // USERS
            // =========================
            var totalUsers = await _db.Users.CountAsync();
            var lockedUsers = await _db.Users.CountAsync(u => u.LockoutEnd.HasValue && u.LockoutEnd.Value.UtcDateTime > now);
            var confirmedUsers = await _db.Users.CountAsync(u => u.EmailConfirmed);
            var confirmedRate = totalUsers == 0 ? 0 : (double)confirmedUsers / totalUsers * 100.0;

            // AppUser CreatedDate yoxdur -> 0
            var newUsersLast7 = 0;

            // =========================
            // WALLETS
            // =========================
            var totalWallets = await _db.Wallets.CountAsync();
            var totalBalance = totalWallets == 0 ? 0m : await _db.Wallets.SumAsync(w => w.Balance);
            var avgBalance = totalWallets == 0 ? 0m : totalBalance / totalWallets;

            var walletActive = await _db.Wallets.CountAsync(w => w.Status == WalletStatus.Active);
            var walletBlocked = await _db.Wallets.CountAsync(w => w.Status == WalletStatus.Blocked);
            var walletClosed = await _db.Wallets.CountAsync(w => w.Status == WalletStatus.Closed);

            // =========================
            // TRANSACTIONS
            // =========================
            var totalTx = await _db.BankTransactions.CountAsync();
            var last24TxCount = await _db.BankTransactions.CountAsync(t => t.CreatedDate >= last24);

            var txLast30Q = _db.BankTransactions.Where(t => t.CreatedDate >= last30);
            var txLast30Count = await txLast30Q.CountAsync();
            var txLast30Volume = txLast30Count == 0 ? 0m : await txLast30Q.SumAsync(t => t.Amount);

            var txLast30Completed = txLast30Count == 0 ? 0 : await txLast30Q.CountAsync(t => t.Status == TransactionStatus.Completed);
            var txLast30Pending = txLast30Count == 0 ? 0 : await txLast30Q.CountAsync(t => t.Status == TransactionStatus.Pending);
            var txLast30Failed = txLast30Count == 0 ? 0 : await txLast30Q.CountAsync(t => t.Status == TransactionStatus.Failed);
            var txLast30Reversed = txLast30Count == 0 ? 0 : await txLast30Q.CountAsync(t => t.Status == TransactionStatus.Reversed);

            var txLast30SuccessRate = txLast30Count == 0 ? 0 : (double)txLast30Completed / txLast30Count * 100.0;

            var failedLast24 = await _db.BankTransactions.CountAsync(t => t.CreatedDate >= last24 && t.Status == TransactionStatus.Failed);

            // =========================
            // NOTIFICATIONS
            // =========================
            var totalNotif = await _db.Notifications.CountAsync();
            var unreadNotif = await _db.Notifications.CountAsync(n => !n.IsRead);
            var last7Notif = await _db.Notifications.CountAsync(n => n.CreatedDate >= last7);

            var notifLast7Q = _db.Notifications.Where(n => n.CreatedDate >= last7);

            var notifIncoming = await notifLast7Q.CountAsync(n => n.Type == NotificationType.IncomingTransfer);
            var notifOutgoing = await notifLast7Q.CountAsync(n => n.Type == NotificationType.OutgoingTransfer);
            var notifSystem = await notifLast7Q.CountAsync(n => n.Type == NotificationType.System);
            var notifSecurity = await notifLast7Q.CountAsync(n => n.Type == NotificationType.Security);

            // =========================
            // CHAT
            // =========================
            var chatLast24 = await _db.ChatMessages.CountAsync(m => m.CreatedDate >= last24);
            var chatUnreadLast24 = await _db.ChatMessages.CountAsync(m => m.CreatedDate >= last24 && !m.IsRead);

            // =========================
            // AUDIT
            // =========================
            var auditFailedLast24 = await _db.AuditLogs.CountAsync(a => !a.IsSuccess && a.CreatedDate >= last24);

            // =========================
            // TRENDS
            // =========================
            var txDaily = await _db.BankTransactions
                .Where(t => t.CreatedDate >= last7)
                .GroupBy(t => new DateTime(t.CreatedDate.Year, t.CreatedDate.Month, t.CreatedDate.Day))
                .Select(g => new DailyTxTrendDto
                {
                    DayUtc = g.Key,
                    Count = g.Count(),
                    Volume = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.DayUtc)
                .ToListAsync();

            // New users trend (CreatedDate yoxdur -> 0)
            var newUsersTrend = Enumerable.Range(0, 7)
                .Select(i => new DailyCountTrendDto
                {
                    DayUtc = new DateTime(last7.AddDays(i).Year, last7.AddDays(i).Month, last7.AddDays(i).Day),
                    Count = 0
                })
                .ToList();

            // =========================
            // LATEST LISTS
            // =========================
            var latestTx = await _db.BankTransactions
                .OrderByDescending(t => t.CreatedDate)
                .Take(10)
                .Select(t => new LatestTransactionDto
                {
                    Id = t.Id,
                    ReferenceNo = t.ReferenceNo,
                    SenderWalletId = t.SenderWalletId,
                    ReceiverWalletId = t.ReceiverWalletId,
                    Amount = t.Amount,
                    FeeAmount = t.FeeAmount,
                    Type = (int)t.Type,
                    Status = (int)t.Status,
                    Description = t.Description,
                    CreatedDate = t.CreatedDate
                })
                .ToListAsync();

            var latestAudits = await _db.AuditLogs
                .OrderByDescending(a => a.CreatedDate)
                .Take(15)
                .Select(a => new LatestAuditDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    ActionType = (int)a.ActionType,
                    IsSuccess = a.IsSuccess,
                    Description = a.Description,
                    IpAddress = a.IpAddress,
                    CorrelationId = a.CorrelationId,
                    CreatedDate = a.CreatedDate
                })
                .ToListAsync();

            var latestNotifs = await _db.Notifications
                .OrderByDescending(n => n.CreatedDate)
                .Take(10)
                .Select(n => new LatestNotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Title = n.Title,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedDate
                })
                .ToListAsync();

            // =========================
            // TOP ACTIVE WALLETS (Last 7 days)
            // Activity = sent + received count, volume = sent + received amount
            // =========================
            var txLast7 = _db.BankTransactions.Where(t => t.CreatedDate >= last7);

            var sentAgg = txLast7.Where(t => t.SenderWalletId != null)
                .GroupBy(t => t.SenderWalletId!.Value)
                .Select(g => new
                {
                    WalletId = g.Key,
                    SentCount = g.Count(),
                    SentVolume = g.Sum(x => x.Amount)
                });

            var recvAgg = txLast7.Where(t => t.ReceiverWalletId != null)
                .GroupBy(t => t.ReceiverWalletId!.Value)
                .Select(g => new
                {
                    WalletId = g.Key,
                    RecvCount = g.Count(),
                    RecvVolume = g.Sum(x => x.Amount)
                });

            var topActiveWallets = await _db.Wallets
                .Select(w => new
                {
                    w.Id,
                    w.UserId,
                    w.Balance,
                    Sent = sentAgg.FirstOrDefault(s => s.WalletId == w.Id),
                    Recv = recvAgg.FirstOrDefault(r => r.WalletId == w.Id)
                })
                .AsNoTracking()
                .ToListAsync();

            var topActiveWalletDtos = topActiveWallets
                .Select(x =>
                {
                    var sentCount = x.Sent?.SentCount ?? 0;
                    var recvCount = x.Recv?.RecvCount ?? 0;
                    var sentVol = x.Sent?.SentVolume ?? 0m;
                    var recvVol = x.Recv?.RecvVolume ?? 0m;

                    return new TopActiveWalletDto
                    {
                        WalletId = x.Id,
                        UserId = x.UserId,
                        Balance = x.Balance,
                        TxCountLast7Days = sentCount + recvCount,
                        VolumeLast7Days = sentVol + recvVol
                    };
                })
                .OrderByDescending(x => x.TxCountLast7Days)
                .ThenByDescending(x => x.VolumeLast7Days)
                .Take(10)
                .ToList();

            // =========================
            // BUILD RESPONSE
            // =========================
            var dto = new AdminDashboardDto
            {
                Kpis = new DashboardKpisDto
                {
                    Users = new UsersKpiDto
                    {
                        Total = totalUsers,
                        Locked = lockedUsers,
                        EmailConfirmed = confirmedUsers,
                        EmailConfirmedRate = confirmedRate,
                        NewLast7Days = newUsersLast7
                    },
                    Wallets = new WalletsKpiDto
                    {
                        TotalWallets = totalWallets,
                        TotalBalance = totalBalance,
                        AvgBalance = avgBalance,
                        StatusBreakdown = new WalletStatusBreakdownDto
                        {
                            Active = walletActive,
                            Blocked = walletBlocked,
                            Closed = walletClosed
                        }
                    },
                    Transactions = new TransactionsKpiDto
                    {
                        Total = totalTx,
                        Last24HoursCount = last24TxCount,
                        Last30DaysVolume = txLast30Volume,
                        Last30DaysSuccessRate = txLast30SuccessRate,
                        FailedLast24Hours = failedLast24,
                        StatusBreakdownLast30Days = new TransactionStatusBreakdownDto
                        {
                            Pending = txLast30Pending,
                            Completed = txLast30Completed,
                            Failed = txLast30Failed,
                            Reversed = txLast30Reversed
                        }
                    },
                    Notifications = new NotificationsKpiDto
                    {
                        Total = totalNotif,
                        Unread = unreadNotif,
                        Last7Days = last7Notif,
                        TypeBreakdownLast7Days = new NotificationTypeBreakdownDto
                        {
                            IncomingTransfer = notifIncoming,
                            OutgoingTransfer = notifOutgoing,
                            System = notifSystem,
                            Security = notifSecurity
                        }
                    },
                    Chat = new ChatKpiDto
                    {
                        MessagesLast24Hours = chatLast24,
                        UnreadLast24Hours = chatUnreadLast24
                    },
                    Audit = new AuditKpiDto
                    {
                        FailedLast24Hours = auditFailedLast24
                    }
                },
                Trends = new DashboardTrendsDto
                {
                    TxDailyLast7Days = txDaily,
                    NewUsersLast7Days = newUsersTrend
                },
                Latest = new DashboardLatestDto
                {
                    Transactions = latestTx,
                    Audits = latestAudits,
                    Notifications = latestNotifs,
                    TopActiveWalletsLast7Days = topActiveWalletDtos
                }
            };

            return ServiceResult<AdminDashboardDto>.Ok(dto);
        }
    }
}

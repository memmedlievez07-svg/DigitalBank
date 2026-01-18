using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class AdminNotificationService : IAdminNotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationPushService _push;

        public AdminNotificationService(
            IUnitOfWork uow,
            UserManager<AppUser> userManager,
            INotificationPushService push)
        {
            _uow = uow;
            _userManager = userManager;
            _push = push;
        }

        public async Task<ServiceResultVoid> SendToAllAsync(AdminSendNotificationDto dto)
        {
            var userIds = await _userManager.Users
                .AsNoTracking()
                .Select(u => u.Id)
                .ToListAsync();

            // çox user varsa AddRangeAsync istifadə et
            var notifications = userIds.Select(uid => new Notification
            {
                UserId = uid,
                Title = dto.Title,
                Body = dto.Body,
                Type = dto.Type,
                IsRead = false,
                RelatedTransactionId = dto.RelatedTransactionId
            }).ToList();

            await _uow.NotificationWriteRepository.AddRangeAsync(notifications);
            await _uow.CommitAsync();

            // 🔔 real-time push (DB uğurlu olandan sonra)
            foreach (var uid in userIds)
            {
                await _push.PushToUserAsync(uid, new
                {
                    title = dto.Title,
                    body = dto.Body,
                    type = (int)dto.Type
                });

                await _push.PushUnreadCountChangedAsync(uid);
            }

            return ServiceResultVoid.Ok("Broadcast sent");
        }

        public async Task<ServiceResultVoid> SendToUserAsync(AdminSendToUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return ServiceResultVoid.Fail("User not found", 404);

            var n = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Body = dto.Body,
                Type = dto.Type,
                IsRead = false,
                RelatedTransactionId = dto.RelatedTransactionId
            };

            await _uow.NotificationWriteRepository.AddAsync(n);
            await _uow.CommitAsync();

            await _push.PushToUserAsync(dto.UserId, new
            {
                title = dto.Title,
                body = dto.Body,
                type = (int)dto.Type
            });

            await _push.PushUnreadCountChangedAsync(dto.UserId);

            return ServiceResultVoid.Ok("Notification sent");
        }
    }
}

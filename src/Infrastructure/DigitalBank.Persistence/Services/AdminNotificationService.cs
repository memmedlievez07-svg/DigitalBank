using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Entities.Identity;
using DigitalBank.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Persistence.Services
{
    public class AdminNotificationService : IAdminNotificationService
    {
        private readonly IUnitOfWork _uow; // Bazaya yazmaq üçün
        private readonly UserManager<AppUser> _userManager; // İstifadəçiləri tapmaq üçün

        public AdminNotificationService(IUnitOfWork uow, UserManager<AppUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        public async Task<ServiceResultVoid> SendNotificationToAllAsync(string title, string text)
        {
            // 1. Bütün istifadəçiləri bazadan çəkirik
            var users = await _userManager.Users.ToListAsync();

            if (users == null || !users.Any())
                return ServiceResultVoid.Fail("Sistemdə istifadəçi tapılmadı", 404);

            foreach (var user in users)
            {
                // 2. Yeni obyekt yaradırıq - Sənin entity adlarına tam uyğun!
                var notification = new Notification
                {
                    UserId = user.Id,
                    Title = title,
                    Body = text, // Səndə 'Body' adlanır, ona görə 'text' bura mənimsədilir
                    Type = NotificationType.System,
                    IsRead = false,
                    // CreatedDate BaseEntity-dən gəlirsə, bəlkə orada avtomatik set olunur
                    // Əgər yoxdursa, bura 'CreatedDate = DateTime.UtcNow' əlavə edə bilərsən
                };

                // 3. Repository vasitəsilə əlavə edirik
                await _uow.NotificationWriteRepository.AddAsync(notification);
            }

            // 4. Bütün dövr bitəndən sonra bazaya yazırıq
            await _uow.SaveChangesAsync();

            return ServiceResultVoid.Ok("Bildiriş hamıya göndərildi.");
        }
    }
}

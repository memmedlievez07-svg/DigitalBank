using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Interfaces
{
    public interface IAdminNotificationService
    {
        // Bütün istifadəçilərə bildiriş göndərmək üçün metod
        Task<ServiceResultVoid> SendToAllAsync(AdminSendNotificationDto dto);
        Task<ServiceResultVoid> SendToUserAsync(AdminSendToUserDto dto);
    }
}

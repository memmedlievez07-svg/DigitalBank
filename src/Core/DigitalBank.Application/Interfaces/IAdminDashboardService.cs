using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Results;

namespace DigitalBank.Application.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<ServiceResult<AdminDashboardDto>> GetAsync();
    }
}

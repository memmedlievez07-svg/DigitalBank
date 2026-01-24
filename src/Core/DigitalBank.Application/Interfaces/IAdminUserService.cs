using DigitalBank.Application.Dtos;
using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Results;

namespace DigitalBank.Application.Interfaces
{
    public interface IAdminUserService
    {
        Task<ServiceResult<List<AdminUserListItemDto>>> GetUsersAsync();
        Task<ServiceResultVoid> LockUserAsync(string userId);
        Task<ServiceResultVoid> UnlockUserAsync(string userId);
        Task<ServiceResultVoid> SetRoleAsync(string userId, SetUserRoleRequestDto dto);

        Task<ServiceResult<List<UserLookUpDto>>>SearchUsersByEmailAsync(string email);
    }
}

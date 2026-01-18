using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Results;
namespace DigitalBank.Application.Interfaces
{


    public interface IAdminTransactionService
    {
        Task<ServiceResult<PagedResult<AdminTransactionListItemDto>>> ListAsync(AdminTransactionFilterDto filter);
        Task<ServiceResult<AdminTransactionDetailsDto>> GetByIdAsync(int id);
    }

}

using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Results;
namespace DigitalBank.Application.Interfaces
{
    
    
        public interface IAdminTransactionService
        {
            Task<ServiceResult<object>> ListAsync(AdminTransactionFilterDto filter);
            Task<ServiceResult<object>> GetByIdAsync(int id);
        }
    
}

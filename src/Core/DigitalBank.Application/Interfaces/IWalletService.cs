using DigitalBank.Application.Dtos;
using DigitalBank.Application.Results;

namespace DigitalBank.Application.Interfaces
{
    public interface IWalletService
    {
        Task<ServiceResult<WalletMeDto>> GetMyWalletAsync();
    }
}

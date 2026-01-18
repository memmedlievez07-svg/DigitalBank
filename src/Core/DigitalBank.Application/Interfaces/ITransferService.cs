using DigitalBank.Application.Dtos;
using DigitalBank.Application.Results;

namespace DigitalBank.Application.Interfaces
{
    public interface ITransferService
    {
        Task<ServiceResultVoid> TransferAsync(TransferRequestDto dto);
    }
}

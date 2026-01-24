using DigitalBank.Application.Dtos;
using DigitalBank.Application.Results;

namespace DigitalBank.Application.Interfaces
{
    public interface ITransferService
    {
        Task<ServiceResultVoid> TransferAsync(TransferRequestDto dto);

        Task<ServiceResult<List<RecentTransferDto>>> GetRecentTransfersAsync();
        Task<ServiceResult<DashboardDto>> GetDashboardDataAsync();
        Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionHistoryAsync();
    }
}

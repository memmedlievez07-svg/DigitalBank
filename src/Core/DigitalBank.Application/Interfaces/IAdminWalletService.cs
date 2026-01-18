using DigitalBank.Application.Dtos.AdminDashBoardDtos.Wallet;
using DigitalBank.Application.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Interfaces
{
    public interface IAdminWalletService
    {

        Task<ServiceResult<PagedResult<AdminWalletListItemDto>>> ListAsync(AdminWalletFilterDto filter);
        Task<ServiceResultVoid> UpdateStatusAsync(int walletId, UpdateWalletStatusDto dto);
    }
}

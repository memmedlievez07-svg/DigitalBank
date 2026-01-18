using DigitalBank.Domain.Enums;
namespace DigitalBank.Application.Dtos.AdminDashBoardDtos.Wallet
{
    public class UpdateWalletStatusDto
    {
        public WalletStatus Status { get; set; } // Active/Blocked/Closed
    }
}

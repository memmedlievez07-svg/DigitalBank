using DigitalBank.Domain.Enums;

namespace DigitalBank.Application.Dtos.AdminDashBoardDtos.Wallet
{
    public class AdminWalletListItemDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string CardNumber { get; set; } = null!;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = null!;
        public WalletStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

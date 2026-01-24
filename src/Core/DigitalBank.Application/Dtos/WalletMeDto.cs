using DigitalBank.Domain.Enums;

namespace DigitalBank.Application.Dtos
{
    public class WalletMeDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = null!;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = null!;
        public int Status { get; set; }
        public List<TransactionDetailDto> Transactions { get; set; } = new();
    }
}

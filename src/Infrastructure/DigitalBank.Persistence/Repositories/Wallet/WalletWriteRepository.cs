using DigitalBank.Application.Repositories.Wallet;
using DigitalBank.Persistence.Dal;

namespace DigitalBank.Persistence.Repositories.Wallet
{
    public class WalletWriteRepository :WriteRepository<DigitalBank.Domain.Entities.Wallet>, IWalletWriteRepository
    {
        public WalletWriteRepository(DigitalBankDbContext context) : base(context)  { }
    }
}

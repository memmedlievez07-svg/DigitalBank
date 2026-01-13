using DigitalBank.Application.Repositories.Wallet;
using DigitalBank.Persistence.Dal;

namespace DigitalBank.Persistence.Repositories.Wallet
{
    public class WalletReadRepository : ReadRepository<DigitalBank.Domain.Entities.Wallet>,IWalletReadRepository
    {
        public WalletReadRepository(DigitalBankDbContext context):base(context) { }
      
    }
}

using DigitalBank.Application.Repositories.BankTransaction;
using DigitalBank.Persistence.Dal;

namespace DigitalBank.Persistence.Repositories.BankTransaction
{
    public class BankTransactionWriteRepository : WriteRepository<DigitalBank.Domain.Entities.BankTransaction>,IBankTransactionWriteRepository
    {
        public BankTransactionWriteRepository(DigitalBankDbContext context) :base (context) { }
     
    }
}

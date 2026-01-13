using DigitalBank.Application.Repositories.BankTransaction;
using DigitalBank.Persistence.Dal;
namespace DigitalBank.Persistence.Repositories.BankTransaction
{
    public class BankTransactionReadRepository :ReadRepository<DigitalBank.Domain.Entities.BankTransaction>,IBankTransactionReadRepository
    {
        public BankTransactionReadRepository(DigitalBankDbContext context):base(context) { }
    }
}

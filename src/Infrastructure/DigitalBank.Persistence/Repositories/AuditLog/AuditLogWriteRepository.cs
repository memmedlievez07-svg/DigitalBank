using DigitalBank.Application.Repositories.AuditLog;
using DigitalBank.Persistence.Dal;
namespace DigitalBank.Persistence.Repositories.AuditLog
{
    public class AuditLogWriteRepository : WriteRepository<DigitalBank.Domain.Entities.AuditLog>,IAuditLogWriteRepository
    {
        public AuditLogWriteRepository(DigitalBankDbContext context) : base(context) { }
    }
}

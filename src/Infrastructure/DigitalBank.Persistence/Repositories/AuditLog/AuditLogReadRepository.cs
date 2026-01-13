using DigitalBank.Application.Repositories.AuditLog;
using DigitalBank.Persistence.Dal;

namespace DigitalBank.Persistence.Repositories.AuditLog
{
    public class AuditLogReadRepository :ReadRepository<DigitalBank.Domain.Entities.AuditLog>, IAuditLogReadRepository
    {
        public AuditLogReadRepository(DigitalBankDbContext context) : base(context) { }
    }
}

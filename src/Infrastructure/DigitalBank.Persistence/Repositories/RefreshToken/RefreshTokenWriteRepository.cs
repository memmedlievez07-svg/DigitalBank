using DigitalBank.Application.Repositories.RefreshToken;
using DigitalBank.Persistence.Dal;

namespace DigitalBank.Persistence.Repositories.RefreshToken
{
    public class RefreshTokenWriteRepository : WriteRepository<DigitalBank.Domain.Entities.RefreshToken>, IRefreshTokenWriteRepository
    {
        public RefreshTokenWriteRepository(DigitalBankDbContext context) :base (context) { }
    }
}

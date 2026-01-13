using DigitalBank.Application.Repositories.RefreshToken;
using DigitalBank.Persistence.Dal;

namespace DigitalBank.Persistence.Repositories.RefreshToken
{
    public class RefreshTokenReadRepository : ReadRepository<DigitalBank.Domain.Entities.RefreshToken>,IRefreshTokenReadRepository
    {
        public RefreshTokenReadRepository(DigitalBankDbContext context):base (context) { }
      
    }
}

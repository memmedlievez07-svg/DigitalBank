using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserContext _current;

        public WalletService(IUnitOfWork uow, ICurrentUserContext current)
        {
            _uow = uow;
            _current = current;
        }

        public async Task<ServiceResult<WalletMeDto>> GetMyWalletAsync()
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<WalletMeDto>.Fail("Unauthorized", 401);

            var userId = _current.UserId!;

            var w = await _uow.WalletReadRepository.Table
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new WalletMeDto
                {
                    Id = x.Id,
                    CardNumber = x.CardNumber,
                    Balance = x.Balance,
                    Currency = x.Currency,
                    Status = x.Status
                })
                .FirstOrDefaultAsync();

            if (w == null)
                return ServiceResult<WalletMeDto>.Fail("Wallet not found", 404);

            return ServiceResult<WalletMeDto>.Ok(w);
        }
    }
}

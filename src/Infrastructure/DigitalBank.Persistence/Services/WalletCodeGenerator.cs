using DigitalBank.Application.Interfaces;
using DigitalBank.Application.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DigitalBank.Persistence.Services
{
    public class WalletCodeGenerator : IWalletCodeGenerator
    {
        private readonly IUnitOfWork _uow;

        public WalletCodeGenerator(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<string> GenerateUniqueCardNumberAsync()
        {
            while (true)
            {
                var number = Generate16Digits();

                var exists = await _uow.WalletReadRepository.Table
                    .AsNoTracking()
                    .AnyAsync(w => w.CardNumber == number);

                if (!exists)
                    return number;
            }
        }

        private static string Generate16Digits()
        {
            Span<byte> bytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(bytes);

            var chars = new char[16];
            for (int i = 0; i < 16; i++)
                chars[i] = (char)('0' + (bytes[i] % 10));

            if (chars[0] == '0') chars[0] = '1';

            return new string(chars);
        }
    }
}

using DigitalBank.Application.Repositories;
using DigitalBank.Domain.Entities.Base;
using DigitalBank.Persistence.Dal;
using Microsoft.EntityFrameworkCore;
namespace DigitalBank.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly DigitalBankDbContext _context;

        public GenericRepository(DigitalBankDbContext context)
        {
            _context = context;
        }

        public DbSet<T> Table => _context.Set<T>();
    }
}


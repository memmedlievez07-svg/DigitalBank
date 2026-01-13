using DigitalBank.Application.Repositories;
using DigitalBank.Domain.Entities.Base;
using DigitalBank.Persistence.Dal;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace DigitalBank.Persistence.Repositories
{
    public class ReadRepository<T> : IReadRepository<T> where T : BaseEntity
    {
        public DbSet<T> Table => _context.Set<T>();
        private readonly DigitalBankDbContext _context;
        public ReadRepository(DigitalBankDbContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync(int id, bool tracking = true)
        {
            return await Table.ApplyIncludesAndTracking(tracking).FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<T?> GetByIdAsync(int id, bool tracking = true, params Expression<Func<T, object>>[] includes)
        {
            return await Table.ApplyIncludesAndTracking(tracking, includes).FirstOrDefaultAsync(x => x.Id == id);
        }

        public IQueryable<T> GetAll(bool tracking) => Table.ApplyIncludesAndTracking(tracking);

        public IQueryable<T> GetAll(bool tracking, params Expression<Func<T, object>>[] includes) => Table.ApplyIncludesAndTracking(tracking, includes);

        public IQueryable<T> GetWhere(bool tracking, Expression<Func<T, bool>> expression)
            => Table.Where(expression).ApplyIncludesAndTracking(tracking);

        public IQueryable<T> GetWhere(bool tracking, Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
            => Table.Where(expression).ApplyIncludesAndTracking(tracking, includes);


    }
}

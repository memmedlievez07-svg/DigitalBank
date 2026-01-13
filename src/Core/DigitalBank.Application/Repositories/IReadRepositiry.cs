using DigitalBank.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace DigitalBank.Application.Repositories

{
    public interface IReadRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        IQueryable<T> GetAll(bool tracking);
        IQueryable<T> GetAll(bool tracking, params Expression<Func<T, object>>[] includes);
        IQueryable<T> GetWhere(bool tracking, Expression<Func<T, bool>> expression);
        IQueryable<T> GetWhere(bool tracking, Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
        public Task<T?> GetByIdAsync(int id, bool tracking = true);
        public Task<T?> GetByIdAsync(int id, bool tracking = true, params Expression<Func<T, object>>[] includes);
    }
}

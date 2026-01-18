using DigitalBank.Domain.Entities.Base;

namespace DigitalBank.Application.Repositories
{
    public interface IWriteRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void Remove(T entity);
       
    }
}


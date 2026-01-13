using DigitalBank.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
namespace DigitalBank.Application.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        public DbSet<T> Table { get; }
    }
}

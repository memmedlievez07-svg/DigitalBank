using DigitalBank.Application.Repositories;
using DigitalBank.Domain.Entities.Base;
using DigitalBank.Persistence.Dal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Persistence.Repositories
{
    public class WriteRepository<T> : IWriteRepository<T> where T : BaseEntity
    {

        private readonly DigitalBankDbContext _ctx;
        public DbSet<T> Table => _ctx.Set<T>();
        public WriteRepository(DigitalBankDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await Table.AddAsync(entity,ct);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            await Table.AddRangeAsync(entities,ct);
        }


        public void Remove(T entity)
        {
            Table.Remove(entity);
        }

        public void Update(T entity)
        {
            Table.Update(entity);
        }
        public void UpdateRange(List<T> entities)
        {
            Table.UpdateRange(entities);
        }


    }
}

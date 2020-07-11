using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DevSilenceKeeperBot.DAL;
using Microsoft.EntityFrameworkCore;

namespace DevSilenceKeeperBot.DLL.Repositories
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly BotDbContext _context;

        protected Repository(BotDbContext context)
        {
            _context = context;
        }


        public ValueTask<TEntity> GetByIdAsync(int id)
        {
            return _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Where(predicate);
        }

        public Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().SingleOrDefaultAsync(predicate);
        }

        public async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _context.Set<TEntity>().AddRangeAsync(entities);
        }

        public void Remove(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().RemoveRange(entities);
        }
    }
}
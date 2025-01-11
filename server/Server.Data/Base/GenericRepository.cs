using Microsoft.EntityFrameworkCore;
using Server.Data.Entities;
using System.Linq.Expressions;

namespace Server.Data.Base
{
    public class GenericRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
    {
        private readonly AppDbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        public IQueryable<TEntity> GetAll()
            => _dbSet.AsNoTracking();

        public IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> predicate)
            => _dbSet.AsNoTracking().Where(predicate);

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate) => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);

        public async Task<TEntity?> GetByIdAsync(TKey id)
            => await _dbSet.FindAsync(id);
        public async Task<TEntity?> GetByIdCompositeKeyAsync(TKey id1, TKey id2)
            => await _dbSet.FindAsync(id1, id2);
        public async Task<TEntity> AddAsync(TEntity entity)
        {
            var entityEntry = await _dbContext.Set<TEntity>().AddAsync(entity);
            return entityEntry.Entity;
        }

        public TEntity Update(TEntity entity)
        {
            var trackedEntity = _dbContext.Set<TEntity>().Local.FirstOrDefault(e => e == entity);
            if (trackedEntity != null)
            {
                // Nếu thực thể đã được theo dõi, tách thực thể trước khi cập nhật
                _dbContext.Entry(trackedEntity).State = EntityState.Detached;
            }

            // Tiếp tục cập nhật thực thể
            var tracker = _dbContext.Attach(entity);
            tracker.State = EntityState.Modified;
            return entity;
        }



        public TEntity Remove(TKey id)
        {
            var entity = GetByIdAsync(id).Result;
            var entityEntry = _dbContext.Set<TEntity>().Remove(entity!);
            return entityEntry.Entity;
        }
        public TEntity RemoveCompositeKey(TKey id1, TKey id2)
        {
            var entity = GetByIdCompositeKeyAsync(id1, id2).Result;
            var entityEntry = _dbContext.Set<TEntity>().Remove(entity!);
            return entityEntry.Entity;
        }

        public Task<int> Commit() => _dbContext.SaveChangesAsync();

        public async Task<int> CountAsync()
        {
            var count = await _dbSet.CountAsync();
            return count;
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var count = await _dbSet.CountAsync(predicate);
            return count;
        }

        public async Task<IEnumerable<TEntity>> GetTopNItems<TKeyProperty>(Expression<Func<TEntity, TKeyProperty>> keySelector, int n)
        {
            var items = await _dbSet.OrderBy(keySelector).Take(n).ToListAsync();
            return items;
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbContext.Set<TEntity>().AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }
        
        
        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }



        public async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().RemoveRange(entities);
            await _dbContext.SaveChangesAsync();
        }
    }
}

using System.Linq.Expressions;

namespace Server.Data.Base
{
    public interface IRepository<TEntity, in TKey>
    {
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<TEntity?> GetByIdCompositeKeyAsync(TKey id1, TKey id2);
        Task<TEntity> AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        TEntity Update(TEntity entity);
        TEntity Remove(TKey id);
        public TEntity RemoveCompositeKey(TKey id1, TKey id2);
        Task<int> Commit();
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetTopNItems<TKeyProperty>(Expression<Func<TEntity, TKeyProperty>> keySelector, int n);
    }
}

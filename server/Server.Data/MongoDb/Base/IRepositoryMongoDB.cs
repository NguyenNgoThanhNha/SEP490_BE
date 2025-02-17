namespace Server.Data.MongoDb.Repository;

public interface IRepositoryMongoDB<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task RemoveAsync(string id);
}
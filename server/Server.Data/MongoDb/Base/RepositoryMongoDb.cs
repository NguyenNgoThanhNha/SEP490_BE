using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Business.Ultils;

namespace Server.Data.MongoDb.Repository;

public class RepositoryMongoDb<T> : IRepositoryMongoDB<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public RepositoryMongoDb(IOptions<MongoDbSetting> mongoDbSetting)
    {
        // Connect to MongoDB
        var client = new MongoClient(mongoDbSetting.Value.ConnectionString);
        
        // Get database
        var database = client.GetDatabase(mongoDbSetting.Value.DatabaseName);
        
        // Get collection
        _collection = database.GetCollection<T>(typeof(T).Name);
    }
    
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        var objectId = new ObjectId(id);
        return await _collection.Find(Builders<T>.Filter.Eq("_id", objectId)).FirstOrDefaultAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        // Thêm một document mới vào collection
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(string id, T entity)
    {
        var objectId = new ObjectId(id);
        await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", objectId), entity);
    }

    public async Task RemoveAsync(string id)
    {
        // Xóa document theo ID
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        await _collection.DeleteOneAsync(filter);
    }
}
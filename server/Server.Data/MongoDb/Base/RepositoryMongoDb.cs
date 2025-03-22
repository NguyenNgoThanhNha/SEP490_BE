using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Business.Ultils;
using Server.Data.MongoDb.Models;

namespace Server.Data.MongoDb.Repository;

public class RepositoryMongoDb<T> : IRepositoryMongoDB<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public RepositoryMongoDb(IOptions<MongoDbSetting> mongoDbSetting)
    {
        var client = new MongoClient(mongoDbSetting.Value.ConnectionString);
        var database = client.GetDatabase(mongoDbSetting.Value.DatabaseName);
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
    
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _collection.Find(Builders<T>.Filter.Eq("userId", id)).FirstOrDefaultAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
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
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        await _collection.DeleteOneAsync(filter);
    }
    
    public async Task RemoveAllAsync()
    {
        await _collection.DeleteManyAsync(FilterDefinition<T>.Empty);
    }

    public async Task<IEnumerable<T>> FindByFieldAsync(string fieldName, object value)
    {
        var filter = Builders<T>.Filter.Eq(fieldName, value);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<T>> FindByConditionAsync(FilterDefinition<T> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }
    
    public async Task<List<T>> GetByUserIdsAsync(List<int> userIds)
    {
        var filter = Builders<T>.Filter.In("userId", userIds);
        return await _collection.Find(filter).ToListAsync();
    }
    
    public async Task<List<T>> GetByMembersAsync(List<string> memberIds)
    {
        var filter = Builders<T>.Filter.AnyIn("Members", memberIds);
        return await _collection.Find(filter).ToListAsync();
    }
    
    public async Task<List<T>> GetMessagesByChannelIdAsync(string channelId)
    {
        var filter = Builders<T>.Filter.Eq("ChannelId", channelId);
        return await _collection.Find(filter).SortBy(m => m.GetType().GetProperty("Timestamp").GetValue(m, null)).ToListAsync();
    }
    
    public async Task<List<T>> SearchByRegexAsync(string field, string searchTerm)
    {
        var regexFilter = Builders<T>.Filter.Regex(field, new BsonRegularExpression(searchTerm, "i"));
        return await _collection.Find(regexFilter).ToListAsync();
    }
    
    public async Task<List<T>> GetManyAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }
    
    public async Task<List<T>> GetManyByIdsAsync(List<string> ids)
    {
        var filter = Builders<T>.Filter.In("_id", ids.Select(id => ObjectId.Parse(id)));
        return await _collection.Find(filter).ToListAsync();
    }



}
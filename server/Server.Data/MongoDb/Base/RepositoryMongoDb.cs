using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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

    /*public async Task<List<T>> GetMessagesByIdsAsync(List<string> messageIds)
    {
        // Chuyển danh sách ID từ string sang ObjectId (nếu cần)
        var objectIds = messageIds.Select(id => new ObjectId(id)).ToList();

        var filter = Builders<T>.Filter.In("_id", objectIds);

        return await _collection.Find(filter)
            .SortByDescending(m => ((Messages)(object)m).Timestamp) // Ép kiểu an toàn
            .ToListAsync();
    }*/

    public async Task<List<MessageDTO>> GetMessagesByIdsAsync(List<string> messageIds)
    {
        var objectIds = messageIds.Select(id => new ObjectId(id)).ToList();
        var filter = Builders<T>.Filter.In("_id", objectIds);

        var listObjects = await _collection
            .Aggregate()
            .Match(filter)
            .Lookup("Customers", "sender", "_id", "SenderCustomer")
            .Lookup("Customers", "recipient", "_id", "RecipientCustomer")
            .SortByDescending(m => m["timestamp"])
            .ToListAsync();

        var messages = listObjects.Select(doc => new MessageDTO
        {
            Id = doc["_id"].ToString(),
            Sender = doc["sender"].ToString(),
            Recipient = doc["recipient"].IsBsonNull ? null : doc["recipient"].ToString(),
            MessageType = doc["messageType"].ToString(),
            Content = doc["content"].ToString(),
            FileUrl = doc["fileUrl"].IsBsonNull ? null : doc["fileUrl"].ToString(),
            Timestamp = doc["timestamp"].ToUniversalTime(),
            SenderCustomer = doc["SenderCustomer"].AsBsonArray
                .Select(c => new CustomerDTO
                {
                    Id = c["_id"].ToString() ?? null,
                    Email = c["email"].ToString() ?? null,
                    FullName = c["fullName"].ToString() ?? null,
                    Password = c["password"].ToString() ?? null,
                    UserId = c["userId"].ToString() ?? null,
                    Image = c["image"].ToString() ?? null
                }).FirstOrDefault(),
            RecipientCustomer = doc["RecipientCustomer"].AsBsonArray
                .Select(c => new CustomerDTO
                {
                    Id = c["_id"].ToString() ?? null,
                    Email = c["email"].ToString() ?? null,
                    FullName = c["fullName"].ToString() ?? null,
                    Password = c["password"].ToString() ?? null,
                    UserId = c["userId"].ToString() ?? null,
                    Image = c["image"].ToString() ?? null
                }).FirstOrDefault()
        }).ToList();

        return messages;
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

    public async Task<T> GetOneAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<T>> GetManyByIdsAsync(List<string> ids)
    {
        var filter = Builders<T>.Filter.In("_id", ids.Select(id => ObjectId.Parse(id)));
        return await _collection.Find(filter).ToListAsync();
    }
    
    public async Task<ChannelsDTO> GetChannelByIdAsync(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);

        var channelDoc = await _collection
            .Aggregate()
            .Match(filter)
            .Lookup("Customers", "admin", "_id", "AdminDetails")
            .Lookup("Customers", "members", "_id", "MemberDetails")
            .FirstOrDefaultAsync();

        if (channelDoc == null)
            return null;

        var channel = new ChannelsDTO
        {
            Id = channelDoc["_id"].ToString(),
            Name = channelDoc["name"].ToString(),
            AppointmentId = channelDoc["appointmentId"].AsInt32,
            Members = channelDoc["members"].AsBsonArray.Select(m => m.ToString()).ToList(),
            Messages = channelDoc["messages"].AsBsonArray.Select(m => m.ToString()).ToList(),
            CreateAt = channelDoc["createAt"].ToUniversalTime(),
            UpdateAt = channelDoc["updateAt"].ToUniversalTime(),
            Admin = channelDoc["admin"].ToString(),
            AdminDetails = channelDoc["AdminDetails"].AsBsonArray.Select(c => new CustomerDTO
            {
                Id = c["_id"].ToString(),
                Email = c["email"].ToString(),
                FullName = c["fullName"].ToString(),
                Password = c["password"].ToString(),
                UserId = c["userId"].ToString(),
                Image = c["image"].ToString()
            }).FirstOrDefault(),
            MemberDetails = channelDoc["MemberDetails"].AsBsonArray.Select(c => new CustomerDTO
            {
                Id = c["_id"].ToString(),
                Email = c["email"].ToString(),
                FullName = c["fullName"].ToString(),
                Password = c["password"].ToString(),
                UserId = c["userId"].ToString(),
                Image = c["image"].ToString()
            }).ToList()
        };

        return channel;
    }
}
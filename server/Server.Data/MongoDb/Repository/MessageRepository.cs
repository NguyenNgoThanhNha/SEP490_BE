using Microsoft.Extensions.Options;
using Server.Business.Ultils;
using Server.Data.MongoDb.Models;

namespace Server.Data.MongoDb.Repository;

public class MessageRepository : RepositoryMongoDb<Messages>
{
    public MessageRepository(IOptions<MongoDbSetting> mongoDbSetting) : base(mongoDbSetting)
    {
    }
}
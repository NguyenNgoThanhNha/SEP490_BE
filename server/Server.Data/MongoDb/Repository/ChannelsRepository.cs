using Microsoft.Extensions.Options;
using Server.Business.Ultils;
using Server.Data.MongoDb.Models;

namespace Server.Data.MongoDb.Repository;

public class ChannelsRepository : RepositoryMongoDb<Channels>
{
    public ChannelsRepository(IOptions<MongoDbSetting> mongoDbSetting) : base(mongoDbSetting)
    {
    }
}
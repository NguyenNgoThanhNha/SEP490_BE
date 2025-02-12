using Microsoft.Extensions.Options;
using Server.Business.Ultils;
using Server.Data.MongoDb.Models;

namespace Server.Data.MongoDb.Repository;

public class CustomerRepository : RepositoryMongoDb<Customers>
{
    public CustomerRepository(IOptions<MongoDbSetting> mongoDbSetting) : base(mongoDbSetting)
    {
    }
}
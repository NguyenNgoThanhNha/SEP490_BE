using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class LoggerRepository : GenericRepository<Logger, int>
{
    public LoggerRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class NotificationRepository : GenericRepository<Notifications, int>
{
    public NotificationRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
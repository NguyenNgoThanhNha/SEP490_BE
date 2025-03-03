using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class FeedbackServiceRepository : GenericRepository<ServiceFeedback, int>
{
    public FeedbackServiceRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class FeedbackAppointmentRepository : GenericRepository<AppointmentFeedback, int>
{
    public FeedbackAppointmentRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
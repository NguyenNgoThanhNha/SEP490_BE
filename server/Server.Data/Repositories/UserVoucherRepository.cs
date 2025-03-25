using Server.Data.Base;
using Server.Data.Entities;

namespace Server.Data.Repositories;

public class UserVoucherRepository : GenericRepository<UserVoucher, int>
{
    public UserVoucherRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
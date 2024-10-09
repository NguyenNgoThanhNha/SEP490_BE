using Server.Data.Entities;
using Server.Data.Repositories;

namespace Server.Data.UnitOfWorks
{
    public class UnitOfWorks
    {
        private readonly AppDbContext _dbContext;
        private UserRepository _userRepo;


        public UnitOfWorks(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public UserRepository UserRepository
        {
            get { return _userRepo ??= new UserRepository(_dbContext); }
        }
    }
}

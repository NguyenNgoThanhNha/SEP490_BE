using Server.Data.Entities;
using Server.Data.Repositories;

namespace Server.Data.UnitOfWorks
{
    public class UnitOfWorks
    {
        private readonly AppDbContext _dbContext;
        private UserRepository _userRepo;
        private AuthRepository _authRepo;
        private UserRoleRepository _userRoleRepo;
        private ServiceRepository _serviceRepo;
        private PromotionRepository _promotionRepo;
        private BranchPromotionRepository _branchpromotionRepo;
        private BranchRepository _branchRepo;
        private ProductRepository _productRepo;
        private CategoryRepository _categoryRepo;

        public UnitOfWorks(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public UserRepository UserRepository
        {
            get { return _userRepo ??= new UserRepository(_dbContext); }
        }
        
        public AuthRepository AuthRepository
        {
            get { return _authRepo ??= new AuthRepository(_dbContext); }
        }
        
        public UserRoleRepository UserRoleRepository
        {
            get { return _userRoleRepo ??= new UserRoleRepository(_dbContext); }
        }

        public ServiceRepository ServiceRepository
        {
            get { return _serviceRepo ??= new ServiceRepository(_dbContext); }
        }

        public ProductRepository ProductRepository
        {
            get { return _productRepo ??= new ProductRepository(_dbContext); }
        }

        public CategoryRepository CategoryRepository
        {
            get { return _categoryRepo ??= new CategoryRepository(_dbContext); }
        }
    }
}

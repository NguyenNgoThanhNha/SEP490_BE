using Microsoft.EntityFrameworkCore;
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
        private AppointmentsRepository _apointmentRepo;
        private StaffRepository _staffRepo;
        private BlogRepository _blogRepo;
        private Brand_ProductRepository _branchProductRepository;

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

        public PromotionRepository PromotionRepository
        {
            get { return _promotionRepo ??= new PromotionRepository(_dbContext); }
        }

        public BranchPromotionRepository BranchPromotionRepository
        {
            get { return _branchpromotionRepo ??= new BranchPromotionRepository(_dbContext); }
        }

        public BranchRepository BranchRepository
        {
            get { return _branchRepo ??= new BranchRepository(_dbContext); }
        }
        
        public AppointmentsRepository AppointmentsRepository
        {
            get { return _apointmentRepo ??= new AppointmentsRepository(_dbContext); }
        }
        
        public StaffRepository StaffRepository
        {
            get { return _staffRepo ??= new StaffRepository(_dbContext); }
        }

        public BlogRepository BlogRepository
        {
            get { return _blogRepo ??= new BlogRepository(_dbContext);}
        }
        public Brand_ProductRepository Brand_ProductRepository
        {
            get { return _branchProductRepository ??= new Brand_ProductRepository(_dbContext); }
        }

        public async Task<int> Commit()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public DbSet<Product> Products => _dbContext.Products;
        public DbSet<Category> Categories => _dbContext.Categorys;
        public DbSet<Company> Companies => _dbContext.Companies;

    }
}

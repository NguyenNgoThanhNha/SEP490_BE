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
        private VoucherRepository _voucherRepo;
        private CompanyRepository _companyRepo;
        private OrderRepository _orderRepo;
        private OrderDetailRepository _orderDetail;
        private Branch_ServiceRepository _branchServiceRepository;
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

        public VoucherRepository VoucherRepository
        {
            get { return _voucherRepo ??= new VoucherRepository(_dbContext); }
        }

        public CompanyRepository CompanyRepository
        {
            get { return _companyRepo ??= new CompanyRepository(_dbContext); }
        }

        public OrderRepository OrderRepository
        {
            get { return _orderRepo ??= new OrderRepository(_dbContext); }
        }

        public OrderDetailRepository OrderDetailRepository
        {
            get { return _orderDetail ??= new OrderDetailRepository(_dbContext); }
        }

        public Branch_ServiceRepository Branch_ServiceRepository
        {
            get { return _branchServiceRepository ??= new Branch_ServiceRepository(_dbContext); }
        }
    }
}

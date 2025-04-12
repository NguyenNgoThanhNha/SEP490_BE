using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Server.Data.Entities;
using Server.Data.Repositories;

namespace Server.Data.UnitOfWorks
{
    public class UnitOfWorks
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction _transaction;

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
        private Branch_ProductRepository _branchProductRepository;
        private VoucherRepository _voucherRepo;
        private CompanyRepository _companyRepo;
        private OrderRepository _orderRepo;
        private OrderDetailRepository _orderDetail;
        private Branch_ServiceRepository _branchServiceRepository;
        private LoggerRepository _loggerRepository;
        private ServiceImageRepository _serviceImageRepository;
        private ProductImageRepository _productImageRepository;
        private SkincareRoutineRepository _skincareRoutineRepository;
        private SkinHealthRepository _skinHealthRepository;
        private ServiceCategoryRepository _serviceCategoryRepository;
        private UserRoutineRepository _userRoutineRepository;
        private SkinHealthImageRepository _skinHealthImageRepository;
        private WorkScheduleRepository _workScheduleRepository;
        private ShiftRepository _shiftRepository;
        private StaffLeaveRepository _staffLeaveRepository;
        private FeedbackAppointmentRepository _feedbackAppointmentRepository;
        private FeedbackServiceRepository _feedbackServiceRepository;
        private Staff_ServiceCategoryRepository _staffServiceCategoryRepository;
        private CartRepository _cartRepository;
        private ProductCartRepository _productCartRepository;
        private UserVoucherRepository _userVoucherRepository;
        private ProductRoutineStepRepository _productRoutineStepRepository;
        private ServiceRoutineStepRepository _serviceRoutineStepRepository;
        private SkinCareRoutineStepRepository _skinCareRoutineStepRepository;
        private UserRoutineStepRepository _userRoutineStepRepository;
        private ShipmentRepository _shipmentRepository;
        private NotificationRepository _notificationRepository;
        private AppointmentFeedbackRepository _appointmentFeedbackRepository;
        private ProductFeedbackRepository _productFeedbackRepository;
        private ServiceFeedbackRepository _serviceFeedbackRepository;
        private ServiceRoutineRepository _serviceRoutineRepository;
        private ProductRoutineRepository _productRoutineRepository;
        private SkinConcernRepository _skinConcernRepository;
        private SkinCareConcernRepository _skinCareConcernRepository;
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
            get { return _blogRepo ??= new BlogRepository(_dbContext); }
        }

        public Branch_ProductRepository Branch_ProductRepository
        {
            get { return _branchProductRepository ??= new Branch_ProductRepository(_dbContext); }
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

        public LoggerRepository LoggerRepository
        {
            get { return _loggerRepository ??= new LoggerRepository(_dbContext); }
        }

        public ServiceImageRepository ServiceImageRepository
        {
            get { return _serviceImageRepository ??= new ServiceImageRepository(_dbContext); }
        }


        public ProductImageRepository ProductImageRepository
        {
            get { return _productImageRepository ??= new ProductImageRepository(_dbContext); }
        }

        public SkincareRoutineRepository SkincareRoutineRepository
        {
            get { return _skincareRoutineRepository ??= new SkincareRoutineRepository(_dbContext); }
        }

        public SkinHealthRepository SkinHealthRepository
        {
            get { return _skinHealthRepository ??= new SkinHealthRepository(_dbContext); }
        }

        public ServiceCategoryRepository ServiceCategoryRepository
        {
            get { return _serviceCategoryRepository ??= new ServiceCategoryRepository(_dbContext); }
        }

        public UserRoutineRepository UserRoutineRepository
        {
            get { return _userRoutineRepository ??= new UserRoutineRepository(_dbContext); }
        }

        public SkinHealthImageRepository SkinHealthImageRepository
        {
            get { return _skinHealthImageRepository ??= new SkinHealthImageRepository(_dbContext); }
        }

        public WorkScheduleRepository WorkScheduleRepository
        {
            get { return _workScheduleRepository ??= new WorkScheduleRepository(_dbContext); }
        }

        public ShiftRepository ShiftRepository
        {
            get { return _shiftRepository ??= new ShiftRepository(_dbContext); }
        }

        public StaffLeaveRepository StaffLeaveRepository
        {
            get { return _staffLeaveRepository ??= new StaffLeaveRepository(_dbContext); }
        }

        public FeedbackAppointmentRepository FeedbackAppointmentRepository
        {
            get { return _feedbackAppointmentRepository ??= new FeedbackAppointmentRepository(_dbContext); }
        }

        public FeedbackServiceRepository FeedbackServiceRepository
        {
            get { return _feedbackServiceRepository ??= new FeedbackServiceRepository(_dbContext); }
        }

        public Staff_ServiceCategoryRepository Staff_ServiceCategoryRepository
        {
            get { return _staffServiceCategoryRepository ??= new Staff_ServiceCategoryRepository(_dbContext); }
        }

        public CartRepository CartRepository
        {
            get { return _cartRepository ??= new CartRepository(_dbContext); }
        }

        public ProductCartRepository ProductCartRepository
        {
            get { return _productCartRepository ??= new ProductCartRepository(_dbContext); }
        }

        public UserVoucherRepository UserVoucherRepository
        {
            get { return _userVoucherRepository ??= new UserVoucherRepository(_dbContext); }
        }

        public ProductRoutineStepRepository ProductRoutineStepRepository
        {
            get { return _productRoutineStepRepository ??= new ProductRoutineStepRepository(_dbContext); }
        }

        public ServiceRoutineStepRepository ServiceRoutineStepRepository
        {
            get { return _serviceRoutineStepRepository ??= new ServiceRoutineStepRepository(_dbContext); }
        }

        public SkinCareRoutineStepRepository SkinCareRoutineStepRepository
        {
            get { return _skinCareRoutineStepRepository ??= new SkinCareRoutineStepRepository(_dbContext); }
        }
        
        public UserRoutineStepRepository UserRoutineStepRepository
        {
            get { return _userRoutineStepRepository ??= new UserRoutineStepRepository(_dbContext); }
        }

        public ShipmentRepository ShipmentRepository
        {
            get { return _shipmentRepository ??= new ShipmentRepository(_dbContext); }
        }

        public NotificationRepository NotificationRepository
        {
            get { return _notificationRepository ??= new NotificationRepository(_dbContext); }
        }

        public AppointmentFeedbackRepository AppointmentFeedbackRepository
        {
            get { return _appointmentFeedbackRepository ??= new AppointmentFeedbackRepository(_dbContext); }
        }

        public ProductFeedbackRepository ProductFeedbackRepository
        {
            get { return _productFeedbackRepository ??= new ProductFeedbackRepository(_dbContext); }
        }

        public ServiceFeedbackRepository ServiceFeedbackRepository
        {
            get { return _serviceFeedbackRepository ??= new ServiceFeedbackRepository(_dbContext); }
        }
        
        public SkinConcernRepository SkinConcernRepository
        {
            get { return _skinConcernRepository ??= new SkinConcernRepository(_dbContext); }
        }
        
        public SkinCareConcernRepository SkinCareConcernRepository
        {
            get { return _skinCareConcernRepository ??= new SkinCareConcernRepository(_dbContext); }
        }

        public ServiceRoutineRepository ServiceRoutineRepository
        {
            get { return _serviceRoutineRepository ??= new ServiceRoutineRepository(_dbContext); }
        }

        public ProductRoutineRepository ProductRoutineRepository
        {
            get { return _productRoutineRepository ??= new ProductRoutineRepository(_dbContext); }
        }


        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
            return _transaction;
        }


        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
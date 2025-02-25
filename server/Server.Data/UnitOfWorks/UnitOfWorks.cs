﻿using Microsoft.EntityFrameworkCore;
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
        private LoggerRepository _loggerRepository;
        private ServiceImageRepository _serviceImageRepository;
        private ProductImageRepository _productImageRepository;
        private SkincareRoutineRepository _skincareRoutineRepository;
        private SkinHealthRepository _skinHealthRepository;
        private ServiceCategoryRepository _serviceCategoryRepository;
        private RoomRepository _roomRepository;
        private BedRepository _bedRepository;
        private BedAvailabilityRepository _bedAvailabilityRepository;
        private UserRoutineRepository _userRoutineRepository;
        private SkinHealthImageRepository _skinHealthImageRepository;
        private WorkScheduleRepository _workScheduleRepository;
        private ShiftRepository _shiftRepository;
        private StaffLeaveRepository _staffLeaveRepository;
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
        
        public RoomRepository RoomRepository
        {
            get { return _roomRepository ??= new RoomRepository(_dbContext); }
        }
        
        public BedRepository BedRepository
        {
            get { return _bedRepository ??= new BedRepository(_dbContext); }
        }
        
        public BedAvailabilityRepository BedAvailabilityRepository
        {
            get { return _bedAvailabilityRepository ??= new BedAvailabilityRepository(_dbContext); }
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
    }
}

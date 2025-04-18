using AutoMapper;
using Org.BouncyCastle.Asn1.Crmf;
using Server.Business.Commons.Request;
using Server.Business.Dtos;
using Server.Business.Dtos.Server.Business.Dtos;
using Server.Business.Models;
using Server.Data.Entities;

namespace Server.Business.Mappers
{
    public class ProfilesMapper : Profile
    {
        public ProfilesMapper()
        {
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<User, UserInfoModel>().ReverseMap();
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<UserModel, UserDTO>().ReverseMap();
            CreateMap<UserInfoModel, UserDTO>().ReverseMap();

            CreateMap<Server.Data.Entities.Service, ServiceModel>().ReverseMap();
            CreateMap<Server.Data.Entities.Service, ServiceDto>().ReverseMap();
            CreateMap<ServiceModel, ServiceDto>().ReverseMap();

            CreateMap<Promotion, PromotionModel>().ReverseMap();
            CreateMap<Promotion, PromotionDTO>().ReverseMap();
            CreateMap<PromotionModel, PromotionDTO>().ReverseMap();

            CreateMap<Branch, BranchModel>().ReverseMap();
            CreateMap<Branch, BranchDTO>().ReverseMap();
            CreateMap<BranchModel, BranchDTO>().ReverseMap();

            CreateMap<Branch_Promotion, BranchPromotionModel>().ReverseMap();
            CreateMap<Branch_Promotion, BranchPromotionDTO>().ReverseMap();
            CreateMap<BranchPromotionModel, BranchPromotionDTO>().ReverseMap();

            CreateMap<Branch_Product, BranchProductDto>().ReverseMap();
            CreateMap<CreateBranchProductDto, Branch_Product>();
            CreateMap<UpdateBranchProductDto, Branch_Product>();

            CreateMap<Staff, StaffModel>().ReverseMap();
            CreateMap<Staff, StaffDTO>().ReverseMap();
            CreateMap<StaffModel, StaffDTO>().ReverseMap();

            CreateMap<Product, ProductModel>().ReverseMap();
            CreateMap<ProductModel, ProductDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, ProductDetailDto>().ReverseMap();

            CreateMap<Product, ProductModel>()
      .ForMember(dest => dest.images, opt => opt.MapFrom(src =>
          src.ProductImages.Select(i => i.image)
      ))
      .ForMember(dest => dest.Branches, opt => opt.MapFrom(src =>
          src.Branch_Products != null && src.Branch_Products.Any()
              ? src.Branch_Products.First().Branch
              : null
      ));








            CreateMap<Category, CategoryModel>().ReverseMap();
            CreateMap<Category, CategoryDetailDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CategoryModel, CategoryDetailDto>().ReverseMap();
            
            CreateMap<CUOrderDto, Order>();
            CreateMap<CUOrderDetailDto, OrderDetail>();
            CreateMap<CUAppointmentDto, Appointments>();

            /*CreateMap<Appointments, AppointmentsModel>()
     .ForMember(dest => dest.TotalSteps, opt => opt.MapFrom(src =>
         src.Service != null &&
         src.Service.ServiceRoutines != null &&
         src.Service.ServiceRoutines.Any()
             ? src.Service.ServiceRoutines
                 .Where(sr => sr.Status == "Active" && sr.Routine != null)
                 .Select(sr => sr.Routine.TotalSteps)
                 .FirstOrDefault()
             : (int?)null
     ));*/
            CreateMap<Appointments, AppointmentsModel>().ReverseMap();
            CreateMap<Appointments, CustomerAppointmentModel>().ForMember(dest => dest.TotalSteps, opt => opt.MapFrom(src =>
         src.Service != null &&
         src.Service.ServiceRoutines != null &&
         src.Service.ServiceRoutines.Any()
             ? src.Service.ServiceRoutines
                 .Where(sr => sr.Status == "Active" && sr.Routine != null)
                 .Select(sr => sr.Routine.TotalSteps)
                 .FirstOrDefault()
             : (int?)null
     ));


            CreateMap<Appointments, AppointmentsInfoModel>().ReverseMap();
            CreateMap<Appointments, AppointmentsDTO>().ReverseMap();
            CreateMap<AppointmentsModel, AppointmentsDTO>().ReverseMap();

            CreateMap<Blog, BlogModel>().ReverseMap();
            CreateMap<Blog, BlogDTO>().ReverseMap();
            CreateMap<BlogModel, BlogDTO>().ReverseMap();
            CreateMap<Blog, BlogModel>()
     .ForMember(dest => dest.AuthorName, opt => opt.Ignore()) // AuthorName ánh xạ thủ công
     .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail)); // Gán thumbnail

            CreateMap<Logger, LoggerModel>().ReverseMap();
            CreateMap<Branch_Service, Branch_ServiceModel>().ReverseMap();
            
            CreateMap<Order, OrderModel>().ReverseMap();
            CreateMap<Shipment, ShipmentModel>();

            CreateMap<SkincareRoutine, SkincareRoutineModel>().ReverseMap();
            
            CreateMap<ServiceRoutine, ServiceRoutineModel>().ReverseMap();
            
            CreateMap<ProductRoutine, ProductRoutineModel>().ReverseMap();
                
            CreateMap<SkinHealth, SkinHealthModel>().ReverseMap();
            
            CreateMap<ServiceCategory, ServiceCategoryModel>().ReverseMap();
            CreateMap<ServiceCategory, ServiceCategoryDto>().ReverseMap();
            CreateMap<ServiceCategoryModel, ServiceCategoryDto>().ReverseMap();
            
            CreateMap<WorkSchedule, WorkScheduleModel>().ReverseMap();
            CreateMap<Shifts, ShiftModel>().ReverseMap();
            CreateMap<StaffLeave, StaffLeaveModel>().ReverseMap();
            
            CreateMap<Order, OrderModel>().ReverseMap();
            CreateMap<Order, OrderInfoModel>().ReverseMap();
            CreateMap<OrderDetail, OrderDetailModels>().ReverseMap();

            CreateMap<CartRequest, ProductCart>().ReverseMap();
            CreateMap<ProductCart, CartDTO>().ReverseMap();
            CreateMap<Voucher, VoucherDto>().ReverseMap();
            
            CreateMap<SkinCareRoutineStep, SkinCareRoutineStepModel>().ReverseMap();
            CreateMap<ServiceRoutineStep, ServiceRoutineStepModel>().ReverseMap();
            CreateMap<ProductRoutineStep, ProductRoutineStepModel>().ReverseMap();
            
            CreateMap<UserRoutine, UserRoutineModel>().ReverseMap();
            CreateMap<UserRoutineStep, UserRoutineStepModel>().ReverseMap();


            CreateMap<Promotion, PromotionDTO>().ReverseMap();

            CreateMap<Branch_Service, BranchServiceDto>().ReverseMap();
            CreateMap<CreateBranchServiceDto, Branch_Service>().ReverseMap();
            CreateMap<UpdateBranchServiceDto, Branch_Service>().ReverseMap();

            CreateMap<AppointmentFeedback, AppointmentFeedbackDetailDto>().ReverseMap();
            CreateMap<AppointmentFeedbackCreateDto, AppointmentFeedback>().ReverseMap();
            CreateMap<AppointmentFeedbackUpdateDto, AppointmentFeedback>().ReverseMap();

            CreateMap<ProductFeedback, ProductFeedbackDetailDto>().ReverseMap();     
            CreateMap<ProductFeedbackCreateDto, ProductFeedback>().ReverseMap();
       


            CreateMap<ProductFeedbackUpdateDto, ProductFeedback>().ReverseMap();

            CreateMap<ServiceFeedback, ServiceFeedbackDetailDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer));

            CreateMap<ServiceFeedbackCreateDto, ServiceFeedback>().ReverseMap();

            CreateMap<ServiceFeedbackUpdateDto, ServiceFeedback>().ReverseMap();

            CreateMap<SkincareRoutine, SkincareRoutineDto>().ReverseMap();
            CreateMap<CreateSkincareRoutineDto, SkincareRoutine>().ReverseMap();
            CreateMap<UpdateSkincareRoutineDto, SkincareRoutine>().ReverseMap();

            CreateMap<SkinCareRoutineStep, SkinCareRoutineStepDto>().ReverseMap();
            CreateMap<CreateSkinCareRoutineStepDto, SkinCareRoutineStep>().ReverseMap();
            CreateMap<UpdateSkinCareRoutineStepDto, SkinCareRoutineStep>().ReverseMap();

            CreateMap<ServiceRoutine, ServiceRoutineDto>().ReverseMap();           
            CreateMap<SkincareRoutine, SkincareRoutineDto>().ReverseMap();


            CreateMap<ServiceRoutineStep, ServiceRoutineStepDto>().ReverseMap();

            CreateMap<ProductRoutine, ProductRoutineDto>()
    .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Products))
    .ForMember(dest => dest.Routine, opt => opt.MapFrom(src => src.Routine));

            CreateMap<ProductRoutineStep, ProductRoutineStepDto>().ReverseMap();

            CreateMap<OrderDetail, OrderDetailModels>().ReverseMap();



        }
    }
}

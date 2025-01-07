using AutoMapper;
using Server.Business.Dtos;
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

            CreateMap<Service, ServiceModel>().ReverseMap();
            CreateMap<Service, ServiceDto>().ReverseMap();
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
           
            CreateMap<Staff, StaffModel>().ReverseMap();
            CreateMap<Staff, StaffDTO>().ReverseMap();
            CreateMap<StaffModel, StaffDTO>().ReverseMap();

            CreateMap<Product, ProductModel>().ReverseMap();
            CreateMap<ProductModel, ProductDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            
            CreateMap<Category, CategoryModel>().ReverseMap();
            CreateMap<Category, CategoryDetailDto>().ReverseMap();
            CreateMap<CategoryModel, CategoryDetailDto>().ReverseMap();
            
            CreateMap<CUOrderDto, Order>();
            CreateMap<CUOrderDetailDto, OrderDetail>();
            CreateMap<CUAppointmentDto, Appointments>();

            CreateMap<Appointments, AppointmentsModel>().ReverseMap();
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
            
            CreateMap<SkincareRoutine, SkincareRoutineModel>().ReverseMap();
            
            CreateMap<UserRoutine,UserRoutineModel>().ReverseMap();
            
            CreateMap<ServiceRoutine, ServiceRoutineModel>().ReverseMap();
            
            CreateMap<ProductRoutine, ProductRoutineModel>().ReverseMap();
                
            CreateMap<SkinHealth, SkinHealthModel>().ReverseMap();
        }
    }
}

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
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<UserModel, UserDTO>().ReverseMap();
            
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
            
            CreateMap<Appointments, AppointmentsModel>().ReverseMap();
            CreateMap<Appointments, AppointmentsDTO>().ReverseMap();
            CreateMap<AppointmentsModel, AppointmentsDTO>().ReverseMap();
        }
    }
}

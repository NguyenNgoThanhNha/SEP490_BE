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

        }
    }
}

﻿using AutoMapper;
using Org.BouncyCastle.Asn1.Crmf;
using Server.Business.Commons.Request;
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
           
            CreateMap<Staff, StaffModel>().ReverseMap();
            CreateMap<Staff, StaffDTO>().ReverseMap();
            CreateMap<StaffModel, StaffDTO>().ReverseMap();

            CreateMap<Product, ProductModel>().ReverseMap();
            CreateMap<ProductModel, ProductDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, ProductDetailDto>().ReverseMap();
            
            CreateMap<Category, CategoryModel>().ReverseMap();
            CreateMap<Category, CategoryDetailDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CategoryModel, CategoryDetailDto>().ReverseMap();
            
            CreateMap<CUOrderDto, Order>();
            CreateMap<CUOrderDetailDto, OrderDetail>();
            CreateMap<CUAppointmentDto, Appointments>();

            CreateMap<Appointments, AppointmentsModel>().ReverseMap();
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

            CreateMap<CartRequest, ProductCart>();
            CreateMap<ProductCart, CartDTO>();
            CreateMap<Voucher, VoucherDto>();
            
            CreateMap<SkinCareRoutineStep, SkinCareRoutineStepModel>();
            CreateMap<ServiceRoutineStep, ServiceRoutineStepModel>();
            CreateMap<ProductRoutineStep, ProductRoutineStepModel>();
            
            CreateMap<UserRoutine, UserRoutineModel>();
            CreateMap<UserRoutineStep, UserRoutineStepModel>();
            
            
        }
    }
}

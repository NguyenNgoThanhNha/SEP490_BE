using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Services
{
    public class ServiceService
    {
        private readonly AppDbContext _context;
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper _mapper;

        public ServiceService(AppDbContext context, UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _context = context;
            this.unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<GetAllServicePaginationResponse> GetAllService(int page)
        {         
            const int pageSize = 4; // Set the number of objects per page
            var services = await unitOfWorks.ServiceRepository.GetAll().OrderByDescending(x => x.ServiceId).ToListAsync();

            // Calculate total count of service
            var totalCount = services.Count();

            // Calculate total service
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get the users for the current page
            var pagedServices = services.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Map to ServiceModal
            var serviceModels = _mapper.Map<List<ServiceModel>>(pagedServices);

            return new GetAllServicePaginationResponse
            {
                data = serviceModels,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }

        public async Task<ServiceDto> GetServiceByIdAsync(int id)
        {
            var service = await _context.Services
        .Include(s => s.Category) 
        .FirstOrDefaultAsync(s => s.ServiceId == id); 

            if (service == null) return null;

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration,
                CategoryName = service.Category.Name, 
                CreatedDate = service.CreatedDate,
                UpdatedDate = service.UpdatedDate
            };
        }

        public async Task<ServiceDto> CreateServiceAsync(ServiceCreateDto serviceDto)
        {
            var service = new Data.Entities.Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Price = serviceDto.Price,
                Duration = serviceDto.Duration,
                CategoryId = serviceDto.CategoryId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            var category = await _context.Categorys.FindAsync(service.CategoryId);
            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration,
                CategoryName = category?.Name,
                CreatedDate = service.CreatedDate,
                UpdatedDate = service.UpdatedDate
            };
        }

        public async Task<ServiceDto> UpdateServiceAsync(ServiceUpdateDto serviceDto, int serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return null;

            service.Name = serviceDto.Name;
            service.Description = serviceDto.Description;
            service.Price = serviceDto.Price;
            service.Duration = serviceDto.Duration;
            service.CategoryId = serviceDto.CategoryId;
            service.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            var category = await _context.Categorys.FindAsync(service.CategoryId);
            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration,
                CategoryName = category?.Name,
                CreatedDate = service.CreatedDate,
                UpdatedDate = service.UpdatedDate
            };
        }

        public async Task<Server.Data.Entities.Service> DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return null;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return service; 
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Server.Business.Dtos;
using Server.Data.Entities;
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

        public ServiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceDto>> GetAllServicesAsync()
        {
            return await _context.Services.Select(s => new ServiceDto
            {
                ServiceId = s.ServiceId,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                Duration = s.Duration,
                CategoryId = s.CategoryId,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate
            }).ToListAsync();
        }

        public async Task<ServiceDto> GetServiceByIdAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return null;

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration,
                CategoryId = service.CategoryId,
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

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration,
                CategoryId = service.CategoryId,
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
            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration,
                CategoryId = service.CategoryId,
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

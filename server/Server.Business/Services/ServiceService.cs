using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;

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

        public async Task<Pagination<Service>> GetListAsync(Expression<Func<Service, bool>> filter = null,
                                    Func<IQueryable<Service>, IOrderedQueryable<Service>> orderBy = null,
                                    int? pageIndex = null, // Optional parameter for pagination (page number)
                                    int? pageSize = null)
        {
            IQueryable<Service> query = _context.Services;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var totalItemsCount = await query.CountAsync();

            if (pageIndex.HasValue && pageIndex.Value == -1)
            {
                pageSize = totalItemsCount; // Set pageSize to total count
                pageIndex = 0; // Reset pageIndex to 0
            }
            else if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10; // Assuming a default pageSize of 10 if an invalid value is passed

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            var items = await query.ToListAsync();

            return new Pagination<Service>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Data = items
            };
        }

        public async Task<GetAllServicePaginationResponse> GetAllService(int page)
        {
            const int pageSize = 4;
            var services = await unitOfWorks.ServiceRepository.GetAll().OrderByDescending(x => x.ServiceId).ToListAsync();


            var totalCount = services.Count();


            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);


            var pagedServices = services.Skip((page - 1) * pageSize).Take(pageSize).ToList();


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

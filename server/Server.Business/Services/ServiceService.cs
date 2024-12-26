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
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public ServiceService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<Pagination<Service>> GetListAsync(
     Expression<Func<Service, bool>> filter = null,
     Func<IQueryable<Service>, IOrderedQueryable<Service>> orderBy = null,
     int? pageIndex = null,
     int? pageSize = null)
        {
            // Truy vấn dữ liệu từ repository với điều kiện ban đầu là `status == "Active"`
            IQueryable<Service> query = _unitOfWorks.ServiceRepository.GetAll()
      .Include(s => s.Category) // Bao gồm thông tin Category
      .Include(s => s.Branch_Services) // Bao gồm Branch_Services
          .ThenInclude(bs => bs.Branch) // Bao gồm thông tin Branch từ Branch_Services
      .Where(s => s.Status == "Active"); // Chỉ lấy các Service có trạng thái Active


            // Áp dụng bộ lọc (nếu có)
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Áp dụng sắp xếp (nếu có)
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Tổng số bản ghi
            var totalItemsCount = await query.CountAsync();

            // Xử lý phân trang
            if (pageIndex.HasValue && pageIndex.Value == -1)
            {
                pageSize = totalItemsCount; // Lấy tất cả nếu `pageIndex == -1`
                pageIndex = 0;
            }
            else if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = Math.Max(pageIndex.Value, 0);
                int validPageSize = Math.Max(pageSize.Value, 1);

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            // Truy vấn dữ liệu sau khi áp dụng các điều kiện
            var items = await query.ToListAsync();

            // Trả về dữ liệu phân trang
            return new Pagination<Service>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Data = items
            };
        }


        public async Task<GetAllServicePaginationResponse> GetAllService(int page, int pageSize)
        {
            var services = await _unitOfWorks.ServiceRepository.GetAll().Include(x => x.Category).OrderByDescending(x => x.ServiceId).ToListAsync();


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
        
        public async Task<GetAllServicePaginationResponse> GetAllServiceForBranch(int page, int pageSize, int branchId)
        {

            // Lấy danh sách ServiceId thuộc về branchId cụ thể
            var serviceIdsOfBranch = await _unitOfWorks.Branch_ServiceRepository.GetAll()
                .Where(bs => bs.BranchId == branchId)
                .Select(bs => bs.ServiceId)
                .ToListAsync();

            // Lấy danh sách Service dựa trên danh sách ServiceId
            var services = await _unitOfWorks.ServiceRepository.GetAll()
                .Include(s => s.Category) // Bao gồm thông tin Category nếu cần
                .Where(s => serviceIdsOfBranch.Contains(s.ServiceId))
                .OrderByDescending(s => s.ServiceId)
                .ToListAsync();
            
            var totalCount = services.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedServices = services
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
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
            // Truy vấn dịch vụ và bao gồm thông tin danh mục
            var service = await _unitOfWorks.ServiceRepository
                .FindByCondition(s => s.ServiceId == id&&s.Status=="Active")
                .Include(s => s.Category) // Bao gồm danh mục nếu cần
                .FirstOrDefaultAsync();

            // Kiểm tra nếu không tìm thấy dịch vụ
            if (service == null)
                return null;

            return _mapper.Map<ServiceDto>(service);
        }


        public async Task<ServiceDto> CreateServiceAsync(ServiceCreateDto serviceDto)
        {
            try
            {
                // Kiểm tra nếu DTO bị null
                if (serviceDto == null)
                {
                    throw new ArgumentNullException(nameof(serviceDto), "Service data is required.");
                }

                // Tạo thực thể Service từ DTO
                var service = new Data.Entities.Service
                {
                    Name = serviceDto.Name,
                    Description = serviceDto.Description,
                    Price = serviceDto.Price,
                    Duration = serviceDto.Duration,
                    CategoryId = serviceDto.CategoryId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Status = "Active",
                };

                // Thêm vào repository qua UnitOfWork
                await _unitOfWorks.ServiceRepository.AddAsync(service);
                await _unitOfWorks.ServiceRepository.Commit();

                // Lấy thông tin danh mục liên quan
                var category = await _unitOfWorks.CategoryRepository
     .FindByCondition(c => c.CategoryId == service.CategoryId)
     .FirstOrDefaultAsync();

                // Trả về DTO
                return new ServiceDto
                {
                    ServiceId = service.ServiceId,
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price,
                    Duration = service.Duration,
                    CategoryName = category?.Name, // Kiểm tra null cho Category
                    CreatedDate = service.CreatedDate,
                    UpdatedDate = service.UpdatedDate,
                    Status = service.Status,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while creating the service: {ex.Message}", ex);
            }
        }


        public async Task<ServiceDto> UpdateServiceAsync(ServiceUpdateDto serviceDto, int serviceId)
        {
            try
            {
                // Kiểm tra nếu DTO bị null
                if (serviceDto == null)
                {
                    throw new ArgumentNullException(nameof(serviceDto), "Service update data is required.");
                }

                // Tìm dịch vụ cần cập nhật thông qua UnitOfWork
                var service = await _unitOfWorks.ServiceRepository
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

                if (service == null)
                {
                    throw new KeyNotFoundException($"Service with ID {serviceId} not found.");
                }

                // Cập nhật thông tin dịch vụ
                service.Name = serviceDto.Name;
                service.Description = serviceDto.Description;
                service.Price = serviceDto.Price;
                service.Duration = serviceDto.Duration;
                service.CategoryId = serviceDto.CategoryId;
                service.UpdatedDate = DateTime.Now;

                // Lưu thay đổi qua UnitOfWork
                _unitOfWorks.ServiceRepository.Update(service);
                await _unitOfWorks.Commit();

                // Lấy thông tin danh mục liên quan
                var category = await _unitOfWorks.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == service.CategoryId);

                // Trả về DTO sau khi cập nhật
                return new ServiceDto
                {
                    ServiceId = service.ServiceId,
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price,
                    Duration = service.Duration,
                    CategoryName = category?.Name, // Kiểm tra null cho Category
                    CreatedDate = service.CreatedDate,
                    UpdatedDate = service.UpdatedDate,
                    Status = service.Status,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the service: {ex.Message}", ex);
            }
        }


        public async Task<Service> DeleteServiceAsync(int id)
        {
            try
            {
                // Tìm dịch vụ cần xóa
                var service = await _unitOfWorks.ServiceRepository
    .FindByCondition(s => s.ServiceId == id)
    .FirstOrDefaultAsync();


                // Kiểm tra nếu không tìm thấy dịch vụ
                if (service == null)
                {
                    throw new KeyNotFoundException($"Service with ID {id} not found.");
                }

                // Cập nhật trạng thái dịch vụ thành Inactive
                service.Status = "Inactive";
                service.UpdatedDate = DateTime.Now; // Cập nhật thời gian sửa đổi

                // Lưu thay đổi
                _unitOfWorks.ServiceRepository.Update(service); // Sử dụng DbSet trực tiếp để cập nhật
                await _unitOfWorks.ServiceRepository.Commit();

                return service;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the status of the service: {ex.Message}", ex);
            }
        }
    }
}

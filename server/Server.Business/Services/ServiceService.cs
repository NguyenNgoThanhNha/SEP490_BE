using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;
using Server.Business.Exceptions;

namespace Server.Business.Services
{
    public class ServiceService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly CloudianryService _cloudianryService;

        public ServiceService(UnitOfWorks unitOfWorks, IMapper mapper, CloudianryService cloudianryService)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _cloudianryService = cloudianryService;
        }

        public async Task<Pagination<ServiceModel>> GetListAsync(
     Expression<Func<Service, bool>> filter = null,
     Func<IQueryable<Service>, IOrderedQueryable<Service>> orderBy = null,
     int? pageIndex = null,
     int? pageSize = null)
        {
            // Truy vấn dữ liệu từ repository với điều kiện ban đầu là `status == "Active"`
            IQueryable<Service> query = _unitOfWorks.ServiceRepository.GetAll()
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
            
            var serviceModels = _mapper.Map<List<ServiceModel>>(items);
            // chạy lặp qua services và lấy hình của chúng ra trong service_images
            foreach (var service in serviceModels)
            {
                var serviceImages = await _unitOfWorks.ServiceImageRepository.GetAll()
                    .Where(si => si.ServiceId == service.ServiceId)
                    .Select(si => si.image)
                    .ToArrayAsync();

                service.images = serviceImages;
            }

            // Trả về dữ liệu phân trang
            return new Pagination<ServiceModel>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Data = serviceModels
            };
        }


        public async Task<GetAllServicePaginationResponse> GetAllService(int page, int pageSize)
        {
            var services = await _unitOfWorks.ServiceRepository.GetAll()
                .OrderByDescending(x => x.ServiceId).ToListAsync();

            var serviceModels = _mapper.Map<List<ServiceModel>>(services);
            // chạy lặp qua services và lấy hình của chúng ra trong service_images
            foreach (var service in serviceModels)
            {
                var serviceImages = await _unitOfWorks.ServiceImageRepository.GetAll()
                    .Where(si => si.ServiceId == service.ServiceId)
                    .Select(si => si.image)
                    .ToArrayAsync();

                service.images = serviceImages;
            }
            var totalCount = services.Count();


            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);


            var pagedServices = serviceModels.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            return new GetAllServicePaginationResponse
            {
                data = pagedServices,
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
                .Where(s => serviceIdsOfBranch.Contains(s.ServiceId))
                .OrderByDescending(s => s.ServiceId)
                .ToListAsync();
            
            var serviceModels = _mapper.Map<List<ServiceModel>>(services);
            // chạy lặp qua services và lấy hình của chúng ra trong service_images
            foreach (var service in serviceModels)
            {
                var serviceImages = await _unitOfWorks.ServiceImageRepository.GetAll()
                    .Where(si => si.ServiceId == service.ServiceId)
                    .Select(si => si.image)
                    .ToArrayAsync();

                service.images = serviceImages;
            }
            
            var totalCount = services.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedServices = serviceModels
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            
            return new GetAllServicePaginationResponse
            {
                data = pagedServices,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }


        public async Task<ServiceModel> GetServiceByIdAsync(int id)
        {
            // Truy vấn dịch vụ và bao gồm thông tin danh mục
            var service = await _unitOfWorks.ServiceRepository
                .FindByCondition(s => s.ServiceId == id&&s.Status=="Active")
                .FirstOrDefaultAsync();
            
            // Kiểm tra nếu không tìm thấy dịch vụ
            if (service == null)
                return null;

            var serviceModel = _mapper.Map<ServiceModel>(service);
            
            var serviceImages = await _unitOfWorks.ServiceImageRepository.FindByCondition(x => x.ServiceId == serviceModel.ServiceId)
                .Where(si => si.ServiceId == service.ServiceId)
                .Select(si => si.image)
                .ToArrayAsync();

            serviceModel.images = serviceImages;
            return serviceModel;
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
                var service = new Service
                {
                    Name = serviceDto.Name,
                    Description = serviceDto.Description,
                    Price = serviceDto.Price,
                    Duration = serviceDto.Duration,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Status = "Active",
                };
                
                // Thêm service vào cơ sở dữ liệu
                await _unitOfWorks.ServiceRepository.AddAsync(service);
                await _unitOfWorks.ServiceRepository.Commit();

                // Bắt đầu tải lên hình ảnh đồng thời
                var uploadTasks = serviceDto.images?.Select(image => _cloudianryService.UploadImageAsync(image)).ToList();
                var imageUploadResults = await Task.WhenAll(uploadTasks);

                // Xử lý kết quả tải lên
                var listServiceImages = imageUploadResults
                    .Where(result => result != null)
                    .Select(result => new ServiceImages
                    {
                        ServiceId = service.ServiceId, // ServiceId sẽ được gán sau khi lưu Service
                        image = result.SecureUrl.ToString(),
                    }).ToList();

                // Thêm hình ảnh vào cơ sở dữ liệu
                if (listServiceImages.Any())
                {
                    await _unitOfWorks.ServiceImageRepository.AddRangeAsync(listServiceImages);
                }
                
                // Lưu lại ServiceId cho hình ảnh
                listServiceImages.ForEach(image => image.ServiceId = service.ServiceId);
                await _unitOfWorks.ServiceImageRepository.Commit();

                return _mapper.Map<ServiceDto>(service);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while creating the service: {ex.Message}", ex);
            }
        }



        public async Task<ServiceDto> UpdateServiceAsync(ServiceUpdateDto serviceDto, int serviceId)
        {
                // Kiểm tra nếu DTO bị null
                if (serviceDto == null)
                {
                    throw new BadRequestException("Service update data is required.");
                }

                // Tìm dịch vụ cần cập nhật thông qua UnitOfWork
                var service = await _unitOfWorks.ServiceRepository
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

                if (service == null)
                {
                    throw new BadRequestException($"Service with ID {serviceId} not found.");
                }

                // Cập nhật thông tin dịch vụ
                service.Name = serviceDto.Name;
                service.Description = serviceDto.Description;
                service.Price = serviceDto.Price;
                service.Duration = serviceDto.Duration;
                service.UpdatedDate = DateTime.Now;

                // Lấy danh sách hình ảnh hiện tại của dịch vụ
                var existingImages = await _unitOfWorks.ServiceImageRepository
                    .FindByCondition(img => img.ServiceId == serviceId).ToListAsync();

                // Kiểm tra số lượng hình ảnh có đồng nhất không
                if (existingImages.Count == serviceDto.images.Count)
                {
                    // Cập nhật URL hình ảnh hiện tại
                    var updateTasks = existingImages.Zip(serviceDto.images, async (existingImage, newImage) =>
                    {
                        var uploadResult = await _cloudianryService.UploadImageAsync(newImage);
                        existingImage.image = uploadResult.SecureUrl.ToString();
                    });

                    await Task.WhenAll(updateTasks);
                    await _unitOfWorks.ServiceImageRepository.UpdateRangeAsync(existingImages);
                }
                else
                {
                    // Xóa hình ảnh cũ và thêm hình ảnh mới
                    await _unitOfWorks.ServiceImageRepository.RemoveRangeAsync(existingImages);

                    var uploadTasks = serviceDto.images.Select(async image =>
                    {
                        var uploadResult = await _cloudianryService.UploadImageAsync(image);
                        return new ServiceImages
                        {
                            ServiceId = service.ServiceId,
                            image = uploadResult.SecureUrl.ToString(),
                        };
                    });

                    var newImages = await Task.WhenAll(uploadTasks);
                    await _unitOfWorks.ServiceImageRepository.AddRangeAsync(newImages);
                }

                // Lưu thay đổi qua UnitOfWork
                await _unitOfWorks.ServiceImageRepository.Commit();
                _unitOfWorks.ServiceRepository.Update(service);
                await _unitOfWorks.ServiceRepository.Commit();

                return _mapper.Map<ServiceDto>(service);
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


        //public async Task<List<FeaturedServiceDto>> GetTop4FeaturedServicesAsync()
        //{
        //    // Lấy danh sách Appointments có trạng thái Confirmed và include Service
        //    var appointments = await _unitOfWorks.AppointmentsRepository
        //        .FindByCondition(a => a.Status == "Confirmed") // Chỉ lọc theo điều kiện
        //        .Include(a => a.Service) // Bao gồm thông tin Service
        //        .ToListAsync();

        //    // Nhóm theo ServiceId và tính toán các thông tin cần thiết
        //    var featuredServices = appointments
        //        .Where(a => a.Service != null) // Bỏ qua những bản ghi không có Service
        //        .GroupBy(a => a.ServiceId)
        //        .Select(g => new FeaturedServiceDto
        //        {
        //            ServiceId = g.Key,
        //            ServiceName = g.First().Service.Name,
        //            Description = g.First().Service.Description,
        //            Price = g.First().Service.Price,
        //            TotalQuantity = g.Sum(a => a.Quantity)
        //        })
        //        .OrderByDescending(s => s.TotalQuantity) // Sắp xếp giảm dần theo tổng Quantity
        //        .Take(4) // Lấy 4 dịch vụ nổi bật
        //        .ToList();

        //    return featuredServices;
        //}

        public async Task<List<Service>> GetTop4FeaturedServicesAsync()
        {
            // Lấy danh sách Appointments có trạng thái Confirmed và bao gồm Service cùng các quan hệ cần thiết
            var appointments = await _unitOfWorks.AppointmentsRepository
                .FindByCondition(a => a.Status == "Confirmed")
                .Include(a => a.Service)
                    .ThenInclude(s => s.Branch_Services) // Bao gồm Branch_Services nếu cần
                .Include(a => a.Service)
                    .ThenInclude(s => s.ServiceRoutines) // Bao gồm ServiceRoutines nếu cần
                .ToListAsync();

            // Nhóm theo ServiceId và tính toán
            var featuredServices = appointments
                .Where(a => a.Service != null) // Loại bỏ các bản ghi không có Service
                .GroupBy(a => a.ServiceId)
                .Select(g =>
                {
                    var service = g.First().Service;

                    // Bao gồm đầy đủ thông tin của Service
                    //service.TotalQuantity = g.Sum(a => a.Quantity); // Bổ sung TotalQuantity nếu cần
                    return service;
                })
                //.OrderByDescending(s => s.TotalQuantity) // Sắp xếp theo TotalQuantity
                .Take(4) // Lấy 4 dịch vụ nổi bật
                .ToList();

            return featuredServices;
        }




    }
}

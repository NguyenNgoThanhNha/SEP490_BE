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
using Service.Business.Services;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Request;
using Server.Data;

namespace Server.Business.Services
{
    public class ServiceService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly CloudianryService _cloudianryService;
        private readonly IAIMLService _gptService;

        public ServiceService(UnitOfWorks unitOfWorks, IMapper mapper, CloudianryService cloudianryService,
            IAIMLService gptService)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _cloudianryService = cloudianryService;
            _gptService = gptService;
        }

        public async Task<Pagination<ServiceModel>> GetListAsync(
            Expression<Func<Server.Data.Entities.Service, bool>> filter = null,
            Func<IQueryable<Server.Data.Entities.Service>, IOrderedQueryable<Server.Data.Entities.Service>> orderBy =
                null,
            int? pageIndex = null,
            int? pageSize = null,
            int? serviceCategoryId = null)
        {
            // Truy vấn dữ liệu từ repository với điều kiện ban đầu là `status == "Active"`
            IQueryable<Server.Data.Entities.Service> query = _unitOfWorks.ServiceRepository.GetAll()
                .Include(x => x.ServiceCategory)
                .Include(s => s.Branch_Services) // Bao gồm Branch_Services
                .ThenInclude(bs => bs.Branch) // Bao gồm thông tin Branch từ Branch_Services
                .Where(s => s.Status == "Active"); // Chỉ lấy các Service có trạng thái Active

            // Lọc theo ServiceCategoryId (nếu có)
            if (serviceCategoryId.HasValue)
            {
                query = query.Where(s => s.ServiceCategoryId == serviceCategoryId);
            }

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
                .Include(x => x.ServiceCategory)
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

        //public async Task<List<ServiceModel>> GetListImagesOfServices(List<Data.Entities.Service> listService)
        //{
        //    var serviceModels = _mapper.Map<List<ServiceModel>>(listService);
        //    // chạy lặp qua services và lấy hình của chúng ra trong service_images
        //    foreach (var service in serviceModels)
        //    {
        //        var serviceImages = await _unitOfWorks.ServiceImageRepository.GetAll()
        //            .Where(si => si.ServiceId == service.ServiceId)
        //            .Select(si => si.image)
        //            .ToArrayAsync();

        //        service.images = serviceImages;
        //    }

        //    return serviceModels;
        //}

        public async Task<List<ServiceModel>> GetListImagesOfServices(List<Data.Entities.Service> listService)
        {
            var serviceIds = listService.Select(s => s.ServiceId).ToList();

            // Lấy toàn bộ hình ảnh trong 1 truy vấn
            var serviceImagesDict = await _unitOfWorks.ServiceImageRepository.GetAll()
                .Where(si => serviceIds.Contains(si.ServiceId))
                .GroupBy(si => si.ServiceId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(i => i.image).ToArray()
                );

            var serviceModels = _mapper.Map<List<ServiceModel>>(listService);

            // Gắn hình ảnh tương ứng vào từng serviceModel
            foreach (var service in serviceModels)
            {
                if (serviceImagesDict.TryGetValue(service.ServiceId, out var images))
                {
                    service.images = images;
                }
                else
                {
                    service.images = Array.Empty<string>();
                }
            }

            return serviceModels;
        }


        public async Task<GetAllServicePaginationResponse> GetAllServiceForBranch(int page, int pageSize, int branchId,
            int? serviceCategoryId)
        {
            // Lấy danh sách ServiceId thuộc branch
            var serviceIdsOfBranch = await _unitOfWorks.Branch_ServiceRepository
                .FindByCondition(bs => bs.BranchId == branchId)
                .OrderByDescending(x => x.Id)
                .Select(bs => bs.ServiceId)
                .ToListAsync();

            if (!serviceIdsOfBranch.Any())
            {
                return new GetAllServicePaginationResponse
                {
                    data = new List<ServiceModel>(),
                    pagination = new Pagination
                    {
                        page = page,
                        totalPage = 0,
                        totalCount = 0
                    }
                };
            }

            // Query lấy dịch vụ (lọc theo serviceCategoryId nếu có)
            var servicesQuery = _unitOfWorks.ServiceRepository.GetAll()
                .Include(x => x.ServiceCategory)
                .Where(s => serviceIdsOfBranch.Contains(s.ServiceId));

            if (serviceCategoryId.HasValue)
            {
                servicesQuery = servicesQuery.Where(s => s.ServiceCategoryId == serviceCategoryId.Value);
            }

            var totalCount = await servicesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var services = await servicesQuery
                .OrderBy(s => s.ServiceId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var serviceModels = _mapper.Map<List<ServiceModel>>(services);

            // Lấy toàn bộ ảnh liên quan đến danh sách service vừa lấy (tối ưu)
            var serviceIdsPaged = services.Select(s => s.ServiceId).ToList();

            var serviceImagesDict = await _unitOfWorks.ServiceImageRepository.GetAll()
                .Where(si => serviceIdsPaged.Contains(si.ServiceId))
                .GroupBy(si => si.ServiceId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(si => si.image).ToArray());

            // Gán ảnh vào từng serviceModel
            foreach (var service in serviceModels)
            {
                if (serviceImagesDict.TryGetValue(service.ServiceId, out var images))
                {
                    service.images = images;
                }
                else
                {
                    service.images = Array.Empty<string>();
                }
            }

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


        public async Task<ServiceModel> GetServiceByIdAsync(int id)
        {
            // Truy vấn dịch vụ và bao gồm thông tin danh mục
            var service = await _unitOfWorks.ServiceRepository
                .FindByCondition(s => s.ServiceId == id && s.Status == "Active")
                .Include(x => x.ServiceCategory)
                .FirstOrDefaultAsync();

            // Kiểm tra nếu không tìm thấy dịch vụ
            if (service == null)
                return null;

            var serviceModel = _mapper.Map<ServiceModel>(service);

            var serviceImages = await _unitOfWorks.ServiceImageRepository
                .FindByCondition(x => x.ServiceId == serviceModel.ServiceId)
                .Where(si => si.ServiceId == service.ServiceId)
                .Select(si => si.image)
                .ToArrayAsync();

            serviceModel.images = serviceImages;
            return serviceModel;
        }


        public async Task<ServiceDto> CreateServiceAsync(ServiceCreateDto serviceDto)
        {
            // Kiểm tra nếu DTO bị null
            if (serviceDto == null)
            {
                throw new BadRequestException("Service data is required.");
            }
            
            if(serviceDto.Price <= 0)
            {
                throw new BadRequestException("Price must be greater than 0.");
            }

            if (serviceDto.ServiceCategoryId == null || serviceDto.ServiceCategoryId == 0)
            {
                throw new BadRequestException("ServiceCategoryId is required.");
            }

            // Tạo thực thể Service từ DTO
            var service = new Data.Entities.Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Price = serviceDto.Price,
                Duration = serviceDto.Duration,
                Steps = serviceDto.Steps != null ? string.Join(",", serviceDto.Steps) : null,
                ServiceCategoryId = serviceDto.ServiceCategoryId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Status = ObjectStatus.Active.ToString(),
            };

            // Thêm service vào cơ sở dữ liệu
            await _unitOfWorks.ServiceRepository.AddAsync(service);
            await _unitOfWorks.ServiceRepository.Commit();

            var result = _mapper.Map<ServiceDto>(service);

            // Tạo danh sách hình ảnh từ DTO
            if (serviceDto.images != null)
            {
                // Bắt đầu tải lên hình ảnh đồng thời
                var uploadTasks = serviceDto.images?.Select(image => _cloudianryService.UploadImageAsync(image))
                    .ToList();
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

                result.images = listServiceImages.Select(x => x.image).ToArray();
            }

            return result;
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

            if (serviceDto.images != null && serviceDto.images.Count > 0)
            {
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
            }
            _unitOfWorks.ServiceRepository.Update(service);
            await _unitOfWorks.ServiceRepository.Commit();

            return _mapper.Map<ServiceDto>(service);
        }

        public async Task<Server.Data.Entities.Service> DeleteServiceAsync(int id)
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


        public async Task<List<Server.Data.Entities.Service>> GetTop4FeaturedServicesAsync()
        {
            // Lấy danh sách Appointments có trạng thái Confirmed và bao gồm Service cùng các quan hệ cần thiết
            var appointments = await _unitOfWorks.AppointmentsRepository
                .FindByCondition(a => a.Status == "Completed")
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
                .Take(5) // Lấy 4 dịch vụ nổi bật
                .ToList();

            return featuredServices;
        }

        public async Task<GrossDTO> CheckInputHasGross(string name)
        {
            var result = await _gptService.GetGross(name);
            GrossDTO gross = new GrossDTO()
            {
                Grosses = result,
                HasGross = result != null && result.Count > 0
            };
            return gross;
        }

        public async Task<List<BranchDTO>> GetBranchesOfService(int serviceId)
        {
            var branchServices = await _unitOfWorks.Branch_ServiceRepository.GetAll()
                .Where(bs => bs.ServiceId == serviceId)
                .Include(bs => bs.Branch)
                .Select(bs => bs.Branch)
                .ToListAsync();

            var branchDtos = _mapper.Map<List<BranchDTO>>(branchServices);
            return branchDtos;
        }


        public async Task<List<ServiceModel>> GetListServiceByBranchId(int branchId, int? serviceCategoryId)
        {
            // Lấy danh sách ServiceId thuộc branchId
            var serviceIds = await _unitOfWorks.Branch_ServiceRepository.GetAll()
                .Where(bs => bs.BranchId == branchId)
                .Select(bs => bs.ServiceId)
                .ToListAsync();

            // Nếu không có service nào liên kết với branch này, trả về danh sách rỗng
            if (!serviceIds.Any())
            {
                return new List<ServiceModel>();
            }

            // Tạo truy vấn service từ danh sách serviceIds
            var servicesQuery = _unitOfWorks.ServiceRepository.GetAll()
                .Where(s => serviceIds.Contains(s.ServiceId));

            // Nếu có lọc theo serviceCategoryId thì thêm điều kiện
            if (serviceCategoryId.HasValue)
            {
                servicesQuery = servicesQuery.Where(s => s.ServiceCategoryId == serviceCategoryId.Value);
            }

            // Thực thi truy vấn
            var services = await servicesQuery.ToListAsync();

            // Map sang ServiceModel
            var serviceModels = _mapper.Map<List<ServiceModel>>(services);
            return serviceModels;
        }

        public async Task<object> AssignOrUpdateBranchServices(AssignServiceToBranchRequest request)
        {
            // Kiểm tra branch
            var branch = await _unitOfWorks.BranchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
                return new { success = false, result = new { message = "Branch không tồn tại." } };
            
            // Lấy danh sách dịch vụ hiện có của branch
            var oldServices = await _unitOfWorks.Branch_ServiceRepository
                .FindByCondition(x => x.BranchId == request.BranchId)
                .ToListAsync();

            // Lấy các serviceId cũ
            var oldServiceIds = oldServices.Select(s => s.ServiceId).ToList();

            // 1. Xóa các service không còn
            var toRemove = oldServices.Where(s => !request.ServiceIds.Contains(s.ServiceId)).ToList();
            if (toRemove.Any())
                await _unitOfWorks.Branch_ServiceRepository.RemoveRangeAsync(toRemove);

            // 2. Thêm các service mới chưa có
            var toAdd = request.ServiceIds.Except(oldServiceIds)
                .Select(id => new Branch_Service
                {
                    BranchId = request.BranchId,
                    ServiceId = id,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                }).ToList();

            if (toAdd.Any())
                await _unitOfWorks.Branch_ServiceRepository.AddRangeAsync(toAdd);

            await _unitOfWorks.Branch_ServiceRepository.Commit();

            return new
            {
                success = true,
                result = new
                {
                    message = "Cập nhật dịch vụ cho chi nhánh thành công."
                }
            };
        }

        public async Task<List<ServiceModel>> GetServicesByRoutineAndUserAsync(int routineId, int userId)
        {
            // Kiểm tra routine có thuộc user không
            var isOwnedByUser = await _unitOfWorks.UserRoutineRepository
                .FindByCondition(ur => ur.RoutineId == routineId && ur.UserId == userId)
                .AnyAsync();

            if (!isOwnedByUser)
                throw new BadRequestException("Routine không tồn tại hoặc không thuộc về người dùng.");

            // Lấy danh sách ServiceId liên kết với Routine
            var serviceIds = await _unitOfWorks.ServiceRoutineRepository
                .FindByCondition(sr => sr.RoutineId == routineId)
                .Select(sr => sr.ServiceId)
                .ToListAsync();

            // Nếu không có dịch vụ nào => trả về rỗng
            if (!serviceIds.Any())
                return new List<ServiceModel>();

            // Lấy danh sách dịch vụ
            var services = await _unitOfWorks.ServiceRepository
                .FindByCondition(s => serviceIds.Contains(s.ServiceId))
                .Include(s => s.ServiceCategory)
                .ToListAsync();

            // Map sang model + lấy ảnh
            var serviceModels = await GetListImagesOfServices(services);
            return serviceModels;
        }

        public async Task<GetBranchesHasServiceResponse<GetBranchesHasService>> GetBranchesHasService(int serviceId)
        {
            var service = await _unitOfWorks.ServiceRepository
                              .FirstOrDefaultAsync(x => x.ServiceId == serviceId)
                          ?? throw new BadRequestException("Không tìm thấy thông tin dịch vụ.");

            var branchServices = await _unitOfWorks.Branch_ServiceRepository
                .FindByCondition(x => x.ServiceId == serviceId)
                .Include(x => x.Branch)
                .Include(x => x.Service)
                .ToListAsync();

            var branchDtos = branchServices.Select(x => new BranchServiceDto
            {
                Id = x.Id,
                Service = _mapper.Map<ServiceDto>(x.Service),
                Branch = _mapper.Map<BranchDTO>(x.Branch),
                Status = x.Status,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate
            }).ToList();

            var result = new GetBranchesHasService
            {
                ServiceId = serviceId,
                Branches = branchDtos
            };

            return new GetBranchesHasServiceResponse<GetBranchesHasService>
            {
                Message = "Lấy danh sách chi nhánh có dịch vụ thành công",
                Data = new List<GetBranchesHasService> { result }
            };
        }

    }
}
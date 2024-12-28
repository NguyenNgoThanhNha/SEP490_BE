using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Server.API.Extensions;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;
using System.Linq.Expressions;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ServiceService _serviceService;
        private readonly IElasticClient _elasticClient;
        private readonly ElasticService<ServiceDto> _elasticService;

        public ServiceController(ServiceService serviceService, IElasticClient elasticClient)
        {
            _serviceService = serviceService;
            _elasticClient = elasticClient;
            _elasticService = new ElasticService<ServiceDto>(_elasticClient, "services");
        }
        
        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(int page = 1, int pageSize = 10)
        {
            try
            {
                var services = await _serviceService.GetListAsync(pageIndex: page - 1, pageSize: pageSize);

                if (services.Data == null || !services.Data.Any())
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "No services found."
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Services retrieved successfully.",
                    data = services
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"An error occurred while retrieving services: {ex.Message}"
                }));
            }
        }
        
        [HttpGet("get-all-services")]
        public async Task<IActionResult> Get([FromQuery] int page = 1, int pageSize = 6)
        {
            try
            {
                // Gọi Service để lấy danh sách dịch vụ
                var services = await _serviceService.GetAllService(page, pageSize);

                // Kiểm tra nếu không có dữ liệu
                if (services.data == null || !services.data.Any())
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "No services found."
                    }));
                }

                services.message = "Services retrieved successfully.";
                return Ok(ApiResult<GetAllServicePaginationResponse>.Succeed(services));
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về phản hồi lỗi
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"An error occurred while retrieving services: {ex.Message}"
                }));
            }
        }
        
        [HttpGet("get-all-services-for-branch")]
        public async Task<IActionResult> GetAllServiceForBranch([FromQuery] int branchId ,int page = 1, int pageSize = 6)
        {
            try
            {
                // Gọi Service để lấy danh sách dịch vụ
                var services = await _serviceService.GetAllServiceForBranch(page,pageSize, branchId);

                // Kiểm tra nếu không có dữ liệu
                if (services.data == null || !services.data.Any())
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "No services found."
                    }));
                }

                services.message = "Services retrieved successfully.";
                return Ok(ApiResult<GetAllServicePaginationResponse>.Succeed(services));
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về phản hồi lỗi
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"An error occurred while retrieving services: {ex.Message}"
                }));
            }
        }


        [HttpGet("get-service-by-id")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            try
            {
                // Gọi Service để lấy dữ liệu dịch vụ theo ID
                var service = await _serviceService.GetServiceByIdAsync(id);

                // Kiểm tra nếu dịch vụ không tồn tại
                if (service == null)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Service not found."
                    }));
                }

                // Trả về dữ liệu nếu tìm thấy
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Service retrieved successfully.",
                    data = service
                }));
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về phản hồi lỗi
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"An error occurred: {ex.Message}"
                }));
            }
        }

        [HttpGet("elasticsearch")]
        public async Task<IActionResult> ElasticSearch(string? keyword)
        {
            var list = new List<ServiceDto>();
            if (!string.IsNullOrEmpty(keyword))
            {
                list = (await _elasticService.SearchAsync(keyword)).ToList();
            }
            else
                list = (await _elasticService.GetAllAsync()).ToList();
            return Ok(ApiResponse.Succeed(list));
        }

        [HttpPost("create-elastic")]
        public async Task<IActionResult> CreateElastic(ServiceDto model)
        {
            try
            {
                await _elasticService.IndexDocumentAsync(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(model);
        }

        [HttpPost("import-elastic")]
        public async Task<IActionResult> ImportElasticAsync(IFormFile file)
        {
            try
            {
                var result = await _elasticService.ImportFromJsonFileAsync(file);
                return Ok(ApiResponse.Succeed(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-service")]
        public async Task<IActionResult> CreateService([FromBody] ServiceCreateDto serviceDto)
        {
            try
            {
                // Kiểm tra nếu model không hợp lệ
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = string.Join(", ", errors)
                    }));
                }

                // Gọi Service để tạo mới dịch vụ
                var service = await _serviceService.CreateServiceAsync(serviceDto);

                // Trả về kết quả nếu thành công
                return CreatedAtAction(nameof(GetServiceById), new { id = service.ServiceId }, ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Service created successfully.",
                    data = service
                }));
            }
            catch (Exception ex)
            {
                // Xử lý lỗi ngoại lệ
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"An error occurred while creating the service: {ex.Message}"
                }));
            }
        }

        [Authorize(Roles = "Admin, Manager,Staff")]
        [HttpPut("update-service")]
        public async Task<IActionResult> UpdateService(int serviceId, [FromBody] ServiceUpdateDto serviceDto)
        {
            try
            {
                // Kiểm tra nếu model không hợp lệ
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Validation failed.",
                        data = errors
                    }));
                }

                // Gọi Service để cập nhật dịch vụ
                var updatedService = await _serviceService.UpdateServiceAsync(serviceDto, serviceId);

                // Kiểm tra nếu không tìm thấy dịch vụ
                if (updatedService == null)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Service not found."
                    }));
                }

                // Trả về kết quả nếu thành công
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Service updated successfully.",
                    data = updatedService
                }));
            }
            catch (Exception ex)
            {
                // Xử lý lỗi ngoại lệ
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"An error occurred while updating the service: {ex.Message}"
                }));
            }
        }

        [Authorize(Roles = "Admin, Manager,Staff")]
        [HttpDelete("delete-service")]
        public async Task<IActionResult> DeleteService(int serviceId)
        {
            try
            {
                // Thực hiện xóa dịch vụ
                var deletedService = await _serviceService.DeleteServiceAsync(serviceId);

                // Kiểm tra nếu không tìm thấy dịch vụ
                if (deletedService == null)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Service not found."
                    }));
                }

                // Trả về kết quả nếu thành công
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Service deleted successfully.",
                    data = new { ServiceId = deletedService.ServiceId } // Trả về ID của dịch vụ đã xóa
                }));
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"An error occurred while deleting the service: {ex.Message}"
                }));
            }
        }

    }
}

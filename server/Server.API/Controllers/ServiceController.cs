using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ServiceController(ServiceService serviceService)
        {
            _serviceService = serviceService;
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
        public async Task<IActionResult> Get([FromQuery] int page = 1)
        {
            try
            {
                // Gọi Service để lấy danh sách dịch vụ
                var services = await _serviceService.GetAllService(page);

                // Kiểm tra nếu không có dữ liệu
                if (services.data == null || !services.data.Any())
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "No services found."
                    }));
                }

                // Trả về dữ liệu nếu thành công
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Services retrieved successfully.",
                    data = new GetAllServicePaginationResponse
                    {
                        data = services.data,
                        pagination = services.pagination
                    }
                }));
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

        [Authorize(Roles = "Admin, Manager,Staff")]
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

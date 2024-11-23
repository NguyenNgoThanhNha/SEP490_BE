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
        public async Task<IActionResult> GetList(string? name,
                        string? description,
                        decimal? price,
                        decimal? endPrice,
                        int? filterTypePrice = 0,
                        int pageIndex = 0,
                        int pageSize = 10)
        {
            Expression<Func<Service, bool>> filter = null;
            filter = c => (string.IsNullOrEmpty(name) || c.Name.ToLower().Contains(name.ToLower()))
                && (string.IsNullOrEmpty(description) || c.Description.ToLower().Contains(description.ToLower()));

            Expression<Func<Service, bool>> priceFilter = null;
            if (price != null && price > 0)
            {
                if (filterTypePrice == 0 && (endPrice != null && endPrice > 0)) // khoảng
                {
                    priceFilter = c => c.Price >= price && c.Price <= endPrice;
                }
                else if (filterTypePrice == 1) // nhỏ hơn
                {
                    priceFilter = c => c.Price <= price;
                }
                else  // lớn hơn
                {
                    priceFilter = c => c.Price >= price;
                }
                filter = filter.And(priceFilter);
            }


            var response = await _serviceService.GetListAsync(
                filter: filter,
                pageIndex: pageIndex,
                pageSize: pageSize);
            return Ok(new ApiResult<Pagination<Service>>
            {
                Success = true,
                Result = response
            });
        }


        [HttpGet("get-all-services")]
        public async Task<IActionResult> Get([FromQuery] int page = 1)
        {
            var services = await _serviceService.GetAllService(page);
            return Ok(ApiResult<GetAllServicePaginationResponse>.Succeed(new GetAllServicePaginationResponse()
            {
                data = services.data,
                pagination = services.pagination
            }));
        }

        [HttpGet("get-service-by-id")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Service not found."
                    }));

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Service retrieved successfully.",
                    data = service
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail(ex));
            }
        }

        [HttpPost("create-service")]
        public async Task<IActionResult> CreateService([FromBody] ServiceCreateDto serviceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Validation failed",
                        data = errors
                    }));
                }

                var service = await _serviceService.CreateServiceAsync(serviceDto);
                return CreatedAtAction(nameof(GetServiceById), new { id = service.ServiceId }, ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Service created successfully.",
                    data = service
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail(ex));
            }
        }

        [HttpPut("update-service")]
        public async Task<IActionResult> UpdateService(int serviceId, [FromBody] ServiceUpdateDto serviceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Validation failed",
                        data = errors
                    }));
                }

                // Bỏ qua kiểm tra ServiceId trong body
                var updatedService = await _serviceService.UpdateServiceAsync(serviceDto, serviceId);
                if (updatedService == null)
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Service not found."
                    }));

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Service updated successfully.",
                    data = updatedService
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail(ex));
            }
        }

        [HttpDelete("delete-service")]
        public async Task<IActionResult> DeleteService(int serviceId)
        {
            try
            {
                var deletedService = await _serviceService.DeleteServiceAsync(serviceId);
                if (deletedService == null)
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Service not found."
                    }));

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Service deleted successfully.",
                    data = new { ServiceId = deletedService.ServiceId } // Trả về ServiceId đã xóa
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail(ex));
            }
        }
    }
}

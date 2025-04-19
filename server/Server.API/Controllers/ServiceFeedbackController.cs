using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using Server.Business.Dtos;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceFeedbackController : Controller
    {
        private readonly ServiceFeedbackService _serviceFeedbackService;



        public ServiceFeedbackController(ServiceFeedbackService serviceFeedbackService)
        {
            _serviceFeedbackService = serviceFeedbackService;

        }

        [HttpGet("get-by-id/{serviceFeedbackId}")]
        public async Task<IActionResult> GetById(int serviceFeedbackId)
        {
            try
            {
                var result = await _serviceFeedbackService.GetByIdAsync(serviceFeedbackId);

                if (result == null)
                {
                    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                    {
                        message = "Không tìm thấy phản hồi dịch vụ với ID được cung cấp.",
                        data = new List<object>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Lấy phản hồi dịch vụ thành công!",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi hệ thống: {ex.Message}",
                    data = new List<object>()
                }));
            }
        }

        [HttpGet("get-by-service/{serviceId}")]
        public async Task<IActionResult> GetByServiceId(int serviceId)
        {
            try
            {
                var result = await _serviceFeedbackService.GetByServiceIdAsync(serviceId);

                if (result == null || !result.Any())
                {
                    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                    {
                        message = "Không tìm thấy phản hồi nào cho dịch vụ này.",
                        data = new List<object>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Lấy danh sách phản hồi theo ServiceId thành công!",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi hệ thống: {ex.Message}",
                    data = new List<object>()
                }));
            }
        }


        [HttpGet("get-all")]
public async Task<IActionResult> GetAll()
{
    var result = await _serviceFeedbackService.GetAllAsync();
    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
    {
        message = "Lấy danh sách phản hồi dịch vụ thành công!",
        data = result
    }));
}

[HttpPost("create")]
public async Task<IActionResult> Create([FromBody] ServiceFeedbackCreateDto dto)
{
    try
    {
        var result = await _serviceFeedbackService.CreateAsync(dto);
        return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
        {
            message = "Tạo phản hồi dịch vụ thành công!",
            data = result
        }));
    }
    catch (ArgumentException ex)
    {
        return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
        {
            message = ex.Message,
            data = new List<object>()
        }));
    }
}

[HttpPut("update/{serviceFeedbackId}")]
public async Task<IActionResult> Update(int serviceFeedbackId, [FromBody] ServiceFeedbackUpdateDto dto)
{
    var result = await _serviceFeedbackService.UpdateAsync(serviceFeedbackId, dto);
    if (result == null)
    {
        return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
        {
            message = "Không tìm thấy phản hồi dịch vụ để cập nhật!",
            data = new List<object>()
        }));
    }

    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
    {
        message = "Cập nhật phản hồi dịch vụ thành công!",
        data = result
    }));
}

[HttpDelete("delete/{serviceFeedbackId}")]
public async Task<IActionResult> Delete(int serviceFeedbackId)
{
    var success = await _serviceFeedbackService.DeleteAsync(serviceFeedbackId);
    if (!success)
    {
        return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
        {
            message = "Không tìm thấy phản hồi dịch vụ để xóa!",
            data = new List<object>()
        }));
    }

    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
    {
        message = "Xóa phản hồi dịch vụ thành công!",
        data = new List<object>()
    }));
}

    }
}

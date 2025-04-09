using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using Server.Data.UnitOfWorks;
using Server.Business.Dtos;
using Server.Data.Entities;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentFeedbackController : Controller
    {
        private readonly AppointmentFeedbackService _appointmentFeedbackService;      
      


        public AppointmentFeedbackController(AppointmentFeedbackService appointmentFeedbackService)
        {
            _appointmentFeedbackService = appointmentFeedbackService;         
           
        }

        [HttpGet("get-by-id/{appointmentFeedbackId}")]
        public async Task<IActionResult> GetById(int appointmentFeedbackId)
        {
            try
            {
                var result = await _appointmentFeedbackService.GetByIdAsync(appointmentFeedbackId);

                if (result == null)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Không tìm thấy phản hồi với ID được cung cấp.",
                        data = new List<object>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Lấy phản hồi thành công!",
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
            var result = await _appointmentFeedbackService.GetAllAsync();
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy tất cả phản hồi thành công!",
                data = result
            }));
        }

        [HttpPost("create")]        
        public async Task<IActionResult> Create([FromBody] AppointmentFeedbackCreateDto dto)
        {
            if (dto.AppointmentId <= 0 || dto.CustomerId <= 0 || dto.UserId <= 0 || dto.Rating <= 0)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "AppointmentId, CustomerId, UserId và Rating phải lớn hơn 0.",
                    data = new List<object>()
                }));
            }

            var result = await _appointmentFeedbackService.CreateAsync(dto);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Tạo phản hồi thành công!",
                data = result
            }));
        }


        [HttpPut("update/{appointmentFeedbackId}")]
        public async Task<IActionResult> Update(int appointmentFeedbackId, [FromBody] AppointmentFeedbackUpdateDto dto)
        {
            var result = await _appointmentFeedbackService.UpdateAsync(appointmentFeedbackId, dto);

            if (result == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy phản hồi để cập nhật!",
                    data = new List<object>()
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Cập nhật phản hồi thành công!",
                data = result
            }));
        }

        [HttpDelete("delete/{appointmentFeedbackId}")]
        public async Task<IActionResult> Delete(int appointmentFeedbackId)
        {
            var success = await _appointmentFeedbackService.DeleteAsync(appointmentFeedbackId);

            if (!success)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy phản hồi để xóa!",
                    data = new List<object>()
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Xóa phản hồi thành công!",
                data = new List<object>()
            }));
        }
    }
}

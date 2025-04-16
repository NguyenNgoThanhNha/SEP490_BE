using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Services;
using Server.Data.UnitOfWorks;
using Server.Business.Dtos;
using Server.Data.Entities;
using static Server.Business.Dtos.AppointmentFeedbackCreateDto;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentFeedbackController : Controller
    {
        private readonly AppointmentFeedbackService _appointmentFeedbackService;
        private readonly CloudianryService _cloudinaryService;



        public AppointmentFeedbackController(AppointmentFeedbackService appointmentFeedbackService, CloudianryService cloudinaryService)
        {
            _appointmentFeedbackService = appointmentFeedbackService;
            _cloudinaryService = cloudinaryService;

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

        [HttpGet("get-by-appointment/{appointmentId}")]
        public async Task<IActionResult> GetByAppointmentId(int appointmentId)
        {
            try
            {
                var result = await _appointmentFeedbackService.GetByAppointmentIdAsync(appointmentId);

                if (result == null || !result.Any())
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Không tìm thấy phản hồi nào cho cuộc hẹn này.",
                        data = new List<object>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Lấy danh sách phản hồi theo AppointmentId thành công!",
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
        public async Task<IActionResult> Create([FromForm] AppointmentFeedbackCreateFormDto dto)
        {
            if (dto.AppointmentId <= 0 || dto.CustomerId <= 0 || dto.StaffId <= 0)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "AppointmentId, CustomerId, StaffId là bắt buộc và phải lớn hơn 0.",
                    data = new List<object>()
                }));
            }

            if (dto.ImageBefore == null || dto.ImageAfter == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Vui lòng cung cấp cả ảnh trước và sau.",
                    data = new List<object>()
                }));
            }

           
            var imageBeforeUpload = await _cloudinaryService.UploadImageAsync(dto.ImageBefore);
            var imageAfterUpload = await _cloudinaryService.UploadImageAsync(dto.ImageAfter);

            if (imageBeforeUpload == null || imageAfterUpload == null)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Tải ảnh lên thất bại."
                }));
            }

          
            var createDto = new AppointmentFeedbackCreateDto
            {
                AppointmentId = dto.AppointmentId,
                CustomerId = dto.CustomerId,
                StaffId = dto.StaffId,
                Comment = dto.Comment,
                Rating = dto.Rating ?? 0,
                ImageBefore = imageBeforeUpload.SecureUrl.ToString(), 
                ImageAfter = imageAfterUpload.SecureUrl.ToString(),
                CreatedBy = "Customer"
            };

            var result = await _appointmentFeedbackService.CreateAsync(createDto);

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Tạo phản hồi thành công!",
                data = result
            }));
        }





        [HttpPatch("update/{appointmentFeedbackId}")]
        public async Task<IActionResult> PatchUpdate(int appointmentFeedbackId, [FromForm] AppointmentFeedbackUpdateFormDto dto)
        {
            var result = await _appointmentFeedbackService.PatchUpdateAsync(appointmentFeedbackId, dto);

            if (result == null)
            {
                return Ok(ApiResult<ApiResponse>.Error(new ApiResponse
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
                return Ok(ApiResult<ApiResponse>.Error(new ApiResponse
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

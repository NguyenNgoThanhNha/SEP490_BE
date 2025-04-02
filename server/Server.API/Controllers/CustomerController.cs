using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

       
        [HttpGet("check-points/{customerId}")]
        public async Task<IActionResult> CheckBonusPoints([FromRoute] int customerId)
        {
            try
            {
                var result = await _customerService.CheckBonusPoints(customerId);

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Kiểm tra điểm thưởng thành công!",
                    data = result
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = ex.Message
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = $"Lỗi máy chủ nội bộ: {ex.Message}"
                }));
            }
        }

      
        [HttpPost("exchange-points")]
        public async Task<IActionResult> ExchangePoints([FromBody] ExchangePointRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            try
            {
                var result = await _customerService.ExchangePoints(request);

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Đổi điểm thành công!",
                    data = result
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = ex.Message
                }));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = ex.Message
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = $"Lỗi máy chủ nội bộ: {ex.Message}"
                }));
            }
        }
    }
}

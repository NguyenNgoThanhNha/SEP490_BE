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

        /// <summary>
        /// API 1: Kiểm tra điểm thưởng hiện tại của khách hàng.
        /// </summary>
        /// <param name="userId">ID của khách hàng</param>
        [HttpGet("check-points/{customerId}")]
        public async Task<IActionResult> CheckBonusPoints([FromRoute] int customerId)
        {
            try
            {
                var result = await _customerService.CheckBonusPoints(customerId);

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Check bonus points successful!",
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
                    message = $"Internal server error: {ex.Message}"
                }));
            }
        }

        /// <summary>
        /// API 2: Đổi điểm thưởng lấy Promotion.
        /// </summary>
        /// <param name="request">Thông tin đổi điểm</param>
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
                    message = "Exchange points successful!",
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
                    message = $"Internal server error: {ex.Message}"
                }));
            }
        }

        [HttpGet("customer-busy-times")]
        public async Task<IActionResult> GetCustomerBusyTimes(int customerId, DateTime date)
        {
            try
            {
                var busyTimes = await _customerService.GetCustomerBusyTimesAsync(customerId, date);

                if (busyTimes == null || !busyTimes.Any())
                {
                    return NotFound(new
                    {
                        Message = "No busy times found for the customer on the specified date."
                    });
                }

                return Ok(new
                {
                    Message = "Successfully retrieved the customer's busy times!",
                    Data = busyTimes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = $"System error: {ex.Message}"
                });
            }
        }
    }
}

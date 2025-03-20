using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly VoucherService _voucherService;
        private readonly AuthService _authService;

        public VoucherController(VoucherService voucherService, AuthService authService)
        {
            _voucherService = voucherService;
            _authService = authService;
        }
        
        [Authorize]
        [HttpGet("get-all-vouchers")]
        public async Task<IActionResult> GetAllVouchers([FromQuery] VoucherRequest request)
        {
            var vouchers = await _voucherService.GetAllVouchers(request);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get voucher by date successfully",
                data = vouchers
            }));
        }
        
        [Authorize]
        [HttpGet("get-voucher-by-date")]
        public async Task<IActionResult> GetVoucherByDate([FromQuery] DateTime dateTime)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            // accessUser
            var currentUser = await _authService.GetUserInToken(tokenValue);
            var vouchers = await _voucherService.GetVoucherByDate(dateTime);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get voucher by date successfully",
                data = vouchers
            }));
        }
    }
}

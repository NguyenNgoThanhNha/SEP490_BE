using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.API.Controllers.Gaurd;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Constants;
using Server.Business.Services;
using Server.Data.Entities;

namespace Server.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AppDbContext _context;

        public UserController(UserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [Authorize(Roles = "Admin, Manager,Staff")]

        [HttpGet("get-customer-list")]
        public async Task<IActionResult> GetCustomerList(int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                // Lấy danh sách khách hàng qua service
                var response = await _userService.GetListAsync(
                    filter: c => c.RoleID == (int)RoleConstant.RoleType.Customer, // Chỉ lấy khách hàng (RoleID = 3)
                    pageIndex: pageIndex,
                    pageSize: pageSize);

                // Kiểm tra nếu không có khách hàng nào
                if (response.TotalItemsCount == 0)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "No customers found."
                    }));
                }

                // Trả về kết quả thành công
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Customer list retrieved successfully.",
                    data = response
                }));
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Error retrieving customer list: {ex.Message}"
                }));
            }
        }


        [Authorize(Roles = "Admin, Manager,Staff")]
        [HttpGet("get-account-detail")]
        public async Task<IActionResult> GetAccountDetail(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Username must not be null or empty."
                    }));
                }

                // Lấy chi tiết tài khoản
                var user = await _userService.GetAccountDetail(username);

                if (user == null)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "User not found."
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Account details retrieved successfully.",
                    data = user // Trực tiếp gán user vào data mà không gói thêm ApiResponse
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Error retrieving account details: {ex.Message}"
                }));
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Constants;
using Server.Business.Services;
using Server.Business.Ultils;
using Server.Data;
using Server.Data.Entities;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly MailService _mailService;

        public UserController(UserService userService, AppDbContext context, AuthService authService, MailService mailService)
        {
            _userService = userService;
            _context = context;
            _authService = authService;
            _mailService = mailService;
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
                        message = "Không tìm thấy khách hàng nào."
                    }));
                }

                // Trả về kết quả thành công
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Danh sách khách hàng đã được lấy thành công.",
                    data = response
                }));
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi khi truy xuất danh sách khách hàng: {ex.Message}"
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
                        message = "Tên người dùng không được để trống hoặc rỗng."
                    }));
                }

                // Lấy chi tiết tài khoản
                var user = await _userService.GetAccountDetail(username);

                if (user == null)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Không tìm thấy người dùng."
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Đã lấy thông tin tài khoản thành công.",
                    data = user // Trực tiếp gán user vào data mà không gói thêm ApiResponse
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi khi truy xuất thông tin tài khoản: {ex.Message}"
                }));
            }
        }
        
        
        [HttpPost("delete-account")]
        public async Task<IActionResult> RequestDeleteAccount(DeleteAccountRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            
            var user = await _authService.GetUserByEmail(req.Email);
            if (user == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Tài khoản không tồn tại"
                }));
            }
            
            if (user != null && user.UpdatedDate > DateTime.Now && user.OTPCode != "0")
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Mã OTP chưa hết hạn"
                }));
            }

            var otp = new Random().Next(100000, 999999);
            user.OTPCode = otp.ToString();
            user.UpdatedDate = DateTime.Now.AddMinutes(2);
            await _userService.UpdateUserInfo(user);
            
            var mailData = new MailData()
            {
                EmailToId = req.Email,
                EmailToName = user.FullName,
                EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <h2 style=""text-transform: uppercase; color: #e74c3c; margin-top: 20px; font-size: 24px; font-weight: bold;"">Account Deletion Request</h2>
    <div style=""font-size: 18px; color: #555; margin-bottom: 20px;"">Your OTP Code is: 
        <span style=""font-weight: bold; color: #e74c3c;"">{otp}</span>
    </div>
    <p style=""color: #888; font-size: 14px;"">If you did not request this, please ignore this email.</p>
    <p style=""color: #aaa; font-size: 12px; margin-top: 20px;"">Powered by Solace</p>
</div>",
                EmailSubject = "Confirm Account Deletion"
            };

            
            var result = await _mailService.SendEmailAsync(mailData, false);
            if (!result)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không gửi được email"
                }));
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Kiểm tra email để xác nhận OTP để xóa tài khoản của bạn."
            }));
        }


        [HttpPost("confirm-delete")]
        public async Task<IActionResult> ConfirmDeleteAccount(ConfirmDeleteRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            
            var user = await _authService.GetUserByEmail(req.Email);
            if (user == null || user.OTPCode != req.OTP)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "OTP không hợp lệ hoặc Tài khoản không tồn tại"
                }));
            }
            
            user.Status = ObjectStatus.InActive.ToString(); 
            user.OTPCode = "0"; 
            await _userService.UpdateUserInfo(user);
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Tài khoản đã bị vô hiệu hóa thành công."
            }));
        }
    }
}
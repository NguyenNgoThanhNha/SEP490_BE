using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Business.Services;
using Server.Business.Ultils;
using System.IdentityModel.Tokens.Jwt;
using Server.Data;
using Server.Data.UnitOfWorks;
using LoginRequest = Server.Business.Commons.Request.LoginRequest;
using Token = Server.Business.Ultils.Token;
using Server.Data.Entities;
using Server.Business.Worker;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService authService;
        private readonly MailService mailService;
        private readonly UserService _userService;
        private readonly IMapper _mapper;

        public AuthController(AuthService authService, MailService mailService, UserService userService, IMapper mapper)
        {
            this.authService = authService;
            this.mailService = mailService;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost("first-step")]
        public async Task<IActionResult> FirstStepResgisterInfo(FirstStepResquest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            var otp = 0;
            var Password = req.Password;
            var email = req.Email;
            var link = req.Link;
            var user = await authService.GetUserByEmail(email);
            if (user != null && user.OTPCode == "0")
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Tài khoản đã tồn tại"
                }));
            }

            if (user != null && user.CreateDate > DateTime.Now && user.OTPCode != "0")
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Mã OTP chưa hết hạn"
                }));
            }

            if (user == null)
            {
                otp = new Random().Next(100000, 999999);
                var href = link + req.Email;
                var mailData = new MailData()
                {
                    EmailToId = email,
                    EmailToName = "KayC",
                    EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTRDn7YDq7gsgIdHOEP2_Mng6Ym3OzmvfUQvQ&usqp=CAU"" alt=""Noto Image"" style=""max-width: 100px; height: auto; display: block; margin: 0 auto; border-radius: 50%;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Welcome to Team Solace</h2>
    <a href=""{href}"" style=""display: inline-block; background-color: #3498db; color: #fff; text-decoration: none; padding: 10px 20px; border-radius: 5px; margin-bottom: 20px;"">Click here to verify</a>
    <div style=""font-size: 18px; color: #555; margin-bottom: 30px;"">Your OTP Code is: <span style=""font-weight: bold; color: #e74c3c;"">{otp}</span></div>
    <p style=""color: #888; font-size: 14px;"">Powered by Team Solace</p>
</div>",
                    EmailSubject = "OTP Verification"
                };


                var result = await mailService.SendEmailAsync(mailData, false);
                if (!result)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Gửi email không thành công"
                    }));
                }
            }
            var createUserModel = new UserModel
            {
                Email = req.Email,
                OTPCode = otp.ToString(),
                Password = Password,
                UserName = req.UserName,
                FullName = req.FullName,
                Address = req.Address,
                City = req.City,
                PhoneNumber = req.Phone
            };
            var userModel = await authService.FirstStep(createUserModel);

            if (userModel.OTPCode != otp.ToString())
            {
                var href = link + req.Email;
                var mailUpdateData = new MailData()
                {
                    EmailToId = email,
                    EmailToName = "KayC",
                    EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTRDn7YDq7gsgIdHOEP2_Mng6Ym3OzmvfUQvQ&usqp=CAU"" alt=""Noto Image"" style=""max-width: 100px; height: auto; display: block; margin: 0 auto; border-radius: 50%;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Welcome to Team Solace</h2>
    <a href=""{href}"" style=""display: inline-block; background-color: #3498db; color: #fff; text-decoration: none; padding: 10px 20px; border-radius: 5px; margin-bottom: 20px;"">Click here to verify</a>
    <div style=""font-size: 18px; color: #555; margin-bottom: 30px;"">Your OTP Code is: <span style=""font-weight: bold; color: #e74c3c;"">{userModel.OTPCode}</span></div>
    <p style=""color: #888; font-size: 14px;"">Powered by Team Solace</p>
</div>",
                    EmailSubject = "OTP Verification"
                };
                var rsUpdate = await mailService.SendEmailAsync(mailUpdateData, false);
                if (!rsUpdate)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Gửi email không thành công"
                    }));
                }
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Kiểm tra Email và Xác minh OTP",
            }));
        }

        [HttpPost("submit-otp")]
        public async Task<IActionResult> SubmitOTP(SubmitOTPResquest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            var result = await authService.SubmitOTP(req);
            if (!result)
            {
                throw new BadRequestException("Mã OTP không đúng");
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = $"Xác minh tài khoản thành công cho email: {req.Email}"
            }));
        }


        [HttpGet("get-time-otp")]
        public async Task<IActionResult> GetTimeOTP(string email)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse.Error(string.Join(", ", errors)));
            }
            var user = await authService.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy người dùng"
                }));
            }

            else
            {
                DateTimeOffset utcTime = DateTimeOffset.Parse(user.CreateDate.ToString());
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Success",
                    data = utcTime
                }));

            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var loginResult = await authService.SignIn(req.Identifier, req.Password);
            if (loginResult.Token == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Tên người dùng hoặc mật khẩu không hợp lệ"
                }));
            }

            var handler = new JwtSecurityTokenHandler();

            var res = new ApiResponse()
            {
                message = "Đăng nhập thành công",
                data = handler.WriteToken(loginResult.Token)
            };
            return Ok(ApiResult<ApiResponse>.Succeed(res));
        }

        [AllowAnonymous]
        [HttpPost("login-mobile")]
        public async Task<IActionResult> LoginMobile([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var loginResult = await authService.SignIn(req.Identifier, req.Password);
            if (loginResult.Token == null)
            {
                var result = ApiResponse.Error("Tên người dùng hoặc mật khẩu không hợp lệ");
                return BadRequest(ApiResult<ApiResponse>.Error(result));
            }

            var handler = new JwtSecurityTokenHandler();

            var res = new ApiResponse
            {
                message = "Đăng nhập thành công",
                data = new Token()
                {
                    accessToken = handler.WriteToken(loginResult.Token),
                    refreshToken = handler.WriteToken(loginResult.Refresh)
                }
            };
            return Ok(ApiResult<ApiResponse>.Succeed(res));
        }

        [AllowAnonymous]
        [HttpPost("login-google")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] LoginWithGGRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse.Error(string.Join(", ", errors)));
            }

            var loginResult = await authService.SignInWithGG(req);

            if (loginResult.Authenticated)
            {
                var handler = new JwtSecurityTokenHandler();

                var res = new ApiResponse()
                {
                    message = "Đăng nhập thành công",
                    data = handler.WriteToken(loginResult.Token)
                };
                return Ok(ApiResult<ApiResponse>.Succeed(res));
            }

            return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Lỗi khi đăng nhập bằng Google!"
            }));
        }

        [AllowAnonymous]
        [HttpPost("login-facebook")]
        public async Task<IActionResult> LoginWithFaceBook([FromBody] LoginWithGGRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var loginResult = await authService.SignInWithFacebook(req);

            if (loginResult.Authenticated)
            {
                var handler = new JwtSecurityTokenHandler();

                var res = new ApiResponse()
                {
                    message = "Đăng nhập thành công",
                    data = handler.WriteToken(loginResult.Token)
                };
                return Ok(ApiResult<ApiResponse>.Succeed(res));
            }

            return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
            {
                message = "Lỗi khi đăng nhập bằng Google!"
            }));
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Dữ liệu yêu cầu không hợp lệ!"
                }));
            }

            var user = await authService.GetUserByEmail(email);
            if (user == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy tài khoản!"
                }));
            }

            var result = await authService.ForgotPass(user);
            if (result == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tạo được OTP!"
                }));
            }

            var mailUpdateData = new MailData()
            {
                EmailToId = email,
                EmailToName = "KayC",
                EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTRDn7YDq7gsgIdHOEP2_Mng6Ym3OzmvfUQvQ&usqp=CAU"" alt=""Noto Image"" style=""max-width: 100px; height: auto; display: block; margin: 0 auto; border-radius: 50%;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Welcome to Team Solace</h2>
    <div style=""font-size: 18px; color: #555; margin-bottom: 30px;"">Your OTP Code is: <span style=""font-weight: bold; color: #e74c3c;"">{result.OTPCode}</span></div>
    <p style=""color: #888; font-size: 14px;"">Powered by Team Solace</p>
</div>",
                EmailSubject = "OTP Verification"
            };
            var rsUpdate = await mailService.SendEmailAsync(mailUpdateData, false);
            if (!rsUpdate)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không gửi được email!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đã gửi OTP thành công!"
            }));
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromQuery] string email, [FromBody] UpdatePasswordRequest req)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(req.Password) || string.IsNullOrEmpty(req.ConfirmPassword))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Dữ liệu yêu cầu không hợp lệ!"
                }));
            }

            if (req.Password != req.ConfirmPassword)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Mật khẩu không khớp!"
                }));
            }

            var result = await authService.UpdatePass(email, req.Password);

            if (result)
            {
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Đã cập nhật mật khẩu thành công!"
                }));
            }
            else
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không cập nhật được mật khẩu! Vui lòng đảm bảo xác minh mã OTP để cập nhật mật khẩu mới!"
                }));
            }
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Dữ liệu yêu cầu không hợp lệ!"
                }));
            }

            var user = await authService.GetUserByEmail(email);
            if (user == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy tài khoản!"
                }));
            }

            if (user != null && user.CreateDate > DateTime.Now && user.OTPCode != "0")
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Mã OTP chưa hết hạn!"
                }));
            }

            var result = await authService.ResendOtp(email);

            if (result != null)
            {
                var mailData = new MailData()
                {
                    EmailToId = email,
                    EmailToName = user?.FullName,
                    EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTRDn7YDq7gsgIdHOEP2_Mng6Ym3OzmvfUQvQ&usqp=CAU"" alt=""Noto Image"" style=""max-width: 100px; height: auto; display: block; margin: 0 auto; border-radius: 50%;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Welcome to Team Solace</h2>
    <div style=""font-size: 18px; color: #555; margin-bottom: 30px;"">Your OTP Code is: <span style=""font-weight: bold; color: #e74c3c;"">{result.OTPCode}</span></div>
    <p style=""color: #888; font-size: 14px;"">Powered by Team Solace</p>
</div>",
                    EmailSubject = "Resend OTP Verification"
                };

                var emailSent = await mailService.SendEmailAsync(mailData, false);
                if (emailSent)
                {
                    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                    {
                        message = "Đã gửi lại OTP thành công!"
                    }));
                }
                else
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Không gửi được email!"
                    }));
                }
            }
            else
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không gửi lại được OTP!"
                }));
            }
        }

        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            // Lấy token từ header
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
            var currentUser = await authService.GetUserInToken(tokenValue);
            // Retrieve the refresh token from the cookie
            var refreshToken = currentUser.RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không cung cấp mã thông báo làm mới, vui lòng đăng nhập lại!"
                }));
            }

            // refreshUser
            var refreshUser = await _userService.GetUserInToken(refreshToken);
            if (currentUser.UserId != refreshUser.UserId)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Mã truy cập không hợp lệ!"
                }));
            }

            try
            {
                // Call the service to validate and refresh the tokens
                var loginResult = await authService.RefreshToken(refreshToken);

                if (!loginResult.Authenticated)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Mã thông báo làm mới đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại"
                    }));
                }

                // Return the new access token
                var handler = new JwtSecurityTokenHandler();
                var res = new ApiResponse
                {
                    message = "Mã thông báo đã được làm mới thành công",
                    data = handler.WriteToken(loginResult.Token)
                };

                return Ok(ApiResult<ApiResponse>.Succeed(res));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = ex.Message
                }));
            }
        }

        [Authorize]
        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            // Lấy token từ header
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
            var currentUser = await authService.GetUserInToken(tokenValue);
            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin người dùng!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thông tin người dùng thành công!",
                data = _mapper.Map<UserDTO>(currentUser)
            }));
        }

        [HttpPost("create-account-with-phone")]
        public async Task<IActionResult> CreateAccountWithPhone([FromBody] CreateAccountWithPhoneRequest request)
        {
            var result = await authService.CreateAccountWithPhone(request.PhoneNumber, request.UserName, request?.Password, request?.Email);
            if (result == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tạo được tài khoản!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tài khoản đã được tạo thành công!",
                data = result
            }));
        }

        [HttpGet("get-user-by-phone-email")]
        public async Task<IActionResult> GetUserByPhoneEmail([FromQuery] GetUserByPhoneEmailRequest request)
        {
            var user = await authService.CheckExistAccount(request.Phone, request.Email);
            if (user == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy người dùng!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thông tin người dùng thành công!",
                data = user
            }));
        }

        [Authorize]
        [HttpPost("update-user-info")]
        public async Task<IActionResult> UpdateUserInfo([FromForm] UserInfoUpdateRequest request)
        {
            var result = await authService.UpdateUserInfoModel(request);
            if (result == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không cập nhật được thông tin người dùng!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Cập nhật thông tin người dùng thành công!",
                data = result
            }));
        }

        [HttpGet("revenue-by-branch")]
        public async Task<IActionResult> GetRevenueByBranch([FromQuery] int month, [FromQuery] int year)
        {
            var result = await authService.GetRevenueByBranchAsync(month, year);

            if (result == null || !result.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không có dữ liệu doanh thu trong tháng/năm này.",
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy doanh thu các chi nhánh thành công!",
                data = result
            }));
        }

        [HttpGet("top-3-revenue-branches")]
        public async Task<IActionResult> GetTop3Branches([FromQuery] int month, [FromQuery] int year)
        {
            var result = await authService.GetTop3RevenueBranchesAsync(month, year);

            if (result == null || !result.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không có dữ liệu doanh thu cho tháng/năm được chọn."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Top 3 chi nhánh có doanh thu cao nhất!",
                data = result
            }));
        }
        [HttpPost("create-admin-manager")]
        public async Task<IActionResult> CreateUser([FromBody] CreateAdminManagerRequestDto request)
        {
            // Gọi hàm CreateUserAsync từ AuthService để tạo user
            var createdUser = await authService.CreateUserAsync(request);

            return Ok(new
            {
                Message = "User created successfully",
                Data = new
                {
                    createdUser.UserId                    
                }
            });
        }

        [HttpPost("update-status-order-routine")] //all step of userroutinestep completed --> update order status (routine)
        public async Task<IActionResult> RunUserRoutineStatusUpdate([FromServices] UserRoutineStatusUpdateService userRoutineStatusUpdateService)
        {
            try
            {
                await userRoutineStatusUpdateService.ManualRunAsync();
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Đã chạy cập nhật trạng thái Routine thành công!"
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi khi chạy cập nhật Routine: {ex.Message}"
                }));
            }
        }

        [HttpPost("run-user-routine-step-update")]
        public async Task<IActionResult> RunUserRoutineStepUpdate(
    [FromServices] UserRoutineStepStatusUpdateService userRoutineStepStatusUpdateService)
        {
            try
            {
                // Tạo một method ManualRunAsync() trong service nếu chưa có để tái sử dụng logic
                await userRoutineStepStatusUpdateService.ManualRunAsync();

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Đã chạy cập nhật bước routine và gửi nhắc nhở thành công!"
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi khi chạy nhắc nhở routine: {ex.Message}"
                }));
            }
        }

        [HttpPost("run-order-appointment-update")]
        public async Task<IActionResult> RunOrderStatusUpdate(
    [FromServices] OrderStatusUpdateService orderStatusUpdateService)
        {
            try
            {
                await orderStatusUpdateService.ManualRunAsync();

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Đã chạy cập nhật trạng thái đơn hàng thành công!"
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}"
                }));
            }
        }

        [HttpPost("run-appointment-missing-staff-reminder")]
        public async Task<IActionResult> RunMissingStaffReminderAsync(
      [FromServices] AppointmentReminderNoRealStaffWorker appointmentReminderNoRealStaffWorker)
        {
            try
            {
                await appointmentReminderNoRealStaffWorker.ManualRunAsync();

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Đã chạy nhắc nhở lịch hẹn chưa có nhân viên thực tế thành công!"
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi khi chạy nhắc nhở lịch hẹn: {ex.Message}"
                }));
            }
        }
    }
}

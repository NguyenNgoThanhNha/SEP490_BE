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
                    message = "Account Already Exists"
                }));
            }

            if (user != null && user.CreateDate > DateTime.Now && user.OTPCode != "0")
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "OTP Code is not expired"
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
                        message = "Send Email Fail"
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
                        message = "Send Email Fail"
                    }));
                }
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Check Email and Verify OTP",
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
                throw new BadRequestException("OTP Code is not Correct");
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = $"Verify Account Success for email: {req.Email}"
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
                    message = "User Not Found"
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
                    message = "Username or password is invalid"
                }));
            }

            var handler = new JwtSecurityTokenHandler();

            var res = new ApiResponse()
            {
                message = "Sign In Successfully",
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
                var result = ApiResponse.Error("Username or password is invalid");
                return BadRequest(ApiResult<ApiResponse>.Error(result));
            }

            var handler = new JwtSecurityTokenHandler();

            var res = new ApiResponse
            {
                message = "Sign In Successfully",
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
                    message = "Sign In Successfully",
                    data = handler.WriteToken(loginResult.Token)
                };
                return Ok(ApiResult<ApiResponse>.Succeed(res));
            }

            return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Error in login with Google!"
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
                    message = "Sign In Successfully",
                    data = handler.WriteToken(loginResult.Token)
                };
                return Ok(ApiResult<ApiResponse>.Succeed(res));
            }

            return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
            {
                message = "Error in login with Google!"
            }));
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Invalid request data!"
                }));
            }

            var user = await authService.GetUserByEmail(email);
            if (user == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Account not found!"
                }));
            }

            var result = await authService.ForgotPass(user);
            if (result == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Failed to generate OTP!"
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
                    message = "Failed to send email!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "OTP sent successfully!"
            }));
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromQuery] string email, [FromBody] UpdatePasswordRequest req)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(req.Password) || string.IsNullOrEmpty(req.ConfirmPassword))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Invalid request data!"
                }));
            }

            if (req.Password != req.ConfirmPassword)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Passwords do not match!"
                }));
            }

            var result = await authService.UpdatePass(email, req.Password);

            if (result)
            {
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Password updated successfully!"
                }));
            }
            else
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Failed to update password! Please make sure verify otp code to update new password!"
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
                    message = "Invalid request data!"
                }));
            }

            var user = await authService.GetUserByEmail(email);
            if (user == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Account not found!"
                }));
            }

            if (user != null && user.CreateDate > DateTime.Now && user.OTPCode != "0")
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "OTP Code is not expired!"
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
                        message = "OTP resent successfully!"
                    }));
                }
                else
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Failed to send email!"
                    }));
                }
            }
            else
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Failed to resend OTP!"
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
                    message = "No refresh token provided, Please login again!"
                }));
            }

            // refreshUser
            var refreshUser = await _userService.GetUserInToken(refreshToken);
            if (currentUser.UserId != refreshUser.UserId)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "AccessToken are invalid!"
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
                        message = "Refresh token expired or invalid, please log in again"
                    }));
                }

                // Return the new access token
                var handler = new JwtSecurityTokenHandler();
                var res = new ApiResponse
                {
                    message = "Token refreshed successfully",
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
                    message = "User info not found!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get user info success!",
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
                    message = "Failed to create account!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Account created successfully!",
                data = result
            }));
        }
    }
}

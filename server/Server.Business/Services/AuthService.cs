﻿using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Business.Ultils;
using Server.Data;
using Server.Data.Entities;
using Server.Data.Helpers;
using Server.Data.MongoDb.Models;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class AuthService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly JwtSettings _jwtSettings;
    private readonly IMapper _mapper;
    private readonly CloudianryService _cloudianryService;
    private readonly MongoDbService _mongoDbService;

    public AuthService(UnitOfWorks unitOfWorks, JwtSettings jwtSettings, IMapper mapper, CloudianryService cloudianryService, MongoDbService mongoDbService)
    {
        _unitOfWorks = unitOfWorks;
        _jwtSettings = jwtSettings;
        _mapper = mapper;
        _cloudianryService = cloudianryService;
        _mongoDbService = mongoDbService;
    }

    public async Task<UserModel> FirstStep(UserModel req)
    {
        var userEntity = _mapper.Map<User>(req);
        var user = _unitOfWorks.UserRepository.FindByCondition(x => x.Email == req.Email).FirstOrDefault();

        if (user != null && user.OTPCode != "0" && user.Status == "InActive")
        {
            user.Status = "InActive";
            user.CreateDate = DateTimeOffset.Now.AddMinutes(2);
            user.BirthDate = DateTime.Now;
            user.Password = SecurityUtil.Hash(req.Password);
            user.OTPCode = new Random().Next(100000, 999999).ToString();
            user.RoleID = 3;
            user.TypeLogin = "Normal";

            user = _unitOfWorks.UserRepository.Update(user);
            int rs = await _unitOfWorks.UserRepository.Commit();
            if (rs > 0)
            {
                return _mapper.Map<UserModel>(user);
            }
            else
            {
                return null;
            }
        }
        userEntity.Status = "InActive";
        userEntity.CreateDate = DateTimeOffset.Now.AddMinutes(2);
        userEntity.Password = SecurityUtil.Hash(req.Password!);
        userEntity.BirthDate = DateTime.Now;
        userEntity.TypeLogin = "Normal";
        userEntity.RoleID = 3;
        var existedUser = _unitOfWorks.UserRepository.FindByCondition(x => x.Email == req.Email || x.PhoneNumber == req.PhoneNumber).FirstOrDefault();
        if (existedUser != null)
        {
            throw new BadRequestException("email or phone already exist");
        }
        userEntity = await _unitOfWorks.UserRepository.AddAsync(userEntity);
        int result = await _unitOfWorks.UserRepository.Commit();
        if (result > 0)
        {
            // get latest userID
            //newUser.UserId = _userRepository.GetAll().OrderByDescending(x => x.);
            req.UserId = userEntity.UserId;
            return _mapper.Map<UserModel>(userEntity);
        }
        else
        {
            return null;
        }
    }

    public async Task<LoginResult> SignIn(string identifier, string password)
    {
        var user = _unitOfWorks.AuthRepository
            .FindByCondition(u => u.Email.ToLower() == identifier.ToLower() || u.PhoneNumber.ToLower() == identifier.ToLower())
            .FirstOrDefault();


        if (user is null || user.Status == "InActive")
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
            };
        }
        var userRole = _unitOfWorks.UserRoleRepository.FindByCondition(ur => ur.RoleId == user.RoleID).FirstOrDefault();

        user.UserRole = userRole!;

        var hash = SecurityUtil.Hash(password);
        if (!user.Password!.Equals(hash))
        {
            return new LoginResult
            {
                Authenticated = false,
                Token = null,
            };
        }
        var handler = new JwtSecurityTokenHandler();
        var refreshToken = CreateJwtToken(user, false);
        user.RefreshToken = handler.WriteToken(refreshToken);

        _unitOfWorks.UserRepository.Update(user);
        var result = await _unitOfWorks.UserRepository.Commit();
        if (result > 0)
        {
            return new LoginResult
            {
                Authenticated = true,
                Token = CreateJwtToken(user, true),
                Refresh = refreshToken
            };
        }

        return null;
    }

    public async Task<LoginResult> SignInWithGG(LoginWithGGRequest req)
    {
        var handler = new JwtSecurityTokenHandler();
        SecurityToken refreshToken = null;
        var user = _unitOfWorks.AuthRepository.FindByCondition(u => u.Email.ToLower() == req.Email.ToLower()).FirstOrDefault();

        if (user != null)
        {
            refreshToken = CreateJwtToken(user, false);
            user.RefreshToken = handler.WriteToken(refreshToken);
            // Generate JWT or another token here if needed.
            _unitOfWorks.UserRepository.Update(user);
            var resultExist = await _unitOfWorks.UserRepository.Commit();
            if (resultExist > 0)
            {
                return new LoginResult
                {
                    Authenticated = true,
                    Token = CreateJwtToken(user, true),
                    Refresh = refreshToken
                };
            }
        }

        var userModel = new UserModel
        {
            Email = req.Email,
            FullName = req.FullName,
            UserName = req.UserName,
            PhoneNumber = req.Phone,
            Password = SecurityUtil.Hash("123456"),
            Avatar = req.Avatar,
            OTPCode = "0",
            TypeLogin = "Google",
            Status = "Active",
            CreateDate = DateTimeOffset.Now,
            BirthDate = DateTime.Now,
            RoleID = req.TypeAccount == "Admin" ? 1 : req.TypeAccount == "Manager" ? 2 : req.TypeAccount == "Customer" ? 3 : req.TypeAccount == "Staff" ? 4 : 3
        };

        var userRegister = _mapper.Map<User>(userModel);
        refreshToken = CreateJwtToken(userRegister, false);
        userRegister.RefreshToken = handler.WriteToken(refreshToken);

        await _unitOfWorks.UserRepository.AddAsync(userRegister);
        int result = await _unitOfWorks.UserRepository.Commit();
        await _mongoDbService.CreateCustomerAsync(userRegister.UserId);
        if (result > 0)
        {
            // Generate JWT or another token here if needed.
            return new LoginResult
            {
                Authenticated = true,
                Token = CreateJwtToken(userRegister, true)
            };
        }

        return new LoginResult
        {
            Authenticated = false,
            Token = null
        };
    }


    public async Task<LoginResult> SignInWithFacebook(LoginWithGGRequest req)
    {
        var handler = new JwtSecurityTokenHandler();
        SecurityToken refreshToken = null;
        var user = _unitOfWorks.AuthRepository.FindByCondition(u => u.Email.ToLower() == req.Email.ToLower()).FirstOrDefault();

        if (user != null)
        {
            refreshToken = CreateJwtToken(user, false);
            user.RefreshToken = handler.WriteToken(refreshToken);
            // Generate JWT or another token here if needed.
            _unitOfWorks.UserRepository.Update(user);
            var resultExist = await _unitOfWorks.UserRepository.Commit();
            if (resultExist > 0)
            {
                return new LoginResult
                {
                    Authenticated = true,
                    Token = CreateJwtToken(user, true),
                    Refresh = refreshToken
                };
            }
        }

        var userModel = new UserModel
        {
            Email = req.Email,
            FullName = req.FullName,
            UserName = req.UserName,
            Password = SecurityUtil.Hash("123456"),
            Avatar = req.Avatar,
            Status = "Active",
            OTPCode = "0",
            TypeLogin = "Facebook",
            CreateDate = DateTimeOffset.Now,
            BirthDate = DateTime.Now,
            RoleID = req.TypeAccount == "Admin" ? 1 : req.TypeAccount == "Manager" ? 2 : req.TypeAccount == "Customer" ? 3 : req.TypeAccount == "Staff" ? 4 : 3
        };

        var userRegister = _mapper.Map<User>(userModel);
        refreshToken = CreateJwtToken(userRegister, false);
        userRegister.RefreshToken = handler.WriteToken(refreshToken);

        await _unitOfWorks.UserRepository.AddAsync(userRegister);
        int result = await _unitOfWorks.UserRepository.Commit();
        await _mongoDbService.CreateCustomerAsync(userRegister.UserId);

        if (result > 0)
        {
            // Generate JWT or another token here if needed.
            return new LoginResult
            {
                Authenticated = true,
                Token = CreateJwtToken(userRegister, true)
            };
        }

        return new LoginResult
        {
            Authenticated = false,
            Token = null
        };
    }



    public async Task<UserModel> GetUserByEmail(string email)
    {
        return _mapper.Map<UserModel>(_unitOfWorks.UserRepository.FindByCondition(x => x.Email.ToLower() == email.ToLower()).FirstOrDefault());
    }

    public async Task<UserModel> GetUserInToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new BadRequestException("Authorization header is missing or invalid.");
        }
        // Decode the JWT token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        var user = await _unitOfWorks.UserRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();
        if (user is null)
        {
            throw new BadRequestException("Can not found User");
        }
        return _mapper.Map<UserModel>(user);
    }

    public async Task<UserModel> ForgotPass(UserModel req)
    {
        var userEntity = _mapper.Map<User>(req);
        var user = _unitOfWorks.UserRepository.FindByCondition(x => x.Email == req.Email).FirstOrDefault();

        if (user != null && user.Status == "Active")
        {
            user.CreateDate = DateTimeOffset.Now.AddMinutes(2);
            user.OTPCode = new Random().Next(100000, 999999).ToString();

            user = _unitOfWorks.UserRepository.Update(user);
            int rs = await _unitOfWorks.UserRepository.Commit();
            if (rs > 0)
            {
                return _mapper.Map<UserModel>(user);
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public async Task<UserModel> ResendOtp(string email)
    {
        var user = _unitOfWorks.UserRepository.FindByCondition(x => x.Email == email).FirstOrDefault();

        var twoMinuteAgo = user.CreateDate.Value.AddMinutes(-2);

        if (DateTimeOffset.Now < twoMinuteAgo && DateTimeOffset.Now > user.CreateDate)
        {
            throw new BadRequestException("OTP code is expired");
        }

        if (user != null)
        {
            user.CreateDate = DateTimeOffset.Now.AddMinutes(2);
            user.OTPCode = new Random().Next(100000, 999999).ToString();

            user = _unitOfWorks.UserRepository.Update(user);
            int rs = await _unitOfWorks.UserRepository.Commit();
            if (rs > 0)
            {
                return _mapper.Map<UserModel>(user);
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }


    public async Task<bool> UpdatePass(string email, string password)
    {
        var user = _unitOfWorks.UserRepository.FindByCondition(x => x.Email == email).FirstOrDefault();

        if (user != null && user.Status == "Active" && user.OTPCode == "0")
        {
            user.ModifyDate = DateTimeOffset.Now;
            user.Password = SecurityUtil.Hash(password);

            user = _unitOfWorks.UserRepository.Update(user);
            int rs = await _unitOfWorks.UserRepository.Commit();
            if (rs > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }



    public async Task<bool> SubmitOTP(SubmitOTPResquest req)
    {
        var existedUser = _unitOfWorks.UserRepository.FindByCondition(x => x.Email == req.Email).FirstOrDefault();

        if (existedUser is null)
        {
            throw new BadRequestException("Account does not exist");
        }

        if (req.OTP != existedUser.OTPCode)
        {
            throw new BadRequestException("OTP Code is not Correct");
        }

        var result = 0;

        var twoMinuteAgo = existedUser.CreateDate.Value.AddMinutes(-2);

        if (DateTimeOffset.Now > twoMinuteAgo && DateTimeOffset.Now < existedUser.CreateDate)
        {
            existedUser.Status = "Active";
            existedUser.OTPCode = "0";
            _unitOfWorks.UserRepository.Update(existedUser);
            result = await _unitOfWorks.UserRepository.Commit();

            await _mongoDbService.CreateCustomerAsync(existedUser.UserId);
        }
        else
        {
            throw new BadRequestException("OTP code is expired");
        }

        if (result > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> ChangeStatus(int userId, string status)
    {

        var userChange = await _unitOfWorks.UserRepository.GetByIdAsync(userId);
        if (userChange != null)
        {
            if (status == "Active" || status == "InActive")
            {
                userChange.Status = status;
                _unitOfWorks.UserRepository.Update(userChange);
            }
            else
            {
                throw new BadRequestException("Invalid provided status");
            }
            var rs = await _unitOfWorks.UserRepository.Commit();
            return rs > 0;
        }
        else
        {
            throw new BadRequestException("Can Not Find The User To Change");
        }

    }


    private SecurityToken CreateJwtToken(User user, bool isAccess)
    {
        var utcNow = DateTime.UtcNow;
        var userRole = _unitOfWorks.UserRoleRepository.FindByCondition(u => u.RoleId == user.RoleID).FirstOrDefault();
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.UserId.ToString()),
/*            new(JwtRegisteredClaimNames.Sub, user.UserName),*/
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, userRole.RoleName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("PhoneNumber", user?.PhoneNumber),
        };
        byte[] key;

        if (isAccess)
        {
            key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
        }
        else
        {
            key = Encoding.ASCII.GetBytes(_jwtSettings.Refresh);
        }

        SecurityTokenDescriptor tokenDescriptor;
        if (isAccess)
        {
            tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(authClaims),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Expires = utcNow.Add(TimeSpan.FromHours(1)),
            };
        }
        else
        {
            tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(authClaims),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Expires = utcNow.Add(TimeSpan.FromDays(30)),
            };
        }

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    public async Task<LoginResult> RefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new BadRequestException("Refresh token is missing or invalid.");
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(refreshToken);

            // Check if the refresh token is expired
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                throw new BadRequestException("Refresh token has expired.");
            }

            // Extract the email from the token claims
            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new BadRequestException("Email claim is missing in the token.");
            }

            // Find the user by email
            var user = await _unitOfWorks.UserRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();
            if (user is null)
            {
                throw new BadRequestException("User not found.");
            }

            // Create a new access token
            var newAccessToken = CreateJwtToken(user, true);

            return new LoginResult
            {
                Authenticated = true,
                Token = newAccessToken
            };
        }
        catch (Exception ex)
        {
            // Handle any exceptions such as invalid token format, etc.
            throw new BadRequestException($"Failed to refresh token: {ex.Message}");
        }
    }

    public async Task<UserInfoModel> CreateAccountWithPhone(string phoneNumber, string userName, string? passWord, string? email)
    {
        var existedUser = await _unitOfWorks.UserRepository
            .FindByCondition(x => x.PhoneNumber == phoneNumber || x.UserName == userName).FirstOrDefaultAsync();
        if (existedUser != null)
        {
            throw new BadRequestException("Phone number or username already exist");
        }
        var random = new Random();
        var randomPassword = random.Next(100000, 999999).ToString();
        var user = new User
        {
            PhoneNumber = phoneNumber,
            UserName = userName,
            Email = email ?? userName.ToLower() + "@gmail.com",
            Password = SecurityUtil.Hash(passWord ?? randomPassword),
            Status = "Active",
            CreateDate = DateTimeOffset.Now,
            BirthDate = DateTime.Now,
            RoleID = 3,
            TypeLogin = "Normal"
        };
        var userEntity = await _unitOfWorks.UserRepository.AddAsync(user);
        var result = await _unitOfWorks.UserRepository.Commit();
        await _mongoDbService.CreateCustomerAsync(userEntity.UserId);
        return result > 0 ? _mapper.Map<UserInfoModel>(userEntity) : null;
    }

    public async Task<UserInfoModel> CheckExistAccount(string? phoneNumber, string? email)
    {
        var existedUser = await _unitOfWorks.UserRepository
            .FindByCondition(x => x.PhoneNumber == phoneNumber || x.Email == email).FirstOrDefaultAsync();
        return existedUser != null ? _mapper.Map<UserInfoModel>(existedUser) : null;
    }

    public async Task<UserInfoModel> UpdateUserInfoModel(UserInfoUpdateRequest userInfoModel)
    {
        var user = await _unitOfWorks.UserRepository.GetByIdAsync(userInfoModel.UserId);
        if (user == null)
        {
            throw new BadRequestException("User not found");
        }

        // Cập nhật thông tin người dùng
        user.UserName = userInfoModel.UserName ?? user.UserName;
        user.FullName = userInfoModel.FullName ?? user.FullName;
        user.Email = userInfoModel.Email ?? user.Email;
        if (userInfoModel.Avatar != null)
        {
            var avatar = await _cloudianryService.UploadImageAsync(userInfoModel.Avatar);
            user.Avatar = avatar.SecureUrl.ToString();
        }
        user.Gender = userInfoModel.Gender ?? user.Gender;
        user.City = userInfoModel.City ?? user.City;
        user.Address = userInfoModel.Address ?? user.Address;
        user.BirthDate = userInfoModel.BirthDate ?? user.BirthDate;
        user.PhoneNumber = userInfoModel.PhoneNumber ?? user.PhoneNumber;
        user.STK = userInfoModel.STK ?? user.STK;
        user.Bank = userInfoModel.Bank ?? user.Bank;
        user.QRCode = userInfoModel.QRCode ?? user.QRCode;
        user.District = userInfoModel.District ?? user.District;
        user.WardCode = userInfoModel.WardCode ?? user.WardCode;

        user.UpdatedDate = DateTime.Now;
        user.ModifyDate = DateTimeOffset.Now;
        user.ModifyBy = user.FullName;

        // Lưu thay đổi vào DB
        var result = _unitOfWorks.UserRepository.Update(user);
        await _unitOfWorks.UserRepository.Commit();

        return _mapper.Map<UserInfoModel>(result);
    }

    public async Task<List<BranchRevenueDto>> GetRevenueByBranchAsync(int month, int year)
    {
        // Bước 1: Lấy các đơn hàng Completed trong tháng/năm
        var orders = await _unitOfWorks.OrderRepository
            .FindByCondition(o =>
                o.Status == "Completed" &&
                o.CreatedDate.Month == month &&
                o.CreatedDate.Year == year)
            .Include(o => o.Appointments) // Lấy luôn các appointments trong đơn
            .ToListAsync();

        // Bước 2: Mỗi branchId có trong appointments của đơn hàng -> cộng nguyên TotalAmount
        var revenueList = orders
            .SelectMany(order =>
            {
                var branchIds = order.Appointments
                    .Select(a => a.BranchId)
                    .Distinct();

                return branchIds.Select(branchId => new
                {
                    BranchId = branchId,
                    Revenue = order.TotalAmount
                });
            })
            .GroupBy(x => x.BranchId)
            .Select(g => new BranchRevenueDto
            {
                BranchId = g.Key,
                BranchName = "", // sẽ gán sau
                TotalRevenue = g.Sum(x => x.Revenue)
            }).ToList();

        // Bước 3: Gán tên chi nhánh
        var branches = await _unitOfWorks.BranchRepository.GetAll().ToListAsync();
        foreach (var item in revenueList)
        {
            var branch = branches.FirstOrDefault(b => b.BranchId == item.BranchId);
            item.BranchName = branch?.BranchName ?? "Unknown";
        }

        return revenueList;
    }

    public async Task<List<BranchRevenueDto>> GetTop3RevenueBranchesAsync(int month, int year)
    {
        // Bước 1: Lấy danh sách Order đã Completed trong tháng/năm
        var orders = await _unitOfWorks.OrderRepository
            .FindByCondition(o =>
                o.Status == "Completed" &&
                o.CreatedDate.Month == month &&
                o.CreatedDate.Year == year)
            .Include(o => o.Appointments)
            .ToListAsync();

        // Bước 2: Với mỗi đơn hàng, mỗi chi nhánh trong Appointments sẽ được cộng full TotalAmount
        var branchRevenue = orders
            .SelectMany(order =>
            {
                var branchIds = order.Appointments
                    .Select(a => a.BranchId)
                    .Distinct();

                return branchIds.Select(branchId => new
                {
                    BranchId = branchId,
                    Revenue = order.TotalAmount
                });
            })
            .GroupBy(x => x.BranchId)
            .Select(g => new BranchRevenueDto
            {
                BranchId = g.Key,
                BranchName = "", // gán ở bước 3
                TotalRevenue = g.Sum(x => x.Revenue)
            })
            .OrderByDescending(b => b.TotalRevenue) // Sắp xếp giảm dần
            .Take(3) // Lấy top 3
            .ToList();

        // Bước 3: Gán tên chi nhánh
        var branches = await _unitOfWorks.BranchRepository.GetAll().ToListAsync();
        foreach (var item in branchRevenue)
        {
            var branch = branches.FirstOrDefault(b => b.BranchId == item.BranchId);
            item.BranchName = branch?.BranchName ?? "Unknown";
        }

        return branchRevenue;
    }

    public async Task<UserModel> CreateUserAsync(CreateAdminManagerRequestDto req)
    {
        if (req.RoleID != 1 && req.RoleID != 2)
        {
            throw new BadRequestException("Chỉ tạo mới cho Admin và Manager");
        }

      
        var existingUser = _unitOfWorks.UserRepository.FindByCondition(x => x.Email == req.Email).FirstOrDefault();
        if (existingUser != null)
        {
            throw new BadRequestException("Email đã tồn tại");
        }

        var emailValidationResult = new EmailAddressAttribute().IsValid(req.Email);
        if (!emailValidationResult)
        {
            throw new BadRequestException("Email không hợp lệ.");
        }


        var userEntity = _mapper.Map<User>(req);

        
        userEntity.Password = SecurityUtil.Hash(req.Password);

       
        userEntity.Status = "Active";
        userEntity.CreateDate = DateTimeOffset.Now; 
        userEntity.UpdatedDate = DateTime.Now; 
        userEntity.OTPCode = "0";
        userEntity.TypeLogin = "Normal";

      
        userEntity = await _unitOfWorks.UserRepository.AddAsync(userEntity);
        int result = await _unitOfWorks.UserRepository.Commit();

       
        if (result > 0)
        {
            return _mapper.Map<UserModel>(userEntity);
        }
        else
        {
            throw new BadRequestException("Tạo mới thất bại.");
        }
    }

}
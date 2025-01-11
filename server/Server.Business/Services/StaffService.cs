using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Constants;
using Server.Business.Dtos;
using Server.Business.Ultils;
using Server.Data.Entities;
using Server.Data.Helpers;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;

namespace Server.Business.Services
{
    public class StaffService
    {

        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly MailService _mailService;

        public StaffService(UnitOfWorks unitOfWorks, IMapper mapper, MailService mailService)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _mailService = mailService;
        }

        public async Task<Pagination<Staff>> GetListAsync(
    Expression<Func<Staff, bool>> filter = null,
    Func<IQueryable<Staff>, IOrderedQueryable<Staff>> orderBy = null,
    int? pageIndex = null,
    int? pageSize = null,
    string name = null)
        {
            IQueryable<Staff> query = _unitOfWorks.StaffRepository.GetAll()
                .Include(s => s.StaffInfo)
                .Include(s => s.Branch);

            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim().ToLower();
                query = query.Where(s => s.StaffInfo.FullName.ToLower().Contains(name));
            }

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            var totalItemsCount = await query.CountAsync();

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query = query.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value);
            }

            var items = await query.ToListAsync();

            return new Pagination<Staff>
            {
                TotalItemsCount = totalItemsCount,
                PageSize = pageSize ?? totalItemsCount,
                PageIndex = pageIndex ?? 0,
                Data = items
            };
        }





        public async Task<ApiResponse> CreateStaffAsync(CUStaffDto staffDto)
        {
            try
            {
                if (staffDto == null)
                    return ApiResponse.Error("Staff data is required.");

                // Kiểm tra Branch tồn tại
                var branchExists = await _unitOfWorks.BranchRepository
     .FindByCondition(x => x.BranchId == staffDto.BranchId)
     .AnyAsync();

                if (!branchExists)
                    return ApiResponse.Error("Branch not found.");

                // Tạo User mới
                var newUser = new User
                {
                    UserName = staffDto.UserName,
                    Email = staffDto.Email,
                    FullName = staffDto.FullName,
                    Password = SecurityUtil.Hash("123456"), // Bạn có thể thêm logic băm mật khẩu ở đây
                    RoleID = (int)RoleConstant.RoleType.Staff, // Vai trò Staff
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    TypeLogin = "Normal" // Thêm giá trị mặc định cho TypeLogin
                };

                await _unitOfWorks.UserRepository.AddAsync(newUser);
                await _unitOfWorks.UserRepository.Commit();

                // Tạo Staff mới liên kết với User
                var newStaff = new Staff
                {
                    UserId = newUser.UserId,
                    BranchId = staffDto.BranchId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _unitOfWorks.StaffRepository.AddAsync(newStaff);
                await _unitOfWorks.StaffRepository.Commit();

                // Gửi email thông báo tài khoản và mật khẩu
                var mailData = new MailData
                {
                    EmailToId = newUser.Email,
                    EmailToName = newUser.FullName,
                    EmailSubject = "Welcome to Our Team!",
                    EmailBody = $@"
<div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
    <h2 style=""text-align: center; color: #3498db; font-weight: bold;"">Welcome to Our Team!</h2>
    <p style=""font-size: 16px; color: #555;"">Dear {newUser.FullName},</p>
    <p style=""font-size: 16px; color: #555;"">You have been added as a staff member in our system.</p>
    <p style=""font-size: 16px; color: #555;"">Here are your login credentials:</p>
    <ul style=""font-size: 16px; color: #555; list-style-type: none; padding: 0;"">
        <li><strong>Username:</strong> {newUser.UserName}</li>
        <li><strong>Password:</strong> 123456</li>
    </ul>
    <p style=""font-size: 16px; color: #555;"">Please log in and update your password as soon as possible.</p>
    <p style=""text-align: center; color: #888; font-size: 14px;"">Powered by Team Solace</p>
</div>"
                };

                _ = Task.Run(async () =>
                {
                    var emailResult = await _mailService.SendEmailAsync(mailData, false);
                    if (!emailResult)
                    {
                        Console.WriteLine("Failed to send email.");
                    }
                });

                // Lấy dữ liệu Staff đã tạo
                var createdStaff = await _unitOfWorks.StaffRepository
                    .GetAll()
                    .Include(s => s.Branch)
                    .Include(s => s.StaffInfo)
                    .FirstOrDefaultAsync(s => s.StaffId == newStaff.StaffId);

                return ApiResponse.Succeed(createdStaff, "Staff created successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                return ApiResponse.Error($"Database Update Error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error($"Error: {ex.Message}");
            }
        }






        public async Task<List<Staff>> GetStaffByCustomerIdAsync(int customerId)
        {
            // Lấy danh sách Appointments có liên quan đến CustomerId thông qua UnitOfWorks
            var appointments = await _unitOfWorks.AppointmentsRepository
                .GetAll()
                .Include(x => x.Staff).ThenInclude(x => x.StaffInfo)
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();

            // Nhóm theo StaffId, loại bỏ trùng lặp, và tạo danh sách Staff
            var staffs = appointments
                .GroupBy(x => x.StaffId)
                .Select(g => g.First())
                .Select(x => new Staff
                {
                    StaffId = x.StaffId,
                    UserId = x.Staff.UserId,
                    BranchId = x.Staff.BranchId,
                    CreatedDate = x.Staff.CreatedDate,
                    UpdatedDate = x.Staff.UpdatedDate,
                    StaffInfo = x.Staff.StaffInfo
                })
                .ToList();

            return staffs;
        }


        public async Task<Staff> GetStaffLastByCustomerIdAsync(int customerId)
        {
            // Lấy tất cả các cuộc hẹn liên quan đến khách hàng qua UnitOfWorks
            var appointments = await _unitOfWorks.AppointmentsRepository
                .GetAll()
                .Include(x => x.Staff).ThenInclude(x => x.StaffInfo)
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.AppointmentsTime)
                .ToListAsync();

            // Nhóm các cuộc hẹn theo StaffId và lấy cuộc hẹn cuối cùng của mỗi nhân viên
            var staff = appointments
                .GroupBy(x => x.StaffId)
                .Select(g => g.First())
                .Select(x => new Staff
                {
                    StaffId = x.StaffId,
                    UserId = x.Staff.UserId,
                    BranchId = x.Staff.BranchId,
                    CreatedDate = x.Staff.CreatedDate,
                    UpdatedDate = x.Staff.UpdatedDate,
                    StaffInfo = x.Staff.StaffInfo
                })
                .FirstOrDefault();

            return staff;
        }


        public async Task<ApiResponse> AssignRoleAsync(int staffId, int roleId)
        {
            try
            {
                // Kiểm tra sự tồn tại của Staff
                var staffExists = await _unitOfWorks.StaffRepository
    .FindByCondition(x => x.StaffId == staffId)
    .AnyAsync();

                if (!staffExists)
                {
                    return ApiResponse.Error("Staff not found");
                }

                // Kiểm tra sự tồn tại của Role
                var roleExists = await _unitOfWorks.UserRepository
     .FindByCondition(x => x.UserRole.RoleId == roleId)
     .AnyAsync();

                if (!roleExists)
                {
                    return ApiResponse.Error("Role not found");
                }

                // Lấy Staff
                var staff = await _unitOfWorks.StaffRepository
     .FindByCondition(x => x.StaffId == staffId)
     .FirstOrDefaultAsync();

                if (staff == null)
                {
                    return ApiResponse.Error("Staff not found");
                }

                // Lấy User liên kết với Staff
                var user = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == staff.UserId);
                if (user == null)
                {
                    return ApiResponse.Error("User not found");
                }

                // Cập nhật Role cho User
                user.RoleID = roleId;
                user.UpdatedDate = DateTime.UtcNow;

                // Cập nhật vào database qua UnitOfWorks
                _unitOfWorks.UserRepository.Update(user);
                await _unitOfWorks.UserRepository.Commit();

                // Trả về kết quả thành công
                return ApiResponse.Succeed(new
                {
                    staff.StaffId,
                    user.UserId,
                    user.RoleID,
                    UpdatedDate = user.UpdatedDate
                }, "Role assigned to staff successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error($"An error occurred: {ex.Message}");
            }
        }



        public async Task<ApiResponse> AssignBranchAsync(int staffId, int branchId)
        {
            try
            {
                // Kiểm tra sự tồn tại của Staff
                var staffExists = await _unitOfWorks.StaffRepository.FindByCondition(x => x.StaffId == staffId).AnyAsync();
                if (!staffExists)
                {
                    return ApiResponse.Error("Staff not found");
                }

                // Kiểm tra sự tồn tại của Branch
                var branchExists = await _unitOfWorks.BranchRepository
     .FindByCondition(x => x.BranchId == branchId)
     .AnyAsync();

                if (!branchExists)
                {
                    return ApiResponse.Error("Branch not found");
                }

                // Lấy thông tin Staff cần cập nhật
                var staff = await _unitOfWorks.StaffRepository.FindByCondition(x => x.StaffId == staffId).FirstOrDefaultAsync();
                if (staff == null)
                {
                    return ApiResponse.Error("Staff not found");
                }

                // Cập nhật BranchId và thời gian cập nhật
                staff.BranchId = branchId;
                staff.UpdatedDate = DateTime.UtcNow;

                // Cập nhật Staff vào database qua UnitOfWorks
                _unitOfWorks.StaffRepository.Update(staff);
                await _unitOfWorks.StaffRepository.Commit();

                // Trả về kết quả
                return ApiResponse.Succeed(staff, "Branch assigned to staff successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error($"An error occurred: {ex.Message}");
            }
        }      



        public async Task<ApiResponse> GetStaffByIdAsync(int staffId)
        {
            try
            {
                // Sử dụng UnitOfWorks để lấy thông tin Staff
                var staff = await _unitOfWorks.StaffRepository
                    .GetAll() // Lấy tất cả dữ liệu từ repository
                    .Include(s => s.StaffInfo)  // Bao gồm thông tin người dùng (User)
                    .Include(s => s.Branch) // Bao gồm thông tin chi nhánh (Branch)
                    .FirstOrDefaultAsync(s => s.StaffId == staffId);

                // Kiểm tra nếu không tìm thấy staff
                if (staff == null)
                {
                    return ApiResponse.Error("Staff not found.");
                }

                // Trả về dữ liệu staff
                return ApiResponse.Succeed(staff, "Staff retrieved successfully.");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về phản hồi lỗi
                return ApiResponse.Error($"An error occurred: {ex.Message}");
            }
        }



        public async Task<ApiResponse> UpdateStaffAsync(int staffId, UpdateStaffDto staffUpdateDto)
        {
            try
            {
                // Tìm kiếm Staff dựa trên staffId từ route
                var existingStaff = await _unitOfWorks.StaffRepository
     .FindByCondition(s => s.StaffId == staffId)
     .Include(s => s.StaffInfo) // Bao gồm thông tin từ bảng User
     .Include(s => s.Branch)    // Bao gồm thông tin từ bảng Branch
     .FirstOrDefaultAsync();


                if (existingStaff == null)
                {
                    return new ApiResponse
                    {
                        message = "Staff not found."
                    };
                }

                // Cập nhật thông tin Staff
                if (existingStaff.StaffInfo != null)
                {
                    existingStaff.StaffInfo.UserName = staffUpdateDto.UserName ?? existingStaff.StaffInfo.UserName;
                    existingStaff.StaffInfo.FullName = staffUpdateDto.FullName ?? existingStaff.StaffInfo.FullName;
                    existingStaff.StaffInfo.Email = staffUpdateDto.Email ?? existingStaff.StaffInfo.Email;
                    existingStaff.StaffInfo.Avatar = staffUpdateDto.Avatar ?? existingStaff.StaffInfo.Avatar;
                }

                existingStaff.BranchId = staffUpdateDto.BranchId;
                existingStaff.UpdatedDate = DateTime.UtcNow;

                // Lưu thay đổi
                _unitOfWorks.StaffRepository.Update(existingStaff);
                await _unitOfWorks.StaffRepository.Commit();

                // Trả về thông tin staff sau khi cập nhật
                return new ApiResponse
                {
                    message = "Staff updated successfully.",
                    data = existingStaff
                };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có ngoại lệ
                return new ApiResponse
                {
                    message = $"Error: {ex.Message}"
                };
            }
        }






        public async Task<ApiResponse> DeleteStaffAsync(int staffId)
        {
            try
            {
                // Tìm Staff trong database
                var staff = await _unitOfWorks.StaffRepository
      .FindByCondition(p => p.StaffId == staffId)
      .FirstOrDefaultAsync();


                // Nếu không tìm thấy Staff
                if (staff == null)
                {
                    return new ApiResponse
                    {
                        message = "Staff not found.",
                        data = null
                    };
                }

                // Tiến hành xóa Staff
                _unitOfWorks.StaffRepository.Remove(staff.StaffId);
                await _unitOfWorks.StaffRepository.Commit();


                // Trả về phản hồi thành công
                return new ApiResponse
                {
                    message = "Staff deleted successfully.",
                    data = staff // Trả về dữ liệu Staff vừa bị xóa nếu cần
                };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về phản hồi lỗi
                return new ApiResponse
                {
                    message = $"An error occurred: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<List<StaffDTO>> GetStaffByBranchAsync(int branchId)
        {
            var staffList = await _unitOfWorks.StaffRepository
                .FindByCondition(s => s.BranchId == branchId)
                .Include(s => s.StaffInfo) // Bao gồm thông tin người dùng
                .ToListAsync();

            var staffDtoList = staffList.Select(s => new StaffDTO
            {
                StaffId = s.StaffId,
                //UserId = s.UserId,
                BranchId = s.BranchId,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate,
                StaffInfo = new UserDTO
                {
                    UserId = s.StaffInfo?.UserId ?? 0,
                    UserName = s.StaffInfo?.UserName ?? "N/A",
                    Email = s.StaffInfo?.Email ?? "N/A"
                }
            }).ToList();

            return staffDtoList;
        }
    
}
}

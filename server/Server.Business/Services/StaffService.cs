using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nest;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Constants;
using Server.Business.Dtos;
using Server.Business.Ultils;
using Server.Data.Entities;
using Server.Data.Helpers;
using Server.Data.UnitOfWorks;
using System.Linq.Expressions;
using Server.Business.Exceptions;
using Server.Business.Models;
using System.Globalization;
using Server.Business.Commons.Request;
using Server.Data;

namespace Server.Business.Services
{
    public class StaffService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly MailService _mailService;
        private readonly ServiceService _serviceService;
        private readonly MongoDbService _mongoDbService;

        public StaffService(UnitOfWorks unitOfWorks, IMapper mapper, MailService mailService, ServiceService serviceService, MongoDbService mongoDbService)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _mailService = mailService;
            _serviceService = serviceService;
            _mongoDbService = mongoDbService;
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

        public async Task<StaffModel> GetStaffByUserId(int userId)
        {
            var staff = await _unitOfWorks.StaffRepository.FindByCondition(x => x.UserId == userId)
                .FirstOrDefaultAsync();
            if (staff == null) return null;
            return _mapper.Map<StaffModel>(staff);
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

                var userExist = await _unitOfWorks.UserRepository
                    .FirstOrDefaultAsync(x => x.Email.ToLower() == staffDto.Email.ToLower());

                if (userExist != null)
                {
                    throw new BadRequestException("Email already exists.");
                }

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

                var userEntity = await _unitOfWorks.UserRepository.AddAsync(newUser);
                await _unitOfWorks.UserRepository.Commit();
                await _mongoDbService.CreateCustomerAsync(userEntity.UserId);

                // Tạo Staff mới liên kết với User
                var newStaff = new Staff
                {
                    UserId = newUser.UserId,
                    BranchId = staffDto.BranchId,
                    RoleId = staffDto.RoleId,
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
                    .FindByCondition(s => s.StaffId == newStaff.StaffId)
                    .Include(s => s.Branch)
                    .Include(s => s.StaffInfo)
                    .FirstOrDefaultAsync();

                return ApiResponse.Succeed(new ApiResponse()
                {
                    message = "Staff created successfully.",
                    data = createdStaff
                });
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
                var staffExists = await _unitOfWorks.StaffRepository.FindByCondition(x => x.StaffId == staffId)
                    .AnyAsync();
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
                var staff = await _unitOfWorks.StaffRepository.FindByCondition(x => x.StaffId == staffId)
                    .FirstOrDefaultAsync();
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
                    .Include(s => s.StaffInfo) // Bao gồm thông tin người dùng (User)
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
                    .Include(s => s.Branch) // Bao gồm thông tin từ bảng Branch
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

        public async Task<List<StaffBranchServiceDto>> GetStaffByBranchAndServiceAsync(int branchId, int serviceId)
        {
            try
            {
                // Truy vấn danh sách nhân viên thuộc chi nhánh và dịch vụ yêu cầu
                var staffList = await _unitOfWorks.StaffRepository
                    .GetAll()
                    .Where(s => s.BranchId == branchId) // Lọc theo chi nhánh
                    .Include(s => s.Branch) // Lấy thông tin chi nhánh
                    .Include(s => s.StaffInfo) // Lấy thông tin nhân viên
                    .Select(s => new StaffBranchServiceDto
                    {
                        Staff = new Staff
                        {
                            StaffId = s.StaffId,
                            UserId = s.UserId,
                            BranchId = s.BranchId,
                            CreatedDate = s.CreatedDate,
                            UpdatedDate = s.UpdatedDate,
                            StaffInfo = s.StaffInfo, // Đảm bảo thông tin nhân viên được bao gồm
                            Branch = s.Branch // Lấy thông tin chi nhánh trong DTO
                        },
                        // Lấy dịch vụ đầu tiên có serviceId phù hợp
                        Service = s.Branch.Branch_Services
                            .Where(bs => bs.ServiceId == serviceId)
                            .Select(bs => bs.Service)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return staffList;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                throw new Exception($"Lỗi khi lấy danh sách nhân viên: {ex.Message}", ex);
            }
        }


        public async Task<List<BusyTimeDto>> GetStaffBusyTimesAsync(int staffId, DateTime date)
        {
            // Lấy tất cả Appointments của nhân viên trong ngày
            var appointments = await _unitOfWorks.AppointmentsRepository
                .FindByCondition(a => a.StaffId == staffId &&
                                      a.AppointmentsTime.Date == date.Date &&
                                      a.Status == OrderStatusEnum.Pending.ToString()) // Chỉ lấy trạng thái đã xác nhận
                .Include(a => a.Service) // Include Service để lấy duration
                .ToListAsync();

            // Tính toán thời gian bận
            var busyTimes = appointments
                .Where(a => a.Service != null) // Bỏ qua các bản ghi không có Service
                .Select(a =>
                {
                    // Parse duration từ Service (VD: "60 minutes")
                    var durationParts = a.Service.Duration.Split(' ');
                    int durationMinutes = int.TryParse(durationParts[0], out var minutes) ? minutes : 0;

                    return new BusyTimeDto
                    {
                        StartTime = a.AppointmentsTime,
                        EndTime = a.AppointmentsTime.AddMinutes(durationMinutes)
                    };
                })
                .OrderBy(bt => bt.StartTime) // Sắp xếp theo thời gian bắt đầu
                .ToList();

            return busyTimes;
        }

        public async Task<List<StaffBusyTimeDto>> GetMultipleStaffBusyTimesAsync(List<int> staffIds, DateTime date)
        {
            var appointments = await _unitOfWorks.AppointmentsRepository
                .FindByCondition(a => staffIds.Contains(a.StaffId) &&
                                      a.AppointmentsTime.Date == date.Date &&
                                      a.Status == OrderStatusEnum.Pending.ToString())
                .Include(a => a.Service)
                .ToListAsync();

            var result = appointments
                .Where(a => a.Service != null)
                .GroupBy(a => a.StaffId)
                .Select(g => new StaffBusyTimeDto
                {
                    StaffId = g.Key,
                    BusyTimes = g.Select(a =>
                    {
                        var durationParts = a.Service.Duration.Split(' ');
                        int durationMinutes = int.TryParse(durationParts[0], out var minutes) ? minutes : 0;

                        return new BusyTimeDto
                        {
                            StartTime = a.AppointmentsTime,
                            EndTime = a.AppointmentsTime.AddMinutes(durationMinutes)
                        };
                    })
                    .OrderBy(bt => bt.StartTime)
                    .ToList()
                })
                .ToList();

            return result;
        }



        public async Task<StaffModel> GetStaffById(int staffId)
        {
            // Sử dụng UnitOfWorks để lấy thông tin Staff
            var staff = await _unitOfWorks.StaffRepository
                .GetAll() // Lấy tất cả dữ liệu từ repository
                .Include(s => s.StaffInfo) // Bao gồm thông tin người dùng (User)
                .Include(s => s.Branch) // Bao gồm thông tin chi nhánh (Branch)
                .FirstOrDefaultAsync(s => s.StaffId == staffId);

            // Kiểm tra nếu không tìm thấy staff
            if (staff == null)
            {
                throw new BadRequestException("Staff not found.");
            }

            // Trả về dữ liệu staff
            return _mapper.Map<StaffModel>(staff);
        }

        public async Task<List<SpecialistScheduleDto>> GetStafflistScheduleAsync(int staffId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Kiểm tra nhân viên có RoleId hợp lệ (1 hoặc 2)
            var staff = await _unitOfWorks.StaffRepository.GetByIdAsync(staffId);
            if (staff == null || (staff.RoleId != 1 && staff.RoleId != 2))
            {
                return new List<SpecialistScheduleDto>();
            }

            var schedules = await _unitOfWorks.WorkScheduleRepository.GetAllAsync(
                ws => ws.StaffId == staffId && ws.WorkDate >= startDate && ws.WorkDate <= endDate
            );

            if (!schedules.Any())
            {
                return new List<SpecialistScheduleDto>();
            }

            var result = new SpecialistScheduleDto
            {
                StaffId = staffId,
                Schedules = schedules.Select(s => new WorkScheduleDto
                {
                    ScheduleId = s.Id,
                    WorkDate = s.WorkDate,
                    DayOfWeek = s.DayOfWeek,
                    ShiftName = s.Shift.ShiftName,
                    StartTime = s.Shift.StartTime,
                    EndTime = s.Shift.EndTime,
                    Status = s.Status
                }).ToList()
            };

            return new List<SpecialistScheduleDto> { result };
        }

        public async Task<bool> CheckStaffExists(int staffId)
        {
            var staff = await _unitOfWorks.StaffRepository.GetByIdAsync(staffId);
            return staff != null;
        }

        public async Task<List<CashierScheduleDto>> GetCashierScheduleAsync(int staffId, int year, int? month,
            int? week)
        {
            var staff = await _unitOfWorks.StaffRepository.GetByIdAsync(staffId);
            if (staff == null || staff.RoleId != 1)
            {
                return new List<CashierScheduleDto>(); // Không phải cashier, trả về danh sách rỗng
            }

            DateTime startDate, endDate;

            if (month.HasValue)
            {
                startDate = new DateTime(year, month.Value, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else if (week.HasValue)
            {
                startDate = ISOWeek.ToDateTime(year, week.Value, DayOfWeek.Monday);
                endDate = startDate.AddDays(6);
            }
            else
            {
                return new List<CashierScheduleDto>(); // Nếu không có tháng hoặc tuần hợp lệ, trả về danh sách rỗng
            }

            var schedules = await _unitOfWorks.WorkScheduleRepository.GetAllAsync(
                ws => ws.StaffId == staffId && ws.WorkDate >= startDate && ws.WorkDate <= endDate
            );

            if (!schedules.Any())
            {
                return new List<CashierScheduleDto>();
            }

            var result = new CashierScheduleDto
            {
                StaffId = staffId,
                //FullName = staff., // Gán tên của cashier
                Schedules = schedules.Select(s => new WorkScheduleDto
                {
                    ScheduleId = s.Id,
                    WorkDate = s.WorkDate,
                    DayOfWeek = s.DayOfWeek,
                    ShiftName = s.Shift.ShiftName,
                    StartTime = s.Shift.StartTime,
                    EndTime = s.Shift.EndTime,
                    Status = s.Status
                }).ToList()
            };

            return new List<CashierScheduleDto> { result };
        }

        //public async Task<StaffScheduleDto> GetStaffScheduleByDayAsync(int staffId, DateTime workDate)
        //{
        //    var staff = await _unitOfWorks.StaffRepository.GetByIdAsync(staffId);
        //    if (staff == null)
        //    {
        //        return null; // Không tìm thấy nhân viên
        //    }

        //    var schedules = await _unitOfWorks.WorkScheduleRepository.GetAllAsync(
        //        ws => ws.StaffId == staffId && ws.WorkDate.Date == workDate.Date
        //    );

        //    var result = new StaffScheduleDto
        //    {
        //        StaffId = staffId,
        //        //FullName = staff.StaffInfo.FullName,
        //        Schedules = schedules.Select(s => new WorkScheduleDto
        //        {
        //            ScheduleId = s.Id,
        //            WorkDate = s.WorkDate,
        //            DayOfWeek = s.DayOfWeek,
        //            ShiftName = s.Shift.ShiftName,
        //            StartTime = s.Shift.StartTime,
        //            EndTime = s.Shift.EndTime,
        //            Status = s.Status
        //        }).ToList()
        //    };

        //    return result;
        //}

        public async Task<StaffScheduleDto> GetStaffScheduleByMonthAsync(int staffId, int year, int month)
        {
            var staff = await _unitOfWorks.StaffRepository.FindByCondition(s => s.StaffId == staffId)
                .Include(s => s.StaffInfo)
                .FirstOrDefaultAsync();

            if (staff == null)
                return null;

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var schedules = await _unitOfWorks.WorkScheduleRepository
                .FindByCondition(s => s.StaffId == staffId &&
                                      s.WorkDate.Year == year &&
                                      s.WorkDate.Month == month)
                .ToListAsync();


            var result = new StaffScheduleDto
            {
                StaffId = staffId,
                //FullName = staff.StaffInfo?.FullName,
                SlotWorkings = schedules.Select(s => new SlotWorkingDto
                {
                    ScheduleId = s.Id,
                    BranchId = s.Staff.BranchId,
                    ShiftName = s.Shift.ShiftName,
                    StartTime = s.Shift.StartTime,
                    EndTime = s.Shift.EndTime,
                    WorkDate = s.WorkDate,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate
                }).ToList()
            };

            return result;
        }

        public async Task<ListStaffFreeInTimeResponse> ListStaffFreeInTimeV4(ListStaffFreeInTimeRequest request)
        {
            // Kiểm tra xem tất cả dịch vụ có tồn tại trong branch không
            var branchServices = await _unitOfWorks.Branch_ServiceRepository
                .FindByCondition(x => x.BranchId == request.BranchId && request.ServiceIds.Contains(x.ServiceId))
                .Select(x => x.ServiceId)
                .ToListAsync();

            if (branchServices.Count != request.ServiceIds.Length)
                return new ListStaffFreeInTimeResponse
                {
                    Message = "One or more services do not exist in branch",
                    Data = new List<StaffFreeInTimeResponse>()
                };

            // Lấy danh sách tất cả nhân viên thuộc branch
            var listStaff = await _unitOfWorks.StaffRepository
                .FindByCondition(x => x.BranchId == request.BranchId)
                .Include(x => x.StaffInfo)
                .ToListAsync();

            if (!listStaff.Any())
                return new ListStaffFreeInTimeResponse
                {
                    Message = "No staff found in branch",
                    Data = new List<StaffFreeInTimeResponse>()
                };

            // Lấy thời lượng của từng service
            var serviceDurations = _unitOfWorks.ServiceRepository
                .FindByCondition(s => request.ServiceIds.Contains(s.ServiceId))
                .AsEnumerable()
                .Select(s => new
                {
                    s.ServiceId,
                    s.ServiceCategoryId, // Lấy ServiceCategoryId của service
                    Duration = int.TryParse(s.Duration, out int duration) ? duration : 0
                })
                .ToList();

            if (serviceDurations.Any(s => s.Duration <= 0))
                return new ListStaffFreeInTimeResponse
                {
                    Message = "Invalid service duration detected",
                    Data = new List<StaffFreeInTimeResponse>()
                };

            // Tạo dictionary map serviceId -> duration và serviceCategoryId
            var serviceDurationDict = serviceDurations.ToDictionary(s => s.ServiceId, s => s.Duration);
            var serviceCategoryDict = serviceDurations.ToDictionary(s => s.ServiceId, s => s.ServiceCategoryId);

            // Lấy danh sách nhân viên có thể làm dịch vụ dựa trên ServiceCategory
            var staffServiceCategories = await _unitOfWorks.Staff_ServiceCategoryRepository
                .FindByCondition(x => serviceCategoryDict.Values.Contains(x.ServiceCategoryId))
                .ToListAsync();

            // Dictionary map StaffId -> Danh sách ServiceCategoryId mà họ có thể làm
            var staffServiceCategoryMap = staffServiceCategories
                .GroupBy(x => x.StaffId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ServiceCategoryId).ToHashSet());

            // Danh sách kết quả
            var responseList = new List<StaffFreeInTimeResponse>();

            for (int i = 0; i < request.ServiceIds.Length; i++)
            {
                int serviceId = request.ServiceIds[i];
                DateTime startTime = request.StartTimes[i];
                int serviceDuration = serviceDurationDict[serviceId];
                int serviceCategoryId = serviceCategoryDict[serviceId];

                var expectedEndTime = startTime.AddMinutes(serviceDuration);

                var busyStaffIds = await _unitOfWorks.AppointmentsRepository
                    .FindByCondition(a =>
                        a.BranchId == request.BranchId &&
                        (a.AppointmentsTime <= startTime &&
                         a.AppointmentsTime.AddMinutes(serviceDuration) > startTime ||
                         a.AppointmentsTime < expectedEndTime &&
                         a.AppointmentsTime.AddMinutes(serviceDuration) >= expectedEndTime ||
                         a.AppointmentsTime >= startTime &&
                         a.AppointmentsTime.AddMinutes(serviceDuration) <= expectedEndTime)
                    )
                    .Select(a => a.StaffId)
                    .Distinct()
                    .ToListAsync();

                // Lọc nhân viên rảnh & có thể làm dịch vụ
                var availableStaff = listStaff
                    .Where(s => !busyStaffIds.Contains(s.StaffId) &&
                                staffServiceCategoryMap.ContainsKey(s.StaffId) &&
                                staffServiceCategoryMap[s.StaffId].Contains(serviceCategoryId))
                    .ToList();

                // Thêm vào danh sách response
                responseList.Add(new StaffFreeInTimeResponse
                {
                    ServiceId = serviceId,
                    StartTime = startTime,
                    Staffs = _mapper.Map<List<StaffModel>>(availableStaff)
                });
            }

            return new ListStaffFreeInTimeResponse
            {
                Message = "Success",
                Data = responseList
            };
        }

        public async Task<GetListStaffByServiceCategoryResponse> GetListStaffByServiceCategory(
            GetListStaffByServiceCategoryRequest request)
        {
            var branch = await _unitOfWorks.BranchRepository
                .FindByCondition(x => x.BranchId == request.BranchId)
                .FirstOrDefaultAsync();
            if (branch == null) throw new BadRequestException("Branch not found!");

            var listStaffResponse = new List<StaffServiceCategoryResponse>();

            foreach (var serviceCategoryId in request.ServiceCategoryIds)
            {
                var listStaffServiceCategories = await _unitOfWorks.Staff_ServiceCategoryRepository
                    .FindByCondition(x => x.ServiceCategoryId == serviceCategoryId)
                    .Include(x => x.StaffInfo)
                    .ThenInclude(x => x.StaffInfo)
                    .Where(x => x.StaffInfo.BranchId == request.BranchId)
                    .ToListAsync();

                var listStaff = new List<Staff>();
                foreach (var staffServiceCategory in listStaffServiceCategories)
                {
                    listStaff.Add(staffServiceCategory.StaffInfo);
                }

                listStaffResponse.Add(new StaffServiceCategoryResponse()
                {
                    ServiceCategoryId = serviceCategoryId,
                    Staffs = _mapper.Map<List<StaffModel>>(listStaff)
                });
            }

            if (listStaffResponse.Any())
            {
                return new GetListStaffByServiceCategoryResponse()
                {
                    Message = "Get list staff successfully!",
                    Data = listStaffResponse.ToArray()
                };
            }

            return null;
        }

        public async Task<StaffModel> GetStaffByCustomerId(int customerId)
        {
            var result = await _unitOfWorks.StaffRepository
                .FirstOrDefaultAsync(x => x.UserId == customerId);
            return _mapper.Map<StaffModel>(result);
        }

        public async Task<List<StaffAppointmentResponse>> GetStaffAppointmentsAsync(List<int> staffIds,
            DateTime startDate, DateTime endDate)
        {
            var appointments = await _unitOfWorks.AppointmentsRepository
                .FindByCondition(a => staffIds.Contains(a.StaffId) &&
                                      a.AppointmentsTime >= startDate &&
                                      a.AppointmentsTime <= endDate)
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .Include(a => a.Service)
                .ThenInclude(s => s.ServiceCategory)
                .Include(a => a.Branch)
                .Include(a => a.Order)
                .ToListAsync();

            var result = appointments
                .GroupBy(a => a.StaffId) // Chỉ nhóm theo StaffId
                .Select(g => new StaffAppointmentResponse
                {
                    StaffId = g.Key,
                    Appointments = g.Select(a => new AppointmentsInfoModel
                    {
                        AppointmentId = a.AppointmentId,
                        OrderId = a.OrderId,
                        Order = _mapper.Map<OrderInfoModel>(a.Order),
                        CustomerId = a.CustomerId,
                        Customer = _mapper.Map<UserInfoModel>(a.Customer),
                        StaffId = a.StaffId,
                        Staff = _mapper.Map<StaffModel>(a.Staff),
                        ServiceId = a.ServiceId,
                        Service = _mapper.Map<ServiceModel>(a.Service),
                        BranchId = a.BranchId,
                        Branch = _mapper.Map<BranchModel>(a.Branch),
                        AppointmentsTime = a.AppointmentsTime,
                        AppointmentEndTime = a.AppointmentEndTime,
                        Status = a.Status,
                        Notes = a.Notes,
                        Feedback = a.Feedback,
                        Quantity = a.Quantity,
                        UnitPrice = a.UnitPrice,
                        SubTotal = a.Quantity * a.UnitPrice,
                        StatusPayment = a.StatusPayment,
                        CreatedDate = a.CreatedDate,
                        UpdatedDate = a.UpdatedDate
                    }).ToList()
                })
                .ToList();
            
            
            // get image of service
            var listStaffAppointments = result.Select(x => x.Appointments).ToList();
            var listService = new List<Data.Entities.Service>();
            foreach (var listAppointment in listStaffAppointments)
            {
                foreach (var appointment in listAppointment)
                {
                    listService.Add(_mapper.Map<Data.Entities.Service>(appointment.Service));
                }
            }

            var listServiceModel = await _serviceService.GetListImagesOfServices(listService);
        
        
            // map images
            foreach (var listAppointment in listStaffAppointments)
            {
                foreach (var appointment in listAppointment)
                {
                    foreach (var service in listServiceModel)
                    {
                        if (appointment.Service.ServiceId == service.ServiceId)
                        {
                            appointment.Service.images = service.images;
                        }
                    }
                }
            }

            return result;
        }

        public async Task<ApiResult<object>> GetStaffWorkingSlots(int branchId, int month, int year)
        {
            if (branchId <= 0 || month <= 0 || year <= 0)
            {
                return ApiResult<object>.Error(null, "Invalid input.");
            }

            var staffs = await _unitOfWorks.StaffRepository
                .FindByCondition(x => x.BranchId == branchId) // Role = Staff
                .Include(x => x.StaffInfo)
                .ToListAsync();

            if (!staffs.Any())
                return ApiResult<object>.Error(null, "No staff found in this branch.");

            var result = new List<object>();

            foreach (var staff in staffs)
            {
                var workSchedules = await _unitOfWorks.WorkScheduleRepository
                    .FindByCondition(x => x.StaffId == staff.StaffId
                                          && x.WorkDate.Month == month
                                          && x.WorkDate.Year == year
                                          && x.Status == "Active")
                    .Include(x => x.Shift)
                    .ToListAsync();

                var slots = workSchedules.Select(ws => new
                {
                    ws.WorkDate,
                    ws.DayOfWeek,
                    ws.ShiftId,
                    ws.Shift.ShiftName,
                    StartTime = ws.Shift.StartTime,
                    EndTime = ws.Shift.EndTime
                });

                result.Add(new
                {
                    StaffId = staff.StaffId,
                    StaffName = staff.StaffInfo.FullName,
                    Slots = slots
                });
            }

            return ApiResult<object>.Succeed(new
            {
                message = "Get working slots successfully!",
                data = result
            });
        }

        public async Task<ApiResult<object>> GetBranchStaffWorkingSlotsByAppointment(int branchId, int month, int year)
        {
            if (branchId <= 0 || month <= 0 || year <= 0)
                return ApiResult<object>.Error(null, "Invalid input.");

            // Lấy danh sách staff trong branch
            var staffs = await _unitOfWorks.StaffRepository
                .FindByCondition(x => x.BranchId == branchId)
                .Include(x => x.StaffInfo)
                .ToListAsync();

            if (!staffs.Any())
                return ApiResult<object>.Error(null, "No staff found in this branch.");

            var data = new List<object>();

            foreach (var staff in staffs)
            {
                // Lấy appointment của staff trong tháng và năm
                var appointments = await _unitOfWorks.AppointmentsRepository
                    .FindByCondition(a => a.StaffId == staff.StaffId &&
                                          a.AppointmentsTime.Month == month &&
                                          a.AppointmentsTime.Year == year)
                    .ToListAsync();

                var slots = appointments.Select(a => new
                {
                    a.AppointmentId,
                    a.OrderId,
                    a.AppointmentsTime,
                    a.Status,
                    a.Notes
                }).ToList();

                data.Add(new
                {
                    StaffId = staff.StaffId,
                    StaffName = staff.StaffInfo.FullName,
                    Slots = slots
                });
            }

            return ApiResult<object>.Succeed(new
            {
                message = "Get working slots successfully!",
                data = data
            });
        }

        public async Task<ApiResult<object>> GetStaffsBusySlots(List<int> staffIds, int month, int year)
        {
            if (staffIds == null || !staffIds.Any() || month <= 0 || year <= 0)
                return ApiResult<object>.Error(null, "Invalid input.");

            var data = new List<object>();

            foreach (var staffId in staffIds)
            {
                var staff = await _unitOfWorks.StaffRepository
                    .FindByCondition(x => x.StaffId == staffId)
                    .Include(x => x.StaffInfo)
                    .FirstOrDefaultAsync();

                if (staff == null) continue;

                var appointments = await _unitOfWorks.AppointmentsRepository
                    .FindByCondition(a => a.StaffId == staffId &&
                                          a.AppointmentsTime.Month == month &&
                                          a.AppointmentsTime.Year == year)
                    .ToListAsync();

                var slots = appointments.Select(a => new
                {
                    a.AppointmentId,
                    a.OrderId,
                    a.AppointmentsTime,
                    a.Status,
                    a.Notes
                }).ToList();

                data.Add(new
                {
                    StaffId = staff.StaffId,
                    StaffName = staff.StaffInfo.FullName,
                    Slots = slots
                });
            }

            return ApiResult<object>.Succeed(new
            {
                message = "Get busy slots successfully!",
                data = data
            });
        }

    }
}
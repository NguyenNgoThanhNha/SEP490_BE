﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;

namespace Server.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly StaffService _staffService;
        private readonly UserService _userService;
        private readonly StaffLeaveService _staffLeaveService;
        private readonly AuthService _authService;
        private readonly WorkScheduleService _workScheduleService;

        public StaffController(StaffService staffService, UserService userService, StaffLeaveService staffLeaveService, 
            AuthService authService, WorkScheduleService workScheduleService)
        {
            _staffService = staffService;
            _userService = userService;
            _staffLeaveService = staffLeaveService;
            _authService = authService;
            _workScheduleService = workScheduleService;
        }

        [HttpGet("get-list")]
        public async Task<ApiResult<ApiResponse>> GetList(string? name, int pageIndex = 0, int pageSize = 10)
        {
            var response = await _staffService.GetListAsync(pageIndex: pageIndex, pageSize: pageSize, name: name);

            if (response == null || response.Data == null || response.Data.Count == 0)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy dữ liệu nhân viên."
                });
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy danh sách nhân viên thành công.",
                data = response
            });
        }


        [HttpPost("assign-role")]
        public async Task<ApiResult<ApiResponse>> AssignRoleAsync(int staffId, int roleId)
        {
            var response = await _staffService.AssignRoleAsync(staffId, roleId);

            // Kiểm tra nếu response.message == null thì coi như thành công
            if (response.message == null)
            {
                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Phân quyền thành công.",
                    data = response.data
                });
            }

            // Trường hợp thất bại, trả về lỗi
            return ApiResult<ApiResponse>.Error(new ApiResponse
            {
                message = response.message,
                data = response.data
            });
        }


        [HttpPost("assign-branch")]
        public async Task<ApiResult<ApiResponse>> AssignBranchAsync(int staffId, int branchId)
        {
            var response = await _staffService.AssignBranchAsync(staffId, branchId);

            if (response.message == null)
            {
                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Chỉ định nhân viên vào chi nhánh thành công.",
                    data = response.data
                });
            }

            return ApiResult<ApiResponse>.Error(new ApiResponse
            {
                message = response.message
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateStaff([FromBody] CUStaffDto staffDto)
        {
            if (staffDto == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Invalid staff data."
                }));
            }

            var response = await _staffService.CreateStaffAsync(staffDto);

            return Ok(ApiResult<ApiResponse>.Succeed(response));
        }

        [HttpGet("{staffId}")]
        public async Task<ApiResult<ApiResponse>> GetStaffById(int staffId)
        {
            var response = await _staffService.GetStaffByIdAsync(staffId);

            // Kiểm tra nếu response không null thì coi như thành công
            if (response != null)
            {
                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Lấy danh sách nhân viên thành công",
                    data = response
                });
            }

            // Trường hợp thất bại, trả về lỗi
            return ApiResult<ApiResponse>.Error(new ApiResponse
            {
                message = $"Nhân viên với ID {staffId} không tìm thấy.",
                data = null
            });
        }

        [HttpPut("update/{staffId}")]
        public async Task<ApiResult<ApiResponse>> UpdateStaff(int staffId, [FromBody] UpdateStaffDto staffUpdateDto)
        {
            if (staffUpdateDto == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Dữ liệu nhân viên không hợp lệ."
                });
            }

            // Gọi service để cập nhật staff
            var response = await _staffService.UpdateStaffAsync(staffId, staffUpdateDto);

            if (response == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không cập nhật được nhân viên.",
                    data = null
                });
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = response.message,
                data = response.data
            });
        }


        [HttpDelete("delete/{staffId}")]
        public async Task<ApiResult<ApiResponse>> DeleteStaff(int staffId)
        {
            // Gọi service để xử lý logic xóa Staff
            var response = await _staffService.DeleteStaffAsync(staffId);

            // Kiểm tra phản hồi từ service
            if (response.data == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = response.message,
                    data = null
                });
            }

            // Trả về thành công nếu xóa thành công
            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = response.message,
                data = response.data
            });
        }


        [HttpGet("get-staff-familiar")]
        public async Task<ApiResult<ApiResponse>> GetStaffFamiliarAsync(int customerId)
        {
            if (customerId == 0 || await _userService.GetCustomerById(customerId) == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy khách hàng."
                });
            }

            var staffs = await _staffService.GetStaffByCustomerIdAsync(customerId);

            if (staffs == null || staffs.Count == 0)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy dữ liệu nhân viên."
                });
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Danh sách nhân viên đã được lấy thành công.",
                data = staffs
            });
        }

        [HttpGet("get-staff-familiar-last")]
        public async Task<ApiResult<ApiResponse>> GetStaffFamiliarLastAsync(int customerId)
        {
            if (customerId == 0 || await _userService.GetCustomerById(customerId) == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy khách hàng."
                });
            }

            var staff = await _staffService.GetStaffLastByCustomerIdAsync(customerId);

            if (staff == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy dữ liệu nhân viên."
                });
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Nhân viên đã lấy thành công.",
                data = staff
            });
        }

        [HttpGet("by-branch/{branchId}")]
        public async Task<IActionResult> GetStaffByBranch(int branchId)
        {
            try
            {
                var staffList = await _staffService.GetStaffByBranchAsync(branchId);

                if (staffList == null || !staffList.Any())
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Không tìm thấy nhân viên nào cho chi nhánh này..",
                        data = new List<object>() // Trả về danh sách rỗng để đảm bảo format
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Lấy danh sách nhân viên thành công.",
                    data = staffList
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = $"Lỗi hệ thống: {ex.Message}",
                    data = new List<object>()
                }));
            }
        }


        [HttpGet("get-staff-by-branch-and-service")]
        public async Task<IActionResult> GetStaffByBranchAndService(int branchId, int serviceId)
        {
            try
            {
                // Gọi service để lấy danh sách nhân viên
                var staffList = await _staffService.GetStaffByBranchAndServiceAsync(branchId, serviceId);

                if (staffList == null || staffList.Count == 0)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Không tìm thấy nhân viên nào cho chi nhánh và dịch vụ đã cho.",
                        data = new List<object>() // Trả về danh sách rỗng để đảm bảo format
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Danh sách nhân viên đã được tải thành công.",
                    data = staffList
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = $"Lỗi hệ thống: {ex.Message}",
                    data = new List<object>()
                }));
            }
        }


        [HttpGet("staff-busy-times")]
        public async Task<IActionResult> GetStaffBusyTimes(int staffId, DateTime date)
        {
            try
            {
                var busyTimes = await _staffService.GetStaffBusyTimesAsync(staffId, date);

                if (busyTimes == null || !busyTimes.Any())
                {
                    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                    {
                        message = "Không tìm thấy thời gian bận nào của nhân viên vào ngày đã chỉ định.",
                        data = new List<BusyTimeDto>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Lấy thành công thời gian bận của nhân viên!",
                    data = busyTimes
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = $"Lỗi hệ thống: {ex.Message}",
                    data = new List<object>()
                }));
            }
        }

        [Authorize]
        [HttpPost("create-staff-leave")]
        public async Task<IActionResult> CreateStaffLeaveAsync(StaffLeaveRequest staffLeaveRequest)
        {
            var result = await _staffLeaveService.CreateStaffLeaveAsync(staffLeaveRequest);
            if (result == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tạo được ngày nghỉ cho nhân viên."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đã tạo thành công ngày nghỉ của nhân viên.",
                data = result
            }));
        }

        [Authorize("Admin, Manager")]
        [HttpPut("approve-staff-leave/{staffLeaveId}")]
        public async Task<IActionResult> ApproveStaffLeaveAsync(int staffLeaveId, [FromBody] string note)
        {
            var result = await _staffLeaveService.ApproveStaffLeaveAsync(staffLeaveId);
            if (!result)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Từ chối ngày nghỉ phép của nhân viên."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Chấp thuận ngày nghỉ phép của nhân viên.",
                data = note
            }));
        }

        [Authorize("Admin, Manager")]
        [HttpPut("reject-staff-leave/{staffLeaveId}")]
        public async Task<IActionResult> RejectStaffLeaveAsync(int staffLeaveId, [FromBody] string note)
        {
            var result = await _staffLeaveService.RejectStaffLeaveAsync(staffLeaveId);
            if (!result)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Từ chối ngày nghỉ của nhân viên thất bại."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đã từ chối ngày nghỉ của nhân viên.",
                data = note
            }));
        }
        

        [HttpGet("staff-schedule")]
        public async Task<IActionResult> GetStaffScheduleAsync([FromQuery] int year, [FromQuery] int month)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin người dùng!"
                }));
            }

            // RoleID phải là 4 (Staff)
            if (currentUser.RoleID != 4)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Bạn không được phép truy cập vào lịch trình của nhân viên. Chỉ có vai trò nhân viên mới được phép."
                }));
            }

            var staff = await _staffService.GetStaffByUserId(currentUser.UserId);

            if (staff == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin nhân viên!"
                }));
            }

            var schedule = await _staffService.GetStafflistScheduleAsync(staff.StaffId, year, month);

            if (schedule == null || !schedule.Any())
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy lịch trình."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đã lấy lịch trình thành công.",
                data = schedule
            }));
        }


    

        [HttpGet("Manager-Admin-getstaff-schedule")]
        public async Task<IActionResult> ManagerAdminGetStaffScheduleAsync([FromQuery] int year, [FromQuery] int month, [FromQuery] int? staffId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin người dùng!"
                }));
            }

            int actualStaffId;

            if (currentUser.RoleID == 1 || currentUser.RoleID == 2) // Admin or Manager
            {
                if (staffId == null)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "StaffId là bắt buộc đối với Admin/Manager."
                    }));
                }

                actualStaffId = staffId.Value;

                var staffExists = await _staffService.CheckStaffExists(actualStaffId);
                if (!staffExists)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Không tìm thấy chuyên viên được chỉ định."
                    }));
                }
            }
            else
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Bạn không được phép truy cập vào lịch trình này. Chỉ những người có vai trò Admin hoặc Manager mới được phép."
                }));
            }

            var schedule = await _staffService.GetStafflistScheduleAsync(actualStaffId, year, month);

            if (schedule == null || !schedule.Any())
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy lịch trình."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đã lấy lịch trình thành công.",
                data = schedule
            }));
        }


        //[Authorize]
        [HttpGet("cashier-schedule")]
        public async Task<IActionResult> GetCashierScheduleAsync([FromQuery] int staffId, [FromQuery] int year, [FromQuery] int? month, [FromQuery] int? week)
        {
            var schedule = await _staffService.GetCashierScheduleAsync(staffId, year, month, week);

            if (schedule == null || !schedule.Any())
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy lịch làm việc của thu ngân hoặc nhân viên không phải là thu ngân."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy lịch trình của thu ngân thành công.",
                data = schedule
            }));
        }

        // [Authorize]
        [HttpGet("slot-working")]
        public async Task<IActionResult> GetStaffScheduleByMonthAsync([FromQuery] int year, [FromQuery] int month)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin người dùng!"
                }));
            }

            // Kiểm tra RoleID – chỉ Staff được phép
            if (currentUser.RoleID != 4)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không thể truy cập. Chỉ nhân viên mới có thể xem lịch trình của họ."
                }));
            }

            // Lấy Staff theo UserId
            var staff = await _staffService.GetStaffByUserId(currentUser.UserId);
            if (staff == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin nhân viên!"
                }));
            }

            // Gọi hàm Service để lấy lịch theo tháng
            var schedule = await _staffService.GetStaffScheduleByMonthAsync(staff.StaffId, year, month);

            if (schedule == null || schedule.SlotWorkings == null || !schedule.SlotWorkings.Any())
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy lịch làm việc nào của nhân viên này trong tháng đã chọn."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công lịch trình nhân viên làm việc trong tháng!",
                data = schedule
            }));
        }

        [HttpGet("Manager-Admin/slot-working")]
        public async Task<IActionResult> GetStaffSlotWorkingByMonthAsync([FromQuery] int staffId, [FromQuery] int year, [FromQuery] int month)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin người dùng!"
                }));
            }

            // Chỉ cho phép Admin (1) hoặc Manager (2)
            if (currentUser.RoleID != 1 && currentUser.RoleID != 2)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Quyền truy cập bị từ chối. Chỉ có Admin hoặc Manager mới có thể xem vị trí nhân viên đang hoạt động."
                }));
            }

            // Kiểm tra staffId tồn tại
            var staff = await _staffService.GetStaffByIdAsync(staffId);
            if (staff == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy nhân viên có ID đã cung cấp."
                }));
            }

            // Lấy lịch làm việc theo tháng
            var schedule = await _staffService.GetStaffScheduleByMonthAsync(staffId, year, month);

            if (schedule == null || schedule.SlotWorkings == null || !schedule.SlotWorkings.Any())
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy lịch làm việc nào của nhân viên này trong tháng đã chọn."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công lịch làm việc của nhân viên trong tháng!",
                data = schedule
            }));
        }




        [HttpPost("staff-free-in-time")]
        public async Task<IActionResult> ListStaffFreeInTime(ListStaffFreeInTimeRequest request)
        {
            var result = await _staffService.ListStaffFreeInTimeV4(request);

            return Ok(ApiResult<ListStaffFreeInTimeResponse>.Succeed(result));
        }

        
        [HttpPost("staff-by-service-category")]
        public async Task<IActionResult> GetListStaffByServiceCategory(GetListStaffByServiceCategoryRequest request)
        {
            var result = await _staffService.GetListStaffByServiceCategory(request);
            return Ok(ApiResult<GetListStaffByServiceCategoryResponse>.Succeed(result));
        }
        
        [Authorize]
        [HttpGet("work-schedules")]
        public async Task<IActionResult> GetWorkSchedulesByMonthYear([FromQuery] int month, [FromQuery] int year)
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
            var currentUser = await _authService.GetUserInToken(tokenValue);
            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin người dùng!"
                }));
            }

            var staff = await _staffService.GetStaffByCustomerId(currentUser.UserId);
            
            var result = await _workScheduleService.GetWorkSchedulesByMonthYearAsync(staff.StaffId, month, year);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy lịch làm việc thành công",
                data = result
            }));
        }

        [Authorize]
        [HttpGet("shifts")]
        public async Task<IActionResult> GetShiftSlotsByMonthYear([FromQuery] int month, [FromQuery] int year)
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
            var currentUser = await _authService.GetUserInToken(tokenValue);
            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin người dùng!"
                }));
            }

            var staff = await _staffService.GetStaffByCustomerId(currentUser.UserId);
            
            var result = await _workScheduleService.GetShiftSlotsByMonthYearAsync(staff.StaffId, month, year);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy ca làm việc thành công",
                data = result
            }));
        }
        
        [HttpPost("get-staffs-appointments")]
        public async Task<IActionResult> GetStaffsAppointments([FromBody] GetStaffsAppointmentsRequest request)
        {
            var result = await _staffService.GetStaffAppointmentsAsync(request.StaffIds, request.StartDate, request.EndDate);
            return Ok(ApiResult<List<StaffAppointmentResponse>>.Succeed(result));
        }

        [HttpGet("working-slots")]
        public async Task<IActionResult> GetStaffWorkingSlots([FromQuery] int branchId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _staffService.GetStaffWorkingSlots(branchId, month, year);
            return Ok(result);
        }

        [HttpGet("branch-staff-appointments")]
        public async Task<IActionResult> GetBranchStaffWorkingSlotsByAppointment([FromQuery] int branchId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _staffService.GetBranchStaffWorkingSlotsByAppointment(branchId, month, year);
            return Ok(result);
        }

        [HttpPost("staffs-busy-slots")]
        public async Task<IActionResult> GetStaffsBusySlots([FromBody] StaffBusySlotRequest request)
        {
            var result = await _staffService.GetStaffsBusySlots(request.StaffIds, request.Month, request.Year);
            return Ok(result);
        }

        [HttpGet("multiple-staff-busy-times")]
        public async Task<IActionResult> GetMultipleStaffBusyTimes([FromQuery] string staffIds, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(staffIds))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Tham số 'staffIds' là bắt buộc.",
                    data = new List<object>()
                }));
            }

            if (date == default)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Tham số 'date' không hợp lệ.",
                    data = new List<object>()
                }));
            }

            try
            {
                var staffIdList = staffIds.Split(',')
                    .Select(id => int.TryParse(id.Trim(), out var parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (!staffIdList.Any())
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Danh sách 'staffIds' không hợp lệ hoặc không chứa ID hợp lệ.",
                        data = new List<object>()
                    }));
                }

                var result = await _staffService.GetMultipleStaffBusyTimesAsync(staffIdList, date);

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Lấy thành công thời gian bận của các nhân viên!",
                    data = result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Lỗi hệ thống: {ex.Message}",
                    data = new List<object>()
                }));
            }
        }





    }
}

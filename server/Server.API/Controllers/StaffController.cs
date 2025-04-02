using Microsoft.AspNetCore.Authorization;
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
                    message = "No staff data found."
                });
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Staff list retrieved successfully.",
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
                    message = "Role assigned successfully.",
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
                    message = "Branch assigned successfully.",
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
                    message = "Staff retrieved successfully.",
                    data = response
                });
            }

            // Trường hợp thất bại, trả về lỗi
            return ApiResult<ApiResponse>.Error(new ApiResponse
            {
                message = $"Staff with ID {staffId} not found.",
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
                    message = "Invalid staff data."
                });
            }

            // Gọi service để cập nhật staff
            var response = await _staffService.UpdateStaffAsync(staffId, staffUpdateDto);

            if (response == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Failed to update staff.",
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
                    message = "Customer not found."
                });
            }

            var staffs = await _staffService.GetStaffByCustomerIdAsync(customerId);

            if (staffs == null || staffs.Count == 0)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "No staff data found."
                });
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Staff list retrieved successfully.",
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
                    message = "Customer not found."
                });
            }

            var staff = await _staffService.GetStaffLastByCustomerIdAsync(customerId);

            if (staff == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "No staff data found."
                });
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Staff retrieved successfully.",
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
                        message = "No staff found for this branch..",
                        data = new List<object>() // Trả về danh sách rỗng để đảm bảo format
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Get staff list successfully.",
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
                        message = "No staff found for the given branch and service.",
                        data = new List<object>() // Trả về danh sách rỗng để đảm bảo format
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Staff list fetched successfully.",
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
                        message = "No busy times found for the staff on the specified date.",
                        data = new List<BusyTimeDto>()
                    }));
                }

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Successfully retrieved the staff's busy times!",
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
                    message = "Failed to create staff leave."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Staff leave created successfully.",
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
                    message = "Failed to approve staff leave."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Staff leave approved successfully.",
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
                    message = "Failed to reject staff leave."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Staff leave rejected successfully.",
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
                    message = "User info not found!"
                }));
            }

            // RoleID phải là 4 (Staff)
            if (currentUser.RoleID != 4)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "You are not authorized to access staff schedule. Only staff role is allowed."
                }));
            }

            var staff = await _staffService.GetStaffByUserId(currentUser.UserId);

            if (staff == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Staff info not found!"
                }));
            }

            var schedule = await _staffService.GetStafflistScheduleAsync(staff.StaffId, year, month);

            if (schedule == null || !schedule.Any())
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Schedule not found."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Schedule retrieved successfully.",
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
                    message = "User info not found!"
                }));
            }

            int actualStaffId;

            if (currentUser.RoleID == 1 || currentUser.RoleID == 2) // Admin or Manager
            {
                if (staffId == null)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "StaffId is required for Admin/Manager."
                    }));
                }

                actualStaffId = staffId.Value;

                var staffExists = await _staffService.CheckStaffExists(actualStaffId);
                if (!staffExists)
                {
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Specified staff not found."
                    }));
                }
            }
            else
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "You are not authorized to access this schedule. Only Admin or Manager roles are allowed."
                }));
            }

            var schedule = await _staffService.GetStafflistScheduleAsync(actualStaffId, year, month);

            if (schedule == null || !schedule.Any())
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Schedule not found."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Schedule retrieved successfully.",
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
                    message = "Cashier schedule not found or the staff is not a cashier."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Cashier schedule retrieved successfully.",
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
                    message = "User info not found!"
                }));
            }

            // Kiểm tra RoleID – chỉ Staff được phép
            if (currentUser.RoleID != 4)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Access denied. Only staff can view their schedule."
                }));
            }

            // Lấy Staff theo UserId
            var staff = await _staffService.GetStaffByUserId(currentUser.UserId);
            if (staff == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Staff info not found!"
                }));
            }

            // Gọi hàm Service để lấy lịch theo tháng
            var schedule = await _staffService.GetStaffScheduleByMonthAsync(staff.StaffId, year, month);

            if (schedule == null || schedule.SlotWorkings == null || !schedule.SlotWorkings.Any())
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "No slot working found for this staff in the selected month."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Successfully retrieved staff slot working for the month!",
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
                    message = "User info not found!"
                }));
            }

            // Chỉ cho phép Admin (1) hoặc Manager (2)
            if (currentUser.RoleID != 1 && currentUser.RoleID != 2)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Access denied. Only Admin or Manager can view staff slot working."
                }));
            }

            // Kiểm tra staffId tồn tại
            var staff = await _staffService.GetStaffByIdAsync(staffId);
            if (staff == null)
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Staff not found with the provided ID."
                }));
            }

            // Lấy lịch làm việc theo tháng
            var schedule = await _staffService.GetStaffScheduleByMonthAsync(staffId, year, month);

            if (schedule == null || schedule.SlotWorkings == null || !schedule.SlotWorkings.Any())
            {
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "No slot working found for this staff in the selected month."
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Successfully retrieved staff slot working for the month!",
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
                    message = "User info not found!"
                }));
            }

            var staff = await _staffService.GetStaffByCustomerId(currentUser.UserId);
            
            var result = await _workScheduleService.GetWorkSchedulesByMonthYearAsync(staff.StaffId, month, year);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get work schedules successfully",
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
                    message = "User info not found!"
                }));
            }

            var staff = await _staffService.GetStaffByCustomerId(currentUser.UserId);
            
            var result = await _workScheduleService.GetShiftSlotsByMonthYearAsync(staff.StaffId, month, year);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get shift slots successfully",
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

    }
}

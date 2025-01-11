using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
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

        public StaffController(StaffService staffService, UserService userService)
        {
            _staffService = staffService;
            _userService = userService;
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
        public async Task<ApiResult<ApiResponse>> CreateStaff([FromBody] CUStaffDto staffDto)
        {
            if (staffDto == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Invalid staff data."
                });
            }

            var response = await _staffService.CreateStaffAsync(staffDto);

            if (response.message == null)
            {
                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Staff created successfully.",
                    data = response.data
                });
            }

            return ApiResult<ApiResponse>.Error(new ApiResponse
            {
                message = response.message
            });
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
                    return NotFound(new
                    {
                        Message = "Không tìm thấy nhân viên nào cho chi nhánh này."
                    });
                }

                return Ok(new
                {
                    Message = "Lấy danh sách nhân viên thành công!",
                    Data = staffList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = $"Lỗi hệ thống: {ex.Message}"
                });
            }
        }
    }
}

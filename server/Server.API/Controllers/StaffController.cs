using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.API.Controllers.Gaurd;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;

namespace Server.API.Controllers
{
    [Authorize]    
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly StaffService _staffService;
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        public StaffController(StaffService staffService, UserService userService, AppDbContext context)
        {
            _staffService = staffService;
            _context = context;
            _userService = userService;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(string? name, string? description, int pageIndex = 0, int pageSize = 10)
        {
            var response = await _staffService.GetListAsync(
                pageIndex: pageIndex,
                pageSize: pageSize);

            return Ok(ApiResponse.Succeed(response));
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRoleAsync(int staffId, int roleId)
        {
            var response = await _staffService.AssignRoleAsync(staffId, roleId);
            if (response.message == null)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("assign-branch")]
        public async Task<IActionResult> AssignBranchAsync(int staffId, int branchId)
        {
            var response = await _staffService.AssignBranchAsync(staffId, branchId);
            if (response.message == null)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateStaff([FromBody] CUStaffDto staffDto)
        {
            if (staffDto == null)
            {
                return BadRequest("Invalid staff data.");
            }

            try
            {
                staffDto.StaffId = 0;
                var result = await _staffService.CreateStaffAsync(staffDto);

                if (result.message == null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{staffId}")]
        public async Task<IActionResult> GetStaffById(int staffId)
        {

            var staff = await _staffService.GetStaffByIdAsync(staffId);

            if (staff == null)
            {
                return NotFound($"Staff with ID {staffId} not found.");
            }

            return Ok(staff);
        }

        [HttpPut("update{staffId}")]
        public async Task<IActionResult> UpdateStaff(int staffId, [FromBody] CUStaffDto staffUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Error("Invalid model state"));
            }

            if (staffId != staffUpdateDto.StaffId)
            {
                return BadRequest(ApiResponse.Error("Staff id not alike"));
            }

            var result = await _staffService.UpdateStaffAsync(staffUpdateDto);


            if (result.message != null)
            {
                return BadRequest(result);
            }


            return Ok(result);
        }

        [HttpDelete("delete/{staffId}")]
        public async Task<IActionResult> DeleteStaff(int staffId)
        {
            try
            {

                var result = await _staffService.DeleteStaffAsync(staffId);


                if (result.message != null)
                {
                    return BadRequest(result);
                }


                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {

                return NotFound(ApiResponse.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {

                return BadRequest(ApiResponse.Error(ex.Message));
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [CustomAuthorize("Admin,Manager,Customer")]
        [HttpGet("get-staff-familiar")]
        public async Task<IActionResult> GetStaffFamiliarAsync(int customerId)
        {
            if (customerId == 0 || await _userService.GetCustomerById(customerId) == null)
                return BadRequest(new ApiResponse
                {
                    message = "Customer not found"
                });
            var staffs = await _staffService.GetStaffByCustomerIdAsync(customerId);
            if (staffs == null)
                return BadRequest(ApiResponse.Error("No staff data"));
            return Ok(ApiResponse.Succeed(staffs));
        }


        [HttpGet("get-staff-familiar-last")]
        public async Task<IActionResult> GetStaffFamiliarLastAsync(int customerId)
        {
            if (customerId == 0 || await _userService.GetCustomerById(customerId) == null)
                return BadRequest(ApiResponse.Error("Customer not found"));
            var staff = await _staffService.GetStaffLastByCustomerIdAsync(customerId);
            if (staff == null)
                return BadRequest(ApiResponse.Error("No staff data"));
            return Ok(ApiResponse.Succeed(staff));
        }


    }
}

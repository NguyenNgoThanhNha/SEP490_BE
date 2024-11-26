using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
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
        private readonly AppDbContext _context;
        public StaffController(StaffService staffService, AppDbContext context)
        {
            _staffService = staffService;
            _context = context;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(string? name, string? description, int pageIndex = 0, int pageSize = 10)
        {
            var response = await _staffService.GetListAsync(
                pageIndex: pageIndex,
                pageSize: pageSize);

            return Ok(new ApiResult<Pagination<Staff>>
            {
                Success = true,
                Result = response
            });
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRoleAsync(int staffId, int roleId)
        {
            var response = await _staffService.AssignRoleAsync(staffId, roleId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response.ErrorMessage);
        }

        [HttpPost("assign-branch")]
        public async Task<IActionResult> AssignBranchAsync(int staffId, int branchId)
        {
            var response = await _staffService.AssignBranchAsync(staffId, branchId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response.ErrorMessage);
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
                var result = await _staffService.CreateStaffAsync(staffDto);

                if (result.Success)
                {

                    return CreatedAtAction(nameof(GetStaffById), new { staffId = result.Result?.StaffId }, result.Result);
                }
                else
                {
                    return BadRequest(result.ErrorMessage);
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
                return BadRequest(ApiResult<string>.Error("Invalid model state"));
            }

            if (staffId != staffUpdateDto.StaffId)
            {
                return BadRequest(ApiResult<string>.Error(""));
            }

            var result = await _staffService.UpdateStaffAsync(staffUpdateDto);


            if (!result.Success)
            {
                return BadRequest(new ApiResult<string>
                {
                    Success = false,
                    Result = null,
                    ErrorMessage = result.ErrorMessage
                });
            }


            var staff = result.Result;

            return Ok(ApiResult<Staff>.Succeed(staff));
        }

        [HttpDelete("delete/{staffId}")]
        public async Task<IActionResult> DeleteStaff(int staffId)
        {
            try
            {

                var result = await _staffService.DeleteStaffAsync(staffId);


                if (!result.Success)
                {
                    return BadRequest(new { message = result.ErrorMessage });
                }


                return Ok(new { message = result.Result });
            }
            catch (KeyNotFoundException ex)
            {

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


    }
}

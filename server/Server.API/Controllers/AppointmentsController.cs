using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.API.Controllers.Gaurd;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;
using System.Linq.Expressions;

namespace Server.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentsService _appointService;
        private readonly StaffService _staffService;
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        public AppointmentsController(AppointmentsService appointmentsService,
            StaffService staffService,
            UserService userService,
            AppDbContext context)
        {
            _appointService = appointmentsService;
            _staffService = staffService;
            _userService = userService;
            _context = context;
        }

        [HttpGet("get-list")]
        [CustomAuthorize("Admin,Manager,Staff")]
        public async Task<IActionResult> GetList(string? customerName,
            string? staffName,
            string? serviceName,
            string? branchName,
            int pageIndex = 0,
            int pageSize = 10)
        {

            Expression<Func<Appointments, bool>> filter = c => (string.IsNullOrEmpty(customerName) || c.Customer.FullName.ToLower().Contains(customerName.ToLower()))
                && (string.IsNullOrEmpty(staffName) || c.Staff.StaffInfo.FullName.ToLower().Contains(staffName.ToLower()))
                && (string.IsNullOrEmpty(serviceName) || c.Service.Name.ToLower().Contains(serviceName.ToLower()))
                && (string.IsNullOrEmpty(branchName) || c.Branch.BranchName.ToLower().Contains(branchName.ToLower()));

            var response = await _appointService.GetListAsync(
                filter: filter,
                includeProperties: "Customer,Staff,Staff.StaffInfo,Service,Branch",
                pageIndex: pageIndex,
                pageSize: pageSize);

            return Ok(new ApiResponse
            {
                data = response
            });
        }

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

        [HttpPost("create-appointment")]
        public async Task<IActionResult> CreateAppointmentAsync(CUAppointmentDto model)
        {
            if (ModelState.IsValid)
            {
                model.AppointmentsId = 0;
                var appoint = await _appointService.CreateAppointmentAsync(model);
                if (appoint.Success)
                    return Ok(ApiResponse.Succeed(appoint));
                else
                    return BadRequest(ApiResponse.Error(appoint.ErrorMessage));
            }
            return BadRequest(ApiResponse.Error("Please enter complete information"));
        }
    }
}

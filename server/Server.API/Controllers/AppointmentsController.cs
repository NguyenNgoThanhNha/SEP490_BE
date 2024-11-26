using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Services;
using Server.Data.Entities;
using System.Linq.Expressions;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentsService _appointService;
        private readonly AppDbContext _context;
        public AppointmentsController(AppointmentsService appointmentsService, AppDbContext context)
        {
            _appointService = appointmentsService;
            _context = context;
        }

        [HttpGet("get-list")]
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

            return Ok(new ApiResult<Pagination<Appointments>>
            {
                Success = true,
                Result = response
            });
        }

    }
}

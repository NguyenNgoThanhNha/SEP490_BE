using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.API.Controllers.Gaurd;
using Server.Business.Commons.Response;
using Server.Business.Services;
using Server.Data.Entities;

namespace Server.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        public UserController(UserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [CustomAuthorize("Admin,Manager,Staff")]
        [HttpGet("get-customer-list")]
        public async Task<IActionResult> GetCustomerList(int pageIndex = 0, int pageSize = 10)
        {
            var response = await _userService.GetListAsync(
                filter: c => c.RoleID == 3,
                pageIndex: pageIndex,
                pageSize: pageSize);

            return Ok(ApiResponse.Succeed(response));
        }


        [CustomAuthorize("Admin,Manager,Staff")]
        [HttpGet("get-account-detail")]
        public async Task<IActionResult> GetAccountDetail(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest(ApiResponse.Error("Username not null"));
            var user = await _userService.GetAccountDetail(username);
            if (user.message == null)
            {
                return Ok(user);
            }
            else
                return BadRequest(user);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Services;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HubController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        public HubController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }
        
        [HttpPost("create-channel")]
        public async Task<IActionResult> CreateChannelAsync(CreateChannelRequest request)
        {
            var admin = await _mongoDbService.GetCustomerByIdAsync(request.AdminId);
            var channel = await _mongoDbService.CreateChannelAsync(request.ChannelName, admin.Id);
            foreach (var customerId in request.CustomerIds)
            {
                var customer = await _mongoDbService.GetCustomerByIdAsync(customerId);
                await _mongoDbService.AddMemberToChannelAsync(channel.Id, customer.Id);
            }
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Create channel successfully",
            }));
        }
    }
}

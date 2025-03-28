using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Services;
using Server.Data.Entities;
using Server.Data.MongoDb.Models;

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

        // Tạo mới channel
        [HttpPost("create-channel")]
        public async Task<IActionResult> CreateChannelAsync(CreateChannelRequest request)
        {
            var admin = await _mongoDbService.GetCustomerByIdAsync(request.AdminId);
            var channel = await _mongoDbService.CreateChannelAsync(request.ChannelName, admin.Id, request.AppointmentId);
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Create channel successfully",
                data = channel
            }));
        }

        // Lấy thông tin channel theo ID
        [HttpGet("channel/{channelId}")]
        public async Task<IActionResult> GetChannelByIdAsync(string channelId)
        {
            var channel = await _mongoDbService.GetChannelByIdAsync(channelId);
            if (channel == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Channel not found",
            }));

            return Ok(ApiResult<Channels>.Succeed(channel));
        }

        // Lấy danh sách tất cả các kênh của một user
        [HttpGet("user-channels/{customerId}")]
        public async Task<IActionResult> GetAllUserChannelsAsync(string customerId)
        {
            var channels = await _mongoDbService.GetAllUserChannelsAsync(customerId);
            return Ok(ApiResult<List<Channels>>.Succeed(channels));
        }
        
        // Thêm thành viên vào channel
        [HttpPost("add-member")]
        public async Task<IActionResult> AddMemberToChannelAsync(string channelId, string customerId)
        {
            await _mongoDbService.AddMemberToChannelAsync(channelId, customerId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Member added successfully",
            }));
        }

        // Thêm thành viên vào channel
        [HttpPost("add-many-member")]
        public async Task<IActionResult> AddMembersToChannelAsync(string channelId, string[] customerIds)
        {
            await _mongoDbService.AddMembersToChannelAsync(channelId, customerIds);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Members added successfully",
            }));
        }

        // Lấy danh sách tin nhắn của một channel
        [HttpGet("channel-messages/{channelId}")]
        public async Task<IActionResult> GetChannelMessagesAsync(string channelId)
        {
            var messages = await _mongoDbService.GetChannelMessagesAsync(channelId);
            return Ok(ApiResult<List<Messages>>.Succeed(messages));
        }

        // Tìm kiếm khách hàng
        [HttpGet("search-contacts")]
        public async Task<IActionResult> SearchContactsAsync([FromQuery] string searchTerm, [FromQuery] string currentCustomerId)
        {
            var contacts = await _mongoDbService.SearchContactsAsync(searchTerm, currentCustomerId);
            return Ok(ApiResult<List<Customers>>.Succeed(contacts));
        }

        // Lấy tin nhắn giữa hai người dùng
        [HttpGet("messages/{user1}/{user2}")]
        public async Task<IActionResult> GetMessagesAsync(string user1, string user2)
        {
            var messages = await _mongoDbService.GetMessagesAsync(user1, user2);
            return Ok(ApiResult<List<Messages>>.Succeed(messages));
        }

        // Tạo mới khách hàng từ MySQL lên MongoDB
        [HttpPost("create-customer/{userId}")]
        public async Task<IActionResult> CreateCustomerAsync(int userId)
        {
            var customer = await _mongoDbService.CreateCustomerAsync(userId);
            return Ok(ApiResult<Customers>.Succeed(customer));
        }

        // Đồng bộ tất cả khách hàng từ MySQL lên MongoDB
        [HttpPost("sync-customers")]
        public async Task<IActionResult> SyncAllCustomersAsync(List<User> customers)
        {
            await _mongoDbService.SyncAllCustomersAsync(customers);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Sync all customers successfully",
            }));
        }

        // Upload file
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "File is required"
                }));

            using var stream = file.OpenReadStream();
            var filePath = await _mongoDbService.UploadFileAsync(stream, file.FileName);
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "File uploaded successfully",
                data = filePath
            }));
        }
        
        [HttpGet("get-customer-info/{customerId}")]
        public async Task<IActionResult> GetCustomerInfoAsync(int customerId)
        {
            var customer = await _mongoDbService.GetCustomerByIdAsync(customerId);
            if (customer == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Customer not found",
            }));

            return Ok(ApiResult<Customers>.Succeed(customer));
        }
        
        [HttpGet("check-exist-channel")]
        public async Task<IActionResult> CheckExistChannelAsync(int appointmentId)
        {
            var channel = await _mongoDbService.CheckExistChannelAppointmentAsync(appointmentId);
            if (channel == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Channel not found",
            }));

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Channel exists",
                data = channel
            }));
        }
    }
}

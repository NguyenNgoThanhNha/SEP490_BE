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
                message = "Tạo kênh thành công",
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
                message = "Không tìm thấy kênh",
            }));

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy kênh thành công",
                data = channel
            }));
        }

        // Lấy danh sách tất cả các kênh của một user
        [HttpGet("user-channels/{customerId}")]
        public async Task<IActionResult> GetAllUserChannelsAsync(string customerId)
        {
            var channels = await _mongoDbService.GetAllUserChannelsAsync(customerId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy tất cả các kênh thành công",
                data = channels
            }));
        }
        
        // Thêm thành viên vào channel
        [HttpPost("add-member")]
        public async Task<IActionResult> AddMemberToChannelAsync(string channelId, string customerId)
        {
            await _mongoDbService.AddMemberToChannelAsync(channelId, customerId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Thêm thành viên thành công",
            }));
        }

        // Thêm thành viên vào channel
        [HttpPost("add-many-member")]
        public async Task<IActionResult> AddMembersToChannelAsync(string channelId, string[] customerIds)
        {
            await _mongoDbService.AddMembersToChannelAsync(channelId, customerIds);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Thêm thành viên thành công",
            }));
        }

        // Lấy danh sách tin nhắn của một channel
        [HttpGet("channel-messages/{channelId}")]
        public async Task<IActionResult> GetChannelMessagesAsync(string channelId)
        {
            var messages = await _mongoDbService.GetChannelMessagesAsync(channelId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy tin nhắn của kênh thành công",
                data = messages,
            }));
        }

        // Tìm kiếm khách hàng
        [HttpGet("search-contacts")]
        public async Task<IActionResult> SearchContactsAsync([FromQuery] string searchTerm, [FromQuery] string currentCustomerId)
        {
            var contacts = await _mongoDbService.SearchContactsAsync(searchTerm, currentCustomerId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tìm kiếm thành công",
                data = contacts,
            }));
        }

        // Lấy tin nhắn giữa hai người dùng
        [HttpGet("messages/{user1}/{user2}")]
        public async Task<IActionResult> GetMessagesAsync(string user1, string user2)
        {
            var messages = await _mongoDbService.GetMessagesAsync(user1, user2);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy tin nhắn thành công",
                data = messages,
            }));
        }

        // Tạo mới khách hàng từ MySQL lên MongoDB
        [HttpPost("create-customer/{userId}")]
        public async Task<IActionResult> CreateCustomerAsync(int userId)
        {
            var customer = await _mongoDbService.CreateCustomerAsync(userId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                data = customer
            }));
        }

        // Đồng bộ tất cả khách hàng từ MySQL lên MongoDB
        /*[HttpPost("sync-customers")]
        public async Task<IActionResult> SyncAllCustomersAsync(List<User> customers)
        {
            await _mongoDbService.SyncAllCustomersAsync(customers);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đồng bộ hóa tất cả khách hàng thành công",
            }));
        }*/

        // Upload file
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Yêu cầu tệp"
                }));

            using var stream = file.OpenReadStream();
            var filePath = await _mongoDbService.UploadFileAsync(stream, file.FileName);
            
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đã tải tệp lên thành công",
                data = filePath
            }));
        }
        
        [HttpGet("get-customer-info/{customerId}")]
        public async Task<IActionResult> GetCustomerInfoAsync(int customerId)
        {
            var customer = await _mongoDbService.GetCustomerByIdAsync(customerId);
            if (customer == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Không tìm thấy khách hàng",
            }));

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                data = customer
            }));
        }
        
        [HttpGet("check-exist-channel")]
        public async Task<IActionResult> CheckExistChannelAsync(int appointmentId)
        {
            var channel = await _mongoDbService.CheckExistChannelAppointmentAsync(appointmentId);
            if (channel == null) return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
            {
                message = "Không tìm thấy kênh",
            }));

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Kênh tồn tại",
                data = channel
            }));
        }

        [HttpGet("get-channel-appointment/{appointmentId}")]
        public async Task<IActionResult> GetChannelAppointmentAsync(int appointmentId)
        {
            var channel = await _mongoDbService.GetChannelByAppointmentIdAsync(appointmentId);
            if (channel == null)
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy kênh",
                }));

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy kênh thành công",
                data = channel
            }));
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Business.Services;
using Server.Data.MongoDb.Models;
using Server.Data.MongoDb.Repository;
using Server.Data.UnitOfWorks;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MongoDbController : ControllerBase
    {
        private readonly CustomerRepository _customerRepository;
        private readonly ChannelsRepository _channelsRepository;
        private readonly MessageRepository _messageRepository;
        private readonly UnitOfWorks _unitOfWorks;
        private readonly MongoDbService _mongoDbService;

        public MongoDbController(CustomerRepository customerRepository, ChannelsRepository channelsRepository, 
            MessageRepository messageRepository, UnitOfWorks unitOfWorks, MongoDbService mongoDbService 
            )
        {
            _customerRepository = customerRepository;
            _channelsRepository = channelsRepository;
            _messageRepository = messageRepository;
            _unitOfWorks = unitOfWorks;
            _mongoDbService = mongoDbService;
        }
        
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerRepository.GetAllAsync();
            return Ok(customers);
        }
        
        [HttpGet("channels")]
        public async Task<IActionResult> GetChannels()
        {
            var channels = await _channelsRepository.GetAllAsync();
            return Ok(channels);
        }
        
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _messageRepository.GetAllAsync();
            return Ok(messages);
        }
        
        [HttpPost("customers")]
        public async Task<IActionResult> AddCustomer()
        {
            var customer = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == 1);
            var newCustomer = new Customers()
            {
                FullName = customer.FullName ?? "",
                Password = customer.Password ?? "",
                UserId = customer.UserId,
                Image = customer.Avatar ?? "",
                Email = customer.Email,
            };
            var newCustomerCreated = await _customerRepository.AddAsync(newCustomer);
            return Ok(newCustomer);
        }
        
        // sync all customer from MySQL to MongoDB
        [HttpPost("sync-customers")]
        public async Task<IActionResult> SyncCustomers()
        {
            var customers = await _unitOfWorks.UserRepository.GetAll().ToListAsync();
            await _mongoDbService.SyncAllCustomersAsync(customers);
            return Ok();
        }
        
    }
}

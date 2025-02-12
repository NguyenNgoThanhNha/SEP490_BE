using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public MongoDbController(CustomerRepository customerRepository, ChannelsRepository channelsRepository, 
            MessageRepository messageRepository, UnitOfWorks unitOfWorks 
            )
        {
            _customerRepository = customerRepository;
            _channelsRepository = channelsRepository;
            _messageRepository = messageRepository;
            _unitOfWorks = unitOfWorks;
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
                FirstName = "Customer 1",
                LastName = "Customer 1",
                Password = customer.Password ?? "",
                UserId = customer.UserId,
                Image = customer.Avatar ?? "",
                Email = customer.Email,
            };
            var newCustomerCreated = await _customerRepository.AddAsync(newCustomer);
            return Ok(newCustomer);
        }
    }
}

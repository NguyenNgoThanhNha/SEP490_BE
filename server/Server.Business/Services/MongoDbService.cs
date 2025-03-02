using MongoDB.Driver.Linq;
using Server.Data.Entities;
using Server.Data.MongoDb.Models;
using Server.Data.MongoDb.Repository;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class MongoDbService
{
    private readonly CustomerRepository _customerRepository;
    private readonly ChannelsRepository _channelsRepository;
    private readonly MessageRepository _messageRepository;
    private readonly UnitOfWorks _unitOfWorks;

    public MongoDbService(CustomerRepository customerRepository, ChannelsRepository channelsRepository, 
        MessageRepository messageRepository, UnitOfWorks unitOfWorks)
    {
        _customerRepository = customerRepository;
        _channelsRepository = channelsRepository;
        _messageRepository = messageRepository;
        _unitOfWorks = unitOfWorks;
    }
    
    // create customer from MySQL to MongoDB
    public async Task<Customers> CreateCustomerAsync(int userId)
    {
        var customer = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        var newCustomer = new Customers
        {
            FullName = customer.FullName ?? "",
            Password = customer.Password ?? "",
            UserId = customer.UserId,
            Image = customer.Avatar ?? "",
            Email = customer.Email,
        };
        await _customerRepository.AddAsync(newCustomer);
        return newCustomer;
    }
    
    // sync all customer from MySQL to MongoDB
    public async Task SyncAllCustomersAsync(List<User> customers)
    {
        await _customerRepository.RemoveAllAsync();
        foreach (var customer in customers)
        {
            var newCustomer = new Customers
            {
                FullName = customer.FullName ?? "",
                Password = customer.Password ?? "",
                UserId = customer.UserId,
                Image = customer.Avatar ?? "",
                Email = customer.Email,
            };
            await _customerRepository.AddAsync(newCustomer);
        }
    }
    
    // tạo mới channel
    public async Task<Channels> CreateChannelAsync(string name, string adminId)
    {
        var channel = new Channels
        {
            Name = name,
            Admin = adminId,
            Members = new List<string> { adminId }
        };
        await _channelsRepository.AddAsync(channel);
        return channel;
    }
    
    // thêm thành viên vào channel
    public async Task AddMemberToChannelAsync(string channelId, string memberId)
    {
        var channel = await _channelsRepository.GetByIdAsync(channelId);
        if (channel == null) return;
        channel.Members.Add(memberId);
        await _channelsRepository.UpdateAsync(channelId, channel);
    }
    
    // get channel theo id
    public async Task<Channels?> GetChannelByIdAsync(string channelId)
    {
        return await _channelsRepository.GetByIdAsync(channelId);
    }
    
    // get customer theo id
    public async Task<Customers?> GetCustomerByIdAsync(int userId)
    {
        return await _customerRepository.GetByIdAsync(userId);
    }
}
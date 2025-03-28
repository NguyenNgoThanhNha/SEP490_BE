using MongoDB.Bson;
using MongoDB.Driver.Linq;
using Server.Data.Entities;
using Server.Data.MongoDb.Models;
using Server.Data.MongoDb.Repository;
using Server.Data.UnitOfWorks;
using System.IO;
using Server.Business.Exceptions;

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
        var existedCustomer = await _customerRepository.GetByIdAsync(userId);
        if (existedCustomer != null)
        {
            throw new BadRequestException("Customer already existed.");
        }
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
    public async Task<Channels> CreateChannelAsync(string name, string adminId, int appointmentId)
    {
        var channelExists = await _channelsRepository
            .GetManyAsync(c => c.Name == name && c.AppointmentId == appointmentId);
        if (channelExists.Any())
        {
            throw new Exception($"Channel's {name} already exists.");
        }
        var admin = await _customerRepository.GetByIdAsync(adminId);
        if (admin == null)
        {
            throw new Exception("Admin Not Found.");
        }

        var channel = new Channels
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = name,
            Admin = adminId,
            AppointmentId = appointmentId,
            CreateAt = DateTime.UtcNow
        };
        await _channelsRepository.AddAsync(channel);
        return channel;
    }
    
    // thêm thành viên vào channel
    public async Task AddMemberToChannelAsync(string channelId, string memberId)
    {
        var channel = await _channelsRepository.GetByIdAsync(channelId);
        if (channel == null) 
            throw new Exception("Channel not found");

        if (channel.Members.Contains(memberId)) 
            throw new Exception("User is already a member of this channel");

        channel.Members.Add(memberId);
        await _channelsRepository.UpdateAsync(channelId, channel);
    }
    
    
    // thêm nhiều thành viên vào channel
    public async Task AddMembersToChannelAsync(string channelId, string[] memberIds)
    {
        var channel = await _channelsRepository.GetByIdAsync(channelId);
        if (channel == null) 
            throw new Exception("Channel not found");

        foreach (var memberId in memberIds)
        {
            if (channel.Members.Contains(memberId)) 
                throw new Exception("User is already a member of this channel");

            channel.Members.Add(memberId);
            await _channelsRepository.UpdateAsync(channelId, channel);
        }
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
    
    // get all user channels
    public async Task<List<Channels>> GetAllUserChannelsAsync(string customerId)
    {
        return await _channelsRepository.GetManyAsync(c => c.Admin == customerId || c.Members.Contains(customerId));
    }
    
    // get messages of a channel
    public async Task<List<Messages>> GetChannelMessagesAsync(string channelId)
    {
        var channel = await _channelsRepository.GetByIdAsync(channelId);
        if (channel == null)
        {
            throw new Exception("Channel not found");
        }

        // Lấy danh sách messageId từ channel
        var messageIds = channel.Messages;
    
        if (messageIds == null || !messageIds.Any())
        {
            return new List<Messages>();
        }

        // Lấy danh sách tin nhắn dựa trên danh sách ID
        var result = await _messageRepository.GetMessagesByIdsAsync(messageIds);
        return result;
    }
    
    // search contacts
    public async Task<List<Customers>> SearchContactsAsync(string searchTerm, string currentCustomerId)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new Exception("Search term is required");
        }

        return await _customerRepository.GetManyAsync(c => 
            (c.FullName.Contains(searchTerm) || c.Email.Contains(searchTerm)) && c.Id != currentCustomerId);
    }
    
    // get messages between two users
    public async Task<List<Messages>> GetMessagesAsync(string user1, string user2)
    {
        return await _messageRepository.GetManyAsync(m =>
            (m.Sender == user1 && m.Recipient == user2) || (m.Sender == user2 && m.Recipient == user1));
    }
    
    // upload file
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var dateFolder = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var fileDir = Path.Combine("upload", "files", dateFolder);
        var filePath = Path.Combine(fileDir, fileName);

        if (!Directory.Exists(fileDir))
        {
            Directory.CreateDirectory(fileDir);
        }

        using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(file);
        }

        return filePath;
    }
    
    // check exist channel appointment
    public async Task<object> CheckExistChannelAppointmentAsync(int appointmentId, string channelName)
    {
        var channel = await _channelsRepository.GetOneAsync(c => c.AppointmentId == appointmentId && c.Name.ToLower() == channelName.ToLower());
        return channel;
    }
}

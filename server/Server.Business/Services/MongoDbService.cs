using Server.Data.MongoDb.Models;
using Server.Data.MongoDb.Repository;

namespace Server.Business.Services;

public class MongoDbService
{
    private readonly CustomerRepository _customerRepository;
    private readonly ChannelsRepository _channelsRepository;
    private readonly MessageRepository _messageRepository;
    public MongoDbService(CustomerRepository customerRepository, ChannelsRepository channelsRepository, MessageRepository messageRepository)
    {
        _customerRepository = customerRepository;
        _channelsRepository = channelsRepository;
        _messageRepository = messageRepository;
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
}
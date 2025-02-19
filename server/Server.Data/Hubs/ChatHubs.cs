using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Server.Data.MongoDb.Models;
using Server.Data.MongoDb.Repository;

public class ChatHubs : Hub
{
    private readonly CustomerRepository _customerRepository;
    private readonly ChannelsRepository _channelsRepository;
    private readonly MessageRepository _messageRepository;
    private static readonly ConcurrentDictionary<string, string> UserSocketMap = new();

    public ChatHubs(CustomerRepository customerRepository, ChannelsRepository channelsRepository, MessageRepository messageRepository)
    {
        _customerRepository = customerRepository;
        _channelsRepository = channelsRepository;
        _messageRepository = messageRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["id"];
        if (!string.IsNullOrEmpty(userId))
        {
            UserSocketMap[userId] = Context.ConnectionId;
            Console.WriteLine($"User Connected: {userId} with socket Id: {Context.ConnectionId}");
        }
        else
        {
            Console.WriteLine("User Id not found during connection");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userEntry = UserSocketMap.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (!string.IsNullOrEmpty(userEntry.Key))
        {
            UserSocketMap.TryRemove(userEntry.Key, out _);
            Console.WriteLine($"User Disconnected: {Context.ConnectionId}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string senderId, string recipientId, string content)
    {
        var sender = await _customerRepository.GetByIdAsync(senderId);
        if (sender == null) return;

        var message = new Messages
        {
            Sender = senderId,
            Recipient = recipientId,
            MessageType = "text",
            Content = content,
            Timestamp = DateTime.UtcNow
        };
        await _messageRepository.AddAsync(message);

        if (UserSocketMap.TryGetValue(recipientId, out var recipientSocketId))
        {
            await Clients.Client(recipientSocketId).SendAsync("receiveMessage", sender.FirstName, content);
        }
        if (UserSocketMap.TryGetValue(senderId, out var senderSocketId))
        {
            await Clients.Client(senderSocketId).SendAsync("receiveMessage", sender.FirstName, content);
        }
    }

    public async Task SendMessageToChannel(string channelId, string senderId, string content, string messageType, string fileUrl = null)
    {
        var sender = await _customerRepository.GetByIdAsync(senderId);
        if (sender == null) return;

        var message = new Messages
        {
            Sender = senderId,
            Recipient = null,
            Content = content,
            MessageType = messageType,
            Timestamp = DateTime.UtcNow,
            FileUrl = fileUrl
        };
        await _messageRepository.AddAsync(message);

        var channel = await _channelsRepository.GetByIdAsync(channelId);
        if (channel == null) return;

        channel.Messages.Add(message.Id);
        await _channelsRepository.UpdateAsync(channel.Id,channel);

        foreach (var memberId in channel.Members)
        {
            if (UserSocketMap.TryGetValue(memberId, out var memberSocketId))
            {
                await Clients.Client(memberSocketId).SendAsync("receiveChannelMessage", message);
            }
        }
    }
}

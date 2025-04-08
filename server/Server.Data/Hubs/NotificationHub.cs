using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

public class NotificationHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> UserSocketMap = new();
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"];
        if (!string.IsNullOrEmpty(userId))
        {
            UserSocketMap[userId] = Context.ConnectionId;
            _logger.LogInformation($"[NotificationHub] Connected: {userId} => {Context.ConnectionId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userEntry = UserSocketMap.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (!string.IsNullOrEmpty(userEntry.Key))
        {
            UserSocketMap.TryRemove(userEntry.Key, out _);
            _logger.LogInformation($"[NotificationHub] Disconnected: {userEntry.Key}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public static bool TryGetConnectionId(string userId, out string connectionId)
    {
        return UserSocketMap.TryGetValue(userId, out connectionId);
    }
}
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Server.Business.Services;

public class RedisKeepAliveService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisKeepAliveService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_redis.IsConnected)
            {
                Console.WriteLine("Redis mất kết nối, đang tái kết nối...");
                await _redis.GetDatabase().PingAsync(); // Thử gửi ping để kiểm tra kết nối
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Kiểm tra mỗi phút
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Business.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Business.Worker
{
    public class OrderStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderStatusUpdateService> _logger;

        public OrderStatusUpdateService(IServiceProvider serviceProvider, ILogger<OrderStatusUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Tạo scope để lấy dịch vụ cần thiết
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

                        // Kiểm tra và cập nhật các đơn hàng dựa trên điều kiện
                        await orderService.UpdateOrderStatusBasedOnPayment();

                        // Log thông tin sau mỗi lần kiểm tra
                        _logger.LogInformation("Checked and updated order statuses at: {time}", DateTimeOffset.Now);
                    }

                    // Chờ 24 giờ trước khi chạy lại
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi xảy ra, log lại và tiếp tục
                    _logger.LogError(ex, "An error occurred while updating order statuses.");
                }
            }
        }
    }
}

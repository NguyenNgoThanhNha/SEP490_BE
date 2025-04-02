using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Business.Services;
using System;
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
            _logger.LogInformation("OrderStatusUpdateService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

                    // Task 1: Auto Cancel đơn hàng chưa thanh toán
                    await orderService.UpdateOrderStatusBasedOnPayment();

                    // Task 2: Auto Complete đơn hàng giao xong 3 ngày
                    await orderService.AutoCompleteOrderAfterDelivery();

                    _logger.LogInformation("OrderStatusUpdateService ran successfully at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing order status updates.");
                }

                // Chờ 10 phút rồi lặp lại
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }

            _logger.LogInformation("OrderStatusUpdateService is stopping.");
        }
    }
}

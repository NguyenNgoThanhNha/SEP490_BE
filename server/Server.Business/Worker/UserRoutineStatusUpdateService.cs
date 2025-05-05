using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Server.Data.UnitOfWorks;
using Server.Business.Constants;
using Server.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class UserRoutineStatusUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRoutineStatusUpdateService> _logger;
    private readonly TimeSpan ScheduledTime = new TimeSpan(2, 0, 0); 

    public UserRoutineStatusUpdateService(IServiceProvider serviceProvider, ILogger<UserRoutineStatusUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRunTime = DateTime.UtcNow.Date.AddDays(now.TimeOfDay > ScheduledTime ? 1 : 0).Add(ScheduledTime);
            var delay = nextRunTime - now;

            _logger.LogInformation($"[UserRoutineStatusUpdateService] Waiting {delay.TotalMinutes} minutes until next run at {nextRunTime} (UTC).");

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            await ExecuteCoreLogicAsync(stoppingToken);
        }
    }

    public async Task ManualRunAsync()
    {
        await ExecuteCoreLogicAsync(CancellationToken.None);
    }

    private async Task ExecuteCoreLogicAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWorks = scope.ServiceProvider.GetRequiredService<UnitOfWorks>();

                var userRoutines = await unitOfWorks.UserRoutineRepository
                    .FindByCondition(ur => ur.Status != "Completed")
                    .Include(ur => ur.UserRoutineSteps)
                    .ToListAsync(stoppingToken);

                foreach (var userRoutine in userRoutines)
                {
                    bool allStepsCompleted = userRoutine.UserRoutineSteps != null
                        && userRoutine.UserRoutineSteps.Any()
                        && userRoutine.UserRoutineSteps.All(s => s.StepStatus == "Completed");

                    if (allStepsCompleted)
                    {
                        var orders = await unitOfWorks.OrderRepository
                            .FindByCondition(o => o.RoutineId == userRoutine.RoutineId
                                                   && o.CustomerId == userRoutine.UserId
                                                   && o.OrderType == OrderType.Routine.ToString()
                                                   && o.Status != OrderStatusEnum.Completed.ToString())
                            .Include(o => o.OrderDetails)
                            .ToListAsync(stoppingToken);

                        foreach (var order in orders)
                        {
                            order.Status = OrderStatusEnum.Completed.ToString();
                            order.UpdatedDate = DateTime.UtcNow;
                            unitOfWorks.OrderRepository.Update(order);

                            if (order.OrderDetails != null && order.OrderDetails.Any())
                            {
                                foreach (var detail in order.OrderDetails)
                                {
                                    detail.Status = OrderStatusEnum.Completed.ToString();
                                    detail.UpdatedDate = DateTime.UtcNow;
                                    unitOfWorks.OrderDetailRepository.Update(detail);
                                }
                            }

                            _logger.LogInformation($"[UserRoutineStatusUpdateService] đã cập nhật OrderId = {order.OrderId} sang Completed.");
                        }
                        
                        userRoutine.Status = ObjectStatus.Completed.ToString();
                        userRoutine.UpdatedDate = DateTime.Now;
                        unitOfWorks.UserRoutineRepository.Update(userRoutine);
                        await unitOfWorks.UserRoutineRepository.Commit();
                        _logger.LogInformation($"[UserRoutineStatusUpdateService] đã cập nhật UserRoutineId = {userRoutine.UserRoutineId} sang Completed.");
                    }
                }

                await unitOfWorks.OrderDetailRepository.Commit();
                await unitOfWorks.OrderRepository.Commit();
                _logger.LogInformation("[UserRoutineStatusUpdateService] kết thúc cập nhật Routine Orders lúc " + DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Đã xảy ra lỗi khi update.");
        }
    }
}

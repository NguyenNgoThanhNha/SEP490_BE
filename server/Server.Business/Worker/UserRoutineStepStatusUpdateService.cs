using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Server.Business.Constants;
using Server.Data;

public class UserRoutineStepStatusUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRoutineStepStatusUpdateService> _logger;

    // Thời gian chạy hàng ngày (Giờ:Phút:Giây)
    private readonly TimeSpan ScheduledTime = new TimeSpan(0, 0, 0); 

    public UserRoutineStepStatusUpdateService(IServiceProvider serviceProvider, ILogger<UserRoutineStepStatusUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRunTime = DateTime.Today.AddDays(now.TimeOfDay > ScheduledTime ? 1 : 0).Add(ScheduledTime);
            var delay = nextRunTime - now;

            _logger.LogInformation($"[UserRoutineStepStatusUpdateService] Waiting {delay.TotalMinutes} minutes until next run at {nextRunTime}.");

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return; 
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var unitOfWorks = scope.ServiceProvider.GetRequiredService<UnitOfWorks>();

                    var userRoutines = await unitOfWorks.UserRoutineRepository
                        .FindByCondition(ur => ur.Status != "Completed")
                        .Include(ur => ur.UserRoutineSteps)
                            .ThenInclude(urs => urs.SkinCareRoutineStep)
                        .ToListAsync(stoppingToken);

                    foreach (var userRoutine in userRoutines)
                    {
                        foreach (var step in userRoutine.UserRoutineSteps)
                        {
                            var skinCareRoutineId = step.SkinCareRoutineStep?.SkincareRoutineId;

                            if (skinCareRoutineId == null) continue;

                            // Lấy danh sách OrderId của user trong routine này
                            var relatedOrderIds = await unitOfWorks.OrderRepository
                                .FindByCondition(o =>
                                    o.CustomerId == userRoutine.UserId &&
                                    o.RoutineId == skinCareRoutineId &&
                                    o.OrderType == OrderType.Routine.ToString())
                                .Select(o => o.OrderId)
                                .ToListAsync(stoppingToken);

                            if (!relatedOrderIds.Any()) continue;

                            // Lấy các appointment của những order đó
                            var relatedAppointments = await unitOfWorks.AppointmentsRepository
                                .FindByCondition(a =>
                                    a.OrderId.HasValue &&
                                    relatedOrderIds.Contains(a.OrderId.Value))
                                .ToListAsync(stoppingToken);

                            // Kiểm tra nếu tất cả đều Completed
                            bool allAppointmentsCompleted = relatedAppointments.Any() &&
                                relatedAppointments.All(a => a.Status == OrderStatusEnum.Completed.ToString());

                            if (allAppointmentsCompleted)
                            {
                                step.StepStatus = "Completed";
                                step.UpdatedDate = DateTime.Now;
                                unitOfWorks.UserRoutineStepRepository.Update(step);

                                _logger.LogInformation($"✔ StepId = {step.UserRoutineStepId} của UserRoutineId = {userRoutine.UserRoutineId} đã được cập nhật sang COMPLETE.");
                            }
                        }
                    }

                    await unitOfWorks.UserRoutineStepRepository.Commit();
                    _logger.LogInformation("Đã hoàn tất cập nhật StepStatus cho các bước lúc " + DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật StepStatus trong background task.");
            }
        }
    }
}

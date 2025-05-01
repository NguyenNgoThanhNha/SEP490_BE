//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.EntityFrameworkCore;
//using Server.Business.Constants;
//using Server.Business.Services;
//using Server.Business.Ultils;
//using Server.Data.UnitOfWorks;
//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Server.Data;
//using Server.Data.Entities;

//public class UserRoutineStepStatusUpdateService : BackgroundService
//{
//    private readonly IServiceProvider _serviceProvider;
//    private readonly ILogger<UserRoutineStepStatusUpdateService> _logger;
//    private readonly TimeSpan ScheduledTime = new TimeSpan(2, 0, 0); // 9h sáng VN (UTC+7)

//    public UserRoutineStepStatusUpdateService(IServiceProvider serviceProvider, ILogger<UserRoutineStepStatusUpdateService> logger)
//    {
//        _serviceProvider = serviceProvider;
//        _logger = logger;

//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            var now = DateTime.UtcNow;
//            var nextRunTime = now.Date.AddDays(now.TimeOfDay > ScheduledTime ? 1 : 0).Add(ScheduledTime);
//            var delay = nextRunTime - now;

//            _logger.LogInformation($"[UserRoutineStepStatusUpdateService] Waiting {delay.TotalMinutes} minutes until next run at {nextRunTime} UTC.");

//            try { await Task.Delay(delay, stoppingToken); }
//            catch (TaskCanceledException) { return; }

//            await ExecuteCoreLogicAsync(stoppingToken);
//        }
//    }

//    public async Task ManualRunAsync()
//    {
//        await ExecuteCoreLogicAsync(CancellationToken.None);
//    }

//    private async Task ExecuteCoreLogicAsync(CancellationToken stoppingToken)
//    {
//        try
//        {
//            using var scope = _serviceProvider.CreateScope();
//            var unitOfWorks = scope.ServiceProvider.GetRequiredService<UnitOfWorks>();
//            var mailService = scope.ServiceProvider.GetRequiredService<MailService>();

//            var userRoutines = await unitOfWorks.UserRoutineRepository
//                .FindByCondition(ur => ur.Status != "Completed")
//                .Include(ur => ur.UserRoutineSteps)
//                    .ThenInclude(urs => urs.SkinCareRoutineStep)
//                .ToListAsync(stoppingToken);

//            foreach (var userRoutine in userRoutines)
//            {
//                var steps = userRoutine.UserRoutineSteps
//                    .Where(s => s.SkinCareRoutineStep != null)
//                    .OrderBy(s => s.SkinCareRoutineStep.Step)
//                    .ToList();

//                for (int i = 0; i < steps.Count; i++)
//                {
//                    var step = steps[i];
//                    var stepNum = step.SkinCareRoutineStep.Step;
//                    var routineId = step.SkinCareRoutineStep.SkincareRoutineId;
//                    if (routineId == null) continue;

//                    var orderIds = await unitOfWorks.OrderRepository
//                        .FindByCondition(o => o.CustomerId == userRoutine.UserId &&
//                                              o.RoutineId == routineId &&
//                                              o.OrderType == OrderType.Routine.ToString())
//                        .Select(o => o.OrderId)
//                        .ToListAsync(stoppingToken);

//                    if (!orderIds.Any()) continue;

//                    var appointments = await unitOfWorks.AppointmentsRepository
//                        .FindByCondition(a => a.OrderId.HasValue &&
//                                              orderIds.Contains(a.OrderId.Value) &&
//                                              a.Step.HasValue &&
//                                              a.Step.Value == stepNum)
//                        .Include(a => a.Customer)
//                        .Include(a => a.Service)
//                        .ToListAsync(stoppingToken);

//                    bool allCompleted = appointments.Any() &&
//                        appointments.All(a => a.Status == OrderStatusEnum.Completed.ToString());

//                    if (allCompleted && step.StepStatus != "Completed")
//                    {
//                        step.SkinCareRoutineStep = null; // tránh lỗi tracking
//                        step.StepStatus = "Completed";
//                        step.UpdatedDate = DateTime.UtcNow;

//                        unitOfWorks.UserRoutineStepRepository.Update(step);
//                        _logger.LogInformation($"✔ Step {step.UserRoutineStepId} updated to Completed.");
//                    }

//                    if (step.StepStatus == "Completed" && i + 1 < steps.Count)
//                    {
//                        var nextStep = steps[i + 1];
//                        if (nextStep.StepStatus == "Completed") continue;

//                        var nextStepNum = nextStep.SkinCareRoutineStep.Step;

//                        var nextAppointments = await unitOfWorks.AppointmentsRepository
//                            .FindByCondition(a => a.OrderId.HasValue &&
//                                                  orderIds.Contains(a.OrderId.Value) &&
//                                                  a.Step.HasValue &&
//                                                  a.Step.Value == nextStepNum)
//                            .Include(a => a.Customer)
//                            .Include(a => a.Service)
//                            .ToListAsync(stoppingToken);

//                        foreach (var appointment in nextAppointments)
//                        {
//                            var appointmentDateUtc = appointment.AppointmentsTime.Date;
//                            var targetDate = DateTime.UtcNow.Date.AddDays(1);

//                            if (appointmentDateUtc == targetDate)
//                            {
//                                var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(
//                                    appointment.AppointmentsTime,
//                                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

//                                var mailData = new MailData
//                                {
//                                    EmailToId = appointment.Customer.Email,
//                                    EmailToName = appointment.Customer.FullName,
//                                    EmailSubject = "[Nhắc nhở] Bước tiếp theo trong liệu trình chăm sóc da",
//                                    EmailBody = $@"
//<div style='max-width:600px;margin:20px auto;padding:20px;background-color:#f9f9f9;border-radius:10px;box-shadow:0 0 10px rgba(0,0,0,0.1);'>
//  <h2 style='text-align:center;color:#3498db;'>Nhắc nhở bước tiếp theo trong liệu trình</h2>
//  <p style='font-size:16px;'>Chào {appointment.Customer.FullName},</p>
//  <p>Bạn đã hoàn thành bước {stepNum}.</p>
//  <p>Đây là nhắc nhở cho bước tiếp theo:</p>
//  <ul style='list-style-type:none;'>
//    <li><strong>Bước tiếp theo:</strong> Bước {nextStepNum}</li>
//    <li><strong>Thời gian thực hiện:</strong> {vietnamTime:yyyy-MM-dd HH:mm}</li>
//  </ul>
//  <p>Vui lòng đến đúng giờ. Nếu có bất kỳ thắc mắc nào, xin liên hệ với chúng tôi.</p>
//  <p style='text-align:center;color:#aaa;'>Đội ngũ Solace Spa trân trọng!</p>
//</div>"
//                                };

//                                var sent = await mailService.SendEmailAsync(mailData, false);
//                                if (sent)
//                                    _logger.LogInformation($"📩 Email sent to {appointment.Customer.Email} for Step {nextStepNum}.");
//                                else
//                                    _logger.LogError($"❌ Failed to send email to {appointment.Customer.Email}.");
//                            }
//                        }
//                    }
//                }
//            }

//            await unitOfWorks.UserRoutineStepRepository.Commit();
//            _logger.LogInformation("✅ Finished updating step statuses and sending routine reminders.");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "❌ Error during step update and reminder process.");
//        }
//    }
//}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Server.Business.Constants;
using Server.Business.Services;
using Server.Business.Ultils;
using Server.Data.UnitOfWorks;
using Server.Data.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Server.Data;

public class UserRoutineStepStatusUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRoutineStepStatusUpdateService> _logger;
    private readonly TimeSpan ScheduledTime = new TimeSpan(2, 0, 0); // 9h sáng VN (UTC+7)

    public UserRoutineStepStatusUpdateService(IServiceProvider serviceProvider, ILogger<UserRoutineStepStatusUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRunTime = now.Date.AddDays(now.TimeOfDay > ScheduledTime ? 1 : 0).Add(ScheduledTime);
            var delay = nextRunTime - now;

            _logger.LogInformation($"[UserRoutineStepStatusUpdateService] Waiting {delay.TotalMinutes} minutes until next run at {nextRunTime} UTC.");

            try { await Task.Delay(delay, stoppingToken); }
            catch (TaskCanceledException) { return; }

            await ExecuteCoreLogicAsync(stoppingToken);
        }
    }

    public async Task ManualRunAsync() => await ExecuteCoreLogicAsync(CancellationToken.None);

    private async Task ExecuteCoreLogicAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWorks = scope.ServiceProvider.GetRequiredService<UnitOfWorks>();
            var mailService = scope.ServiceProvider.GetRequiredService<MailService>();

            var userRoutines = await unitOfWorks.UserRoutineRepository
                .FindByCondition(ur => ur.Status != "Completed")
                .Include(ur => ur.UserRoutineSteps)
                    .ThenInclude(urs => urs.SkinCareRoutineStep)
                .ToListAsync(stoppingToken);

            foreach (var userRoutine in userRoutines)
            {
                var steps = userRoutine.UserRoutineSteps
                    .Where(s => s.SkinCareRoutineStep != null)
                    .OrderBy(s => s.SkinCareRoutineStep.Step)
                    .ToList();

                for (int i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    var stepNum = step.SkinCareRoutineStep.Step;
                    var routineId = step.SkinCareRoutineStep.SkincareRoutineId;

                    if (routineId == null) continue;

                    var orderIds = await unitOfWorks.OrderRepository
                        .FindByCondition(o => o.CustomerId == userRoutine.UserId &&
                                              o.RoutineId == routineId &&
                                              o.OrderType == OrderType.Routine.ToString())
                        .Select(o => o.OrderId)
                        .ToListAsync(stoppingToken);

                    if (!orderIds.Any()) continue;

                    var appointments = await unitOfWorks.AppointmentsRepository
                        .FindByCondition(a => a.OrderId.HasValue &&
                                              orderIds.Contains(a.OrderId.Value) &&
                                              a.Step.HasValue &&
                                              a.Step.Value == stepNum)
                        .ToListAsync(stoppingToken);

                    bool allCompleted = appointments.Any() &&
                        appointments.All(a => a.Status == OrderStatusEnum.Completed.ToString());

                    if (allCompleted && step.StepStatus != "Completed")
                    {
                        // Tách thực thể khỏi navigation để tránh lỗi tracking
                        var targetStep = await unitOfWorks.UserRoutineStepRepository
                            .FindByCondition(s => s.UserRoutineStepId == step.UserRoutineStepId)
                            .FirstOrDefaultAsync(stoppingToken);

                        if (targetStep != null)
                        {
                            targetStep.StepStatus = "Completed";
                            targetStep.UpdatedDate = DateTime.UtcNow;

                            unitOfWorks.UserRoutineStepRepository.Update(targetStep);
                            _logger.LogInformation($"✔ Step {targetStep.UserRoutineStepId} updated to Completed.");
                        }
                    }

                    // Gửi email nếu bước tiếp theo là ngày mai
                    if (step.StepStatus == "Completed" && i + 1 < steps.Count)
                    {
                        var nextStep = steps[i + 1];
                        if (nextStep.StepStatus == "Completed") continue;

                        var nextStepNum = nextStep.SkinCareRoutineStep.Step;

                        var nextAppointments = await unitOfWorks.AppointmentsRepository
                            .FindByCondition(a => a.OrderId.HasValue &&
                                                  orderIds.Contains(a.OrderId.Value) &&
                                                  a.Step.HasValue &&
                                                  a.Step.Value == nextStepNum)
                            .Include(a => a.Customer)
                            .ToListAsync(stoppingToken);

                        foreach (var appointment in nextAppointments)
                        {
                            var appointmentDateUtc = appointment.AppointmentsTime.Date;
                            var targetDate = DateTime.UtcNow.Date.AddDays(1);

                            if (appointmentDateUtc == targetDate)
                            {
                                var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(
                                    appointment.AppointmentsTime,
                                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                                var mailData = new MailData
                                {
                                    EmailToId = appointment.Customer.Email,
                                    EmailToName = appointment.Customer.FullName,
                                    EmailSubject = "[Nhắc nhở] Bước tiếp theo trong liệu trình chăm sóc da",
                                    EmailBody = $@"
<div style='max-width:600px;margin:20px auto;padding:20px;background-color:#f9f9f9;border-radius:10px;box-shadow:0 0 10px rgba(0,0,0,0.1);'>
  <h2 style='text-align:center;color:#3498db;'>Nhắc nhở bước tiếp theo trong liệu trình</h2>
  <p style='font-size:16px;'>Chào {appointment.Customer.FullName},</p>
  <p>Bạn đã hoàn thành bước {stepNum}.</p>
  <p>Đây là nhắc nhở cho bước tiếp theo:</p>
  <ul style='list-style-type:none;'>
    <li><strong>Bước tiếp theo:</strong> Bước {nextStepNum}</li>
    <li><strong>Thời gian thực hiện:</strong> {vietnamTime:yyyy-MM-dd HH:mm}</li>
  </ul>
  <p>Vui lòng đến đúng giờ. Nếu có bất kỳ thắc mắc nào, xin liên hệ với chúng tôi.</p>
  <p style='text-align:center;color:#aaa;'>Đội ngũ Solace Spa trân trọng!</p>
</div>"
                                };

                                var sent = await mailService.SendEmailAsync(mailData, false);
                                if (sent)
                                    _logger.LogInformation($"📩 Email sent to {appointment.Customer.Email} for Step {nextStepNum}.");
                                else
                                    _logger.LogError($"❌ Failed to send email to {appointment.Customer.Email}.");
                            }
                        }
                    }
                }
            }

            await unitOfWorks.UserRoutineStepRepository.Commit();
            _logger.LogInformation("✅ Finished updating step statuses and sending routine reminders.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during step update and reminder process.");
        }
    }
}


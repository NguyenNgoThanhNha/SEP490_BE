using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Business.Ultils;
using Server.Data.UnitOfWorks;
using Server.Data;
using Server.Business.Services;
using Microsoft.EntityFrameworkCore;
using Server.Business.Constants;

public class UserRoutineStepStatusUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRoutineStepStatusUpdateService> _logger;
    private readonly TimeSpan ScheduledTime = new TimeSpan(2, 0, 0); // 2h UTC = 9h sáng VN

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

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWorks = scope.ServiceProvider.GetRequiredService<UnitOfWorks>();
                var mailService = scope.ServiceProvider.GetRequiredService<MailService>();

                var userRoutines = await unitOfWorks.UserRoutineRepository
                    .FindByCondition(ur => ur.Status != "Completed")
                    .Include(ur => ur.UserRoutineSteps)
                        .ThenInclude(urs => urs.SkinCareRoutineStep)
                        .ThenInclude(scrs => scrs.SkincareRoutine)
                    .ToListAsync(stoppingToken);

                foreach (var userRoutine in userRoutines)
                {
                    var steps = userRoutine.UserRoutineSteps
                        .OrderBy(s => s.SkinCareRoutineStep.Step)
                        .ToList();

                    for (int i = 0; i < steps.Count; i++)
                    {
                        var step = steps[i];
                        var routineId = step.SkinCareRoutineStep?.SkincareRoutineId;
                        if (routineId == null) continue;

                        var orderIds = await unitOfWorks.OrderRepository
                            .FindByCondition(o =>
                                o.CustomerId == userRoutine.UserId &&
                                o.RoutineId == routineId &&
                                o.OrderType == OrderType.Routine.ToString())
                            .Select(o => o.OrderId)
                            .ToListAsync(stoppingToken);

                        if (!orderIds.Any()) continue;

                        var appointments = await unitOfWorks.AppointmentsRepository
                            .FindByCondition(a =>
                                a.OrderId.HasValue &&
                                orderIds.Contains(a.OrderId.Value) &&
                                a.Step.HasValue &&
                                a.Step.Value.Equals(step.SkinCareRoutineStep.Step))
                            .Include(a => a.Customer)
                            .Include(a => a.Service)
                            .ToListAsync(stoppingToken);

                        bool allCompleted = appointments.Any() && appointments.All(a => a.Status == OrderStatusEnum.Completed.ToString());

                        if (allCompleted && step.StepStatus != "Completed")
                        {
                            step.StepStatus = "Completed";
                            step.UpdatedDate = DateTime.UtcNow;
                            unitOfWorks.UserRoutineStepRepository.Update(step);
                            _logger.LogInformation($"✔ Step {step.UserRoutineStepId} updated to Completed.");
                        }

                        if (step.StepStatus == "Completed" && i + 1 < steps.Count)
                        {
                            var nextStep = steps[i + 1];
                            if (nextStep.StepStatus == "Completed") continue;

                            var nextAppointments = await unitOfWorks.AppointmentsRepository
                                .FindByCondition(a =>
                                    a.OrderId.HasValue &&
                                    orderIds.Contains(a.OrderId.Value) &&
                                    a.Step.HasValue &&
                                    a.Step.Value.Equals(nextStep.SkinCareRoutineStep.Step))
                                .Include(a => a.Customer)
                                .Include(a => a.Service)
                                .ToListAsync(stoppingToken);

                            _logger.LogInformation($"[DEBUG] Found {nextAppointments.Count} appointments for Step {nextStep.SkinCareRoutineStep.Step}");

                            foreach (var appointment in nextAppointments)
                            {
                                var appointmentDateUtc = DateTime.SpecifyKind(appointment.AppointmentsTime, DateTimeKind.Utc).Date;
                                var targetDate = DateTime.UtcNow.Date.AddDays(1);

                                _logger.LogInformation($"[CHECK] AppointmentId: {appointment.AppointmentId}, CustomerEmail: {appointment.Customer?.Email}, Step: {nextStep.SkinCareRoutineStep.Step}, AppointmentTime: {appointment.AppointmentsTime}");


                                if (appointmentDateUtc == targetDate)
                                {
                                    var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(
                                        appointment.AppointmentsTime,
                                        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                                    var mailData = new MailData
                                    {
                                        EmailToId = appointment.Customer.Email,
                                        EmailToName = appointment.Customer.FullName,
                                        EmailSubject = $"[Nhắc nhở] Bước tiếp theo trong liệu trình chăm sóc da",
                                        EmailBody = $@"
<div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
    <h2 style=""text-align: center; color: #3498db;"">Nhắc nhở bước tiếp theo trong liệu trình</h2>
    <p style=""font-size: 16px;"">Chào {appointment.Customer.FullName},</p>
    <p>Bạn đã hoàn thành bước {step.SkinCareRoutineStep.Step} trong liệu trình <strong>{step.SkinCareRoutineStep.SkincareRoutine?.Name}</strong>.</p>
    <p>Đây là nhắc nhở cho bước tiếp theo:</p>
    <ul style=""list-style-type: none;"">
        <li><strong>Tên liệu trình:</strong> {step.SkinCareRoutineStep.SkincareRoutine?.Name}</li>
        <li><strong>Số bước:</strong> {step.SkinCareRoutineStep.SkincareRoutine?.TotalSteps}</li>
        <li><strong>Bước tiếp theo:</strong> Bước {nextStep.SkinCareRoutineStep.Step}</li>
        <li><strong>Thời gian thực hiện:</strong> {vietnamTime:yyyy-MM-dd HH:mm}</li>
    </ul>
    <p>Vui lòng đến đúng giờ. Nếu có bất kỳ thắc mắc nào, xin liên hệ với chúng tôi.</p>
    <p style=""text-align:center; color: #aaa;"">Đội ngũ Solace Spa trân trọng!</p>
</div>"
                                    };


                                    var sent = await mailService.SendEmailAsync(mailData, false);
                                    if (sent)
                                        _logger.LogInformation($"📩 Email sent to {appointment.Customer.Email} for Step {nextStep.SkinCareRoutineStep.Step}.");
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
}

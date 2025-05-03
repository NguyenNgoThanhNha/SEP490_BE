using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Business.Services;
using Server.Business.Ultils;
using Server.Data.UnitOfWorks;

public class AppointmentReminderNoRealStaffWorker : BackgroundService
{
    private readonly ILogger<AppointmentReminderNoRealStaffWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AppointmentReminderNoRealStaffWorker(
        ILogger<AppointmentReminderNoRealStaffWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("⏰ Reminder worker started and waiting for 2:00 UTC daily schedule.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddHours(2); // 2:00 UTC hôm nay

            if (now > nextRun)
                nextRun = nextRun.AddDays(1); // nếu quá giờ -> đợi đến 2h UTC hôm sau

            var delay = nextRun - now;
            await Task.Delay(delay, stoppingToken);

            await RunAsync();
        }
    }

    public async Task RunAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWorks = scope.ServiceProvider.GetRequiredService<UnitOfWorks>();
        var mailService = scope.ServiceProvider.GetRequiredService<MailService>();

        var targetDate = DateTime.UtcNow.Date.AddDays(2);

        var appointments = await unitOfWorks.AppointmentsRepository
            .FindByCondition(x =>
                x.AppointmentsTime.Date == targetDate &&
                x.Staff != null &&
                x.Staff.RoleId == 3 &&
                x.Staff.StaffInfo.RoleID == 3)
            .Include(x => x.Customer)
            .Include(x => x.Staff).ThenInclude(s => s.StaffInfo)
            .ToListAsync();

        foreach (var appt in appointments)
        {
            if (appt.Customer == null || string.IsNullOrWhiteSpace(appt.Customer.Email))
                continue;

            var mailData = new MailData
            {
                EmailToId = appt.Customer.Email,
                EmailToName = appt.Customer.FullName,
                EmailSubject = "Nhắc nhở cập nhật nhân viên cho lịch hẹn",
                EmailBody = $@"
<p>Chào {appt.Customer.FullName},</p>
<p>Lịch hẹn của bạn vào <b>{appt.AppointmentsTime:dd/MM/yyyy HH:mm}</b> hiện đang được gán cho nhân viên hệ thống.</p>
<p>Vui lòng cập nhật nhân viên thực tế để đảm bảo trải nghiệm dịch vụ.</p>
<p>Trân trọng,</p>
<p>Đội ngũ Solace Spa</p>"
            };

            var sent = await mailService.SendEmailAsync(mailData, false);
            if (!sent)
                _logger.LogWarning($"❌ Gửi email thất bại cho: {appt.Customer.Email}");
        }

        _logger.LogInformation($"✅ Đã gửi nhắc nhở cho {appointments.Count} lịch hẹn có nhân viên hệ thống.");
    }

    // Dùng trong API
    public async Task ManualRunAsync() => await RunAsync();
}

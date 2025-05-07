using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Business.Services;
using Server.Business.Ultils;
using Server.Data.Entities;
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
            var nextRun = now.Date.AddHours(2);

            if (now > nextRun)
                nextRun = nextRun.AddDays(1);

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
        var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

        // Ngày gửi thông báo cho khách
        var dateForUser = DateTime.UtcNow.Date.AddDays(2);
        var userAppointments = await unitOfWorks.AppointmentsRepository
            .FindByCondition(x =>
                x.AppointmentsTime.Date == dateForUser &&
                x.Staff != null &&
                x.Staff.RoleId == 3 &&
                x.Staff.StaffInfo.RoleID == 3)
            .Include(x => x.Customer)
            .Include(x => x.Staff).ThenInclude(s => s.StaffInfo)
            .Include(x => x.Branch)
            .ToListAsync();

        foreach (var appt in userAppointments)
        {
            if (appt.Customer == null || string.IsNullOrWhiteSpace(appt.Customer.Email))
                continue;

            // Gửi Email
            var mailData = new MailData
            {
                EmailToId = appt.Customer.Email,
                EmailToName = appt.Customer.FullName,
                EmailSubject = "Nhắc nhở cập nhật nhân viên cho lịch hẹn",
                EmailBody = $@"
<p>Chào {appt.Customer.FullName},</p>
<p>Lịch hẹn của bạn vào <b>{appt.AppointmentsTime:dd/MM/yyyy HH:mm}</b> hiện đang được gán cho nhân viên hệ thống.</p>
<p>Vui lòng cập nhật nhân viên thực tế để đảm bảo trải nghiệm dịch vụ.</p>
<p>Trân trọng,</p><p>Đội ngũ Solace Spa</p>"
            };
            var sent = await mailService.SendEmailAsync(mailData, false);
            if (!sent)
                _logger.LogWarning($"❌ Gửi email KH thất bại: {appt.Customer.Email}");

            // Gửi SignalR notification
            var customerMongo = await mongoDbService.GetCustomerByIdAsync(appt.Customer.UserId);
            var notification = new Notifications
            {
                UserId = appt.Customer.UserId,
                Content = $"Lịch hẹn {appt.AppointmentsTime:dd/MM/yyyy HH:mm} của bạn chưa có nhân viên phục vụ cụ thể.",
                Type = "Reminder",
                isRead = false,
                ObjectId = appt.AppointmentId,
                CreatedDate = DateTime.UtcNow
            };

            await unitOfWorks.NotificationRepository.AddAsync(notification);
            await unitOfWorks.NotificationRepository.Commit();

            if (NotificationHub.TryGetConnectionId(customerMongo?.Id ?? "", out var connId))
            {
                await hubContext.Clients.Client(connId)
                    .SendAsync("receiveNotification", notification);
            }
        }

        // Ngày gửi email cho Manager
        var dateForManager = DateTime.UtcNow.Date.AddDays(1);
        var managerAppointments = await unitOfWorks.AppointmentsRepository
            .FindByCondition(x =>
                x.AppointmentsTime.Date == dateForManager &&
                x.Staff != null &&
                x.Staff.RoleId == 3 &&
                x.Staff.StaffInfo.RoleID == 3)
            .Include(x => x.Branch).ThenInclude(b => b.ManagerBranch)
            .Include(x => x.Staff)
            .ToListAsync();

        var groupedByManager = managerAppointments
            .Where(a => a.Branch?.ManagerBranch != null)
            .GroupBy(a => a.Branch.ManagerBranch);

        foreach (var group in groupedByManager)
        {
            var manager = group.Key;
            if (string.IsNullOrWhiteSpace(manager.Email)) continue;

            var mailBody = $@"
<p>Chào {manager.FullName},</p>
<p>Các lịch hẹn vào ngày <b>{dateForManager:dd/MM/yyyy}</b> vẫn đang được gán cho nhân viên hệ thống.</p>
<ul>{string.Join("", group.Select(a => $"<li>Lịch hẹn #{a.AppointmentId} lúc {a.AppointmentsTime:HH:mm}</li>"))}</ul>
<p>Vui lòng cập nhật nhân viên thực tế cho các lịch hẹn này.</p>
<p>Trân trọng,</p><p>Đội ngũ Solace Spa</p>";

            var mailData = new MailData
            {
                EmailToId = manager.Email,
                EmailToName = manager.FullName,
                EmailSubject = "Nhắc nhở cập nhật nhân viên cho lịch hẹn ngày mai",
                EmailBody = mailBody
            };

            var sent = await mailService.SendEmailAsync(mailData, false);
            if (!sent)
                _logger.LogWarning($"❌ Gửi email Manager thất bại: {manager.Email}");
        }

        _logger.LogInformation($"✅ Đã gửi nhắc nhở KH: {userAppointments.Count}, Quản lý: {groupedByManager.Count()}.");
    }

    public async Task ManualRunAsync() => await RunAsync();
}

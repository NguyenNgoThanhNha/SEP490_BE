﻿using Microsoft.AspNetCore.SignalR;
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
            var nextRun = now.Date.AddHours(2); // 2:00 UTC = 9:00 VN

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

        var vnNow = DateTime.UtcNow.AddHours(7);
        var dateForUser = vnNow.Date.AddDays(2);
        var dateForManager = vnNow.Date.AddDays(1);

        // 🔹 Nhắc khách hàng lịch hẹn sau 2 ngày nếu staff là hệ thống
        var userAppointments = await unitOfWorks.AppointmentsRepository
            .FindByCondition(x =>
                x.AppointmentsTime.Date == dateForUser &&
                x.Staff != null &&
                x.Staff.RoleId == 3 &&
                x.Staff.StaffInfo.RoleID == 4)
            .Include(x => x.Customer)
            .Include(x => x.Staff).ThenInclude(s => s.StaffInfo)
            .Include(x => x.Branch)
            .ToListAsync();

        foreach (var appt in userAppointments)
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
<p>Trân trọng,<br/>Đội ngũ Solace Spa</p>"
            };

            var sent = await mailService.SendEmailAsync(mailData, false);
            if (!sent)
                _logger.LogWarning($"❌ Gửi email KH thất bại: {appt.Customer.Email}");

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

        // 🔹 Nhắc quản lý nếu lịch hẹn ngày mai có nhân viên hệ thống
        var managerAppointments = await unitOfWorks.AppointmentsRepository
            .FindByCondition(x =>
                x.AppointmentsTime.Date == dateForManager &&
                x.Staff != null &&
                x.Staff.RoleId == 3 &&
                x.Staff.StaffInfo.RoleID == 4)
            .Include(x => x.Branch)
            .Include(x => x.Staff)
            .ToListAsync();

        var groupedByManagerId = managerAppointments
            .Where(a => a.Branch != null)
            .GroupBy(a => a.Branch.ManagerId);

        foreach (var group in groupedByManagerId)
        {
            var managerId = group.Key;
            var manager = await unitOfWorks.UserRepository
                .FindByCondition(u => u.UserId == managerId && u.RoleID == 2)
                .FirstOrDefaultAsync();

            if (manager == null || string.IsNullOrWhiteSpace(manager.Email))
            {
                _logger.LogWarning($"⚠️ Không tìm thấy hoặc thiếu email cho ManagerId = {managerId}");
                continue;
            }

            var mailBody = $@"
<p>Chào {manager.FullName},</p>
<p>Các lịch hẹn vào ngày <b>{dateForManager:dd/MM/yyyy}</b> vẫn đang được gán cho nhân viên hệ thống.</p>
<ul>{string.Join("", group.Select(a => $"<li>Lịch hẹn #{a.AppointmentId} lúc {a.AppointmentsTime:HH:mm}</li>"))}</ul>
<p>Vui lòng cập nhật nhân viên thực tế cho các lịch hẹn này.</p>
<p>Trân trọng,<br/>Đội ngũ Solace Spa</p>";

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

        _logger.LogInformation($"✅ Đã gửi nhắc nhở KH: {userAppointments.Count}, Quản lý: {groupedByManagerId.Count()}.");
    }

    public async Task ManualRunAsync() => await RunAsync();
}

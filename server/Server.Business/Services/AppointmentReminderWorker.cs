using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Server.Data.UnitOfWorks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Business.Ultils;

namespace Server.Business.Services
{
    public class AppointmentReminderWorker : BackgroundService
    {
        private readonly ILogger<AppointmentReminderWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AppointmentReminderWorker(ILogger<AppointmentReminderWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Reminder Worker is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (true) // Chạy ngay lập tức, không phụ thuộc vào giờ
                {
                    _logger.LogInformation("Sending appointment reminders (Test Mode)...");
                    await SendAppointmentRemindersAsync();
                }


                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task SendAppointmentRemindersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWorks>();
            var mailService = scope.ServiceProvider.GetRequiredService<MailService>();

            var tomorrow = DateTime.Now.Date.AddDays(1);

            // Truy vấn lấy các Appointments có trạng thái Confirmed và ngày hẹn là ngày mai
            var appointments = await unitOfWork.AppointmentsRepository
                .FindByCondition(a => a.AppointmentsTime.Date == tomorrow && a.Status == "Confirmed")
                .Include(a => a.Customer)
                .Include(a => a.Service)
                .ToListAsync();

            _logger.LogInformation($"Found {appointments.Count} confirmed appointments for tomorrow.");

            foreach (var appointment in appointments)
            {
                var mailData = new MailData
                {
                    EmailToId = appointment.Customer.Email,
                    EmailToName = appointment.Customer.FullName,
                    EmailSubject = "Appointment Reminder",
                    EmailBody = $@"
<p>Dear {appointment.Customer.FullName},</p>
<p>This is a reminder for your upcoming appointment:</p>
<ul>
    <li>Service: {appointment.Service.Name}</li>
    <li>Date: {appointment.AppointmentsTime:yyyy-MM-dd}</li>
    <li>Time: {appointment.AppointmentsTime:HH:mm}</li>
</ul>
<p>Please arrive on time. Thank you!</p>"
                };

                var result = await mailService.SendEmailAsync(mailData, false);
                if (result)
                {
                    _logger.LogInformation($"Email sent to {appointment.Customer.Email} for Appointment ID: {appointment.AppointmentsId}.");
                }
                else
                {
                    _logger.LogError($"Failed to send email to {appointment.Customer.Email} for Appointment ID: {appointment.AppointmentsId}.");
                }
            }
        }

    }
}

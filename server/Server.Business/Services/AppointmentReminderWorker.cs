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

            // Kiểm tra khi nào đến 8:00 sáng và chạy công việc
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                if (now.Hour == 8 && now.Minute == 0)
                {
                    _logger.LogInformation("Sending appointment reminders at 8:00 AM...");
                    await SendAppointmentRemindersAsync();

                    _logger.LogInformation("Appointment reminders have been sent. Worker stopping...");

                    break; // Dừng Worker sau khi hoàn thành công việc
                }

                // Chờ 30 giây trước khi kiểm tra lại thời gian
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
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
<div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
    <h2 style=""text-align: center; color: #3498db; font-weight: bold;"">Appointment Reminder</h2>
    <p style=""font-size: 16px; color: #555;"">Dear {appointment.Customer.FullName},</p>
    <p style=""font-size: 16px; color: #555;"">This is a friendly reminder for your upcoming appointment:</p>
    <ul style=""font-size: 16px; color: #555; list-style-type: none; padding: 0;"">
        <li><strong>Service:</strong> {appointment.Service.Name}</li>
        <li><strong>Date:</strong> {appointment.AppointmentsTime:yyyy-MM-dd}</li>
        <li><strong>Time:</strong> {appointment.AppointmentsTime:HH:mm}</li>
    </ul>
    <p style=""font-size: 16px; color: #555;"">Please arrive on time. If you have any questions, feel free to contact us.</p>
    <p style=""text-align: center; color: #888; font-size: 14px;"">Thank you for choosing our services!</p>
    <p style=""text-align: center; color: #888; font-size: 14px;"">Powered by Team Solace</p>
</div>"
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

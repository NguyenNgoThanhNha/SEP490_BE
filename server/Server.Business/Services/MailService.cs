using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Server.Business.Ultils;

namespace Server.Business.Services;

    public class MailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettingsOptions)
        {
            _mailSettings = mailSettingsOptions.Value;
        }

        public async Task<bool> SendEmailAsync(MailData mailData, bool isCost)
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
                emailMessage.To.Add(new MailboxAddress(mailData.EmailToName, mailData.EmailToId));
                emailMessage.Subject = mailData.EmailSubject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = mailData.EmailBody
                };

                if (isCost)
                {
                    // Thêm file đính kèm từ URL
                    var urls = new List<string>
                    {
                        "https://res.cloudinary.com/dgezepi0f/image/upload/v1728304449/mxvewtx7xj03lrysvany.png",
                        "https://res.cloudinary.com/dgezepi0f/image/upload/v1728304438/c8soj0baqqhzs33odl2a.png"
                    };

                    using (var httpClient = new HttpClient())
                    {
                        foreach (var url in urls)
                        {
                            var fileBytes = await httpClient.GetByteArrayAsync(url);
                            var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
                    
                            // Đính kèm file từ stream
                            bodyBuilder.Attachments.Add(fileName, fileBytes);
                        }
                    }
                }

                
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_mailSettings.Server, int.Parse(_mailSettings.Port), MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as per your application's requirements
                return false;
            }
        }
    }
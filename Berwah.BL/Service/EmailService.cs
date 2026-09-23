using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
using Microsoft.Extensions.Options;

namespace Berwah.BL.Service
{
    public class EmailService: IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            // الاتصال بالسيرفر عبر TLS
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);

            // تسجيل الدخول
            await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);

            // الإرسال
            await smtp.SendAsync(email);

            // قطع الاتصال
            await smtp.DisconnectAsync(true);
        }
    }
}

using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Options;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


namespace DigitalBank.Persistence.Services
{
    public class EmailSender:IEmailSender
    {
        private readonly SmtpOptions _smtp;

        public EmailSender(IOptions<SmtpOptions> smtpOptions)
        {
            _smtp = smtpOptions.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var body = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = body.ToMessageBody();

            using var client = new SmtpClient();

            var secure = _smtp.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(_smtp.Host, _smtp.Port, secure);
            await client.AuthenticateAsync(_smtp.UserName, _smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}


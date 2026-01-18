using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;


namespace DigitalBank.Persistence.Services
{
    //public class EmailSender:IEmailSender
    //{
    //    private readonly SmtpOptions _smtp;

    //    public EmailSender(IOptions<SmtpOptions> smtpOptions)
    //    {
    //        _smtp = smtpOptions.Value;
    //    }

    //    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    //    {
    //        var message = new MimeMessage();
    //        message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
    //        message.To.Add(MailboxAddress.Parse(toEmail));
    //        message.Subject = subject;

    //        var body = new BodyBuilder { HtmlBody = htmlBody };
    //        message.Body = body.ToMessageBody();

    //        using var client = new SmtpClient();

    //        var secure = _smtp.UseSsl
    //? SecureSocketOptions.SslOnConnect
    //: SecureSocketOptions.StartTls;

    //        await client.ConnectAsync(_smtp.Host, _smtp.Port, secure);
    //        client.AuthenticationMechanisms.Remove("XOAUTH2");
    //        await client.AuthenticateAsync(_smtp.UserName, _smtp.Password);
    //        await client.SendAsync(message);
    //        await client.DisconnectAsync(true);
    //    }
    //}

    public class EmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtp;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<EmailSender> logger)
        {
            _smtp = smtpOptions.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                using var client = new SmtpClient();

                var secure = _smtp.UseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await client.ConnectAsync(_smtp.Host, _smtp.Port, secure);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.AuthenticateAsync(_smtp.UserName, _smtp.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SMTP send failed. Host={Host} Port={Port} From={From} To={To}",
                    _smtp.Host, _smtp.Port, _smtp.FromEmail, toEmail);

                throw; // AuthService catch görsün
            }
        }
    }

}


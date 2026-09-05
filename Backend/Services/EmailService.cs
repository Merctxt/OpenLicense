using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Options;

namespace OpenLicenseApi.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string token);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
        {
            _settings = options.Value;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_settings.From));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Password Recovery - OpenLicense";

            var builder = new BodyBuilder();
            builder.HtmlBody = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                    <h2 style=""color: #333;"">Password Recovery</h2>
                    <p>You requested a password recovery for your OpenLicense account.</p>
                    <p>Your recovery token is:</p>
                    <div style=""background-color: #f4f4f4; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; border-radius: 5px; margin: 20px 0;"">
                        {token}
                    </div>
                    <p style=""color: #666; font-size: 12px;"">This token expires in 15 minutes.</p>
                    <p style=""color: #666; font-size: 12px;"">If you did not request this recovery, please ignore this email.</p>
                </div>
            ";
            builder.TextBody = $@"
                Password Recovery - OpenLicense

                You requested a password recovery for your OpenLicense account.

                Your recovery token is: {token}

                This token expires in 15 minutes.
                If you did not request this recovery, please ignore this email.
            ";

            email.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            var socketOptions = _settings.Secure
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            try
            {
                await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool Secure { get; set; } = false;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
    }
}

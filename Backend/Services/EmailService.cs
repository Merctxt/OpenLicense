using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Options;
using OpenLicenseApi.DTOs;

namespace OpenLicenseApi.Services
{
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

        public async Task SendReportEmailAsync(string toEmail, string subject, ReportDataDto report)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_settings.From));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder();

            var summariesHtml = string.Join("\n", report.LicenseSummaries.Select(s => $@"
                <tr>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{s.ProductName}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{s.TotalLicenses} / {s.MaxLicenses}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{s.ActiveLicenses}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{s.SuspendedLicenses}</td>
                </tr>"));

            var expiringHtml = string.Join("\n", report.ExpiringLicenses.Select(l => $@"
                <tr>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.LicenseName}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.ProductName}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.LicenseKey}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.ExpiresAt.ToString("MMM dd, yyyy")}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.DaysRemaining} day(s)</td>
                </tr>"));

            var expiredHtml = string.Join("\n", report.ExpiredLicenses.Select(l => $@"
                <tr>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.LicenseName}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.ProductName}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.LicenseKey}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.ExpiresAt.ToString("MMM dd, yyyy")}</td>
                    <td style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">{l.DaysExpired} day(s)</td>
                </tr>"));

            builder.HtmlBody = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto;"">
                    <h2 style=""color: #333;"">License Report</h2>
                    <p>Hi {report.UserName},</p>
                    <p>Here is your license status report generated at {report.GeneratedAt.ToString("MMM dd, yyyy HH:mm UTC")}.</p>

                    <h3 style=""color: #333;"">Summary</h3>
                    <div style=""background-color: #f9f9f9; padding: 12px; margin-bottom: 20px;"">
                        <div style=""margin-bottom: 8px;""><strong>Total Licenses:</strong> {report.TotalLicenses}</div>
                        <div style=""margin-bottom: 8px;""><strong>Active Licenses:</strong> {report.ActiveLicenses}</div>
                        <div style=""margin-bottom: 8px;""><strong>Suspended Licenses:</strong> {report.SuspendedLicenses}</div>
                        <div style=""margin-bottom: 8px;""><strong>Expiring Soon (7 days):</strong> {report.ExpiringCount}</div>
                        <div style=""margin-bottom: 8px;""><strong>Expired:</strong> {report.ExpiredCount}</div>
                    </div>

                    <h3 style=""color: #333;"">Licenses by Product</h3>
                    <table style=""width: 100%; border-collapse: collapse; margin-bottom: 20px;"">
                        <thead>
                            <tr style=""background-color: #f0f0f0;"">
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Product</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Licenses</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Active</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Suspended</th>
                            </tr>
                        </thead>
                        <tbody>
                            {summariesHtml}
                        </tbody>
                    </table>
";

            if (report.ExpiringLicenses.Count > 0)
            {
                builder.HtmlBody += $@"
                    <h3 style=""color: #333;"">Expiring Within 7 Days</h3>
                    <table style=""width: 100%; border-collapse: collapse; margin-bottom: 20px;"">
                        <thead>
                            <tr style=""background-color: #fff3cd;"">
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Name</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Product</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Key</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Expires</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Days Left</th>
                            </tr>
                        </thead>
                        <tbody>
                            {expiringHtml}
                        </tbody>
                    </table>
";
            }

            if (report.ExpiredLicenses.Count > 0)
            {
                builder.HtmlBody += $@"
                    <h3 style=""color: #333;"">Expired Licenses</h3>
                    <table style=""width: 100%; border-collapse: collapse; margin-bottom: 20px;"">
                        <thead>
                            <tr style=""background-color: #f8d7da;"">
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Name</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Product</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Key</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Expired</th>
                                <th style=""padding: 8px; border: 1px solid #ddd; text-align: left;"">Days Overdue</th>
                            </tr>
                        </thead>
                        <tbody>
                            {expiredHtml}
                        </tbody>
                    </table>
";
            }

            builder.TextBody = $@"
License Report
==============

Hi {report.UserName},

Here is your license status report generated at {report.GeneratedAt.ToString("MMM dd, yyyy HH:mm UTC")}.

SUMMARY
-------
Total Licenses: {report.TotalLicenses}
Active Licenses: {report.ActiveLicenses}
Suspended Licenses: {report.SuspendedLicenses}
Expiring Soon (7 days): {report.ExpiringCount}
Expired: {report.ExpiredCount}

LICENSES BY PRODUCT
-------------------
{string.Join("\n", report.LicenseSummaries.Select(s => $"Product: {s.ProductName}\nLicenses: {s.TotalLicenses} / {s.MaxLicenses}\nActive: {s.ActiveLicenses}\nSuspended: {s.SuspendedLicenses}\n"))}
";

            if (report.ExpiringLicenses.Count > 0)
            {
                builder.TextBody += $@"
EXPIRING WITHIN 7 DAYS
----------------------
{string.Join("\n", report.ExpiringLicenses.Select(l => $"Name: {l.LicenseName}\nProduct: {l.ProductName}\nKey: {l.LicenseKey}\nExpires: {l.ExpiresAt.ToString("MMM dd, yyyy")}\nDays Left: {l.DaysRemaining}\n"))}
";
            }

            if (report.ExpiredLicenses.Count > 0)
            {
                builder.TextBody += $@"
EXPIRED LICENSES
----------------
{string.Join("\n", report.ExpiredLicenses.Select(l => $"Name: {l.LicenseName}\nProduct: {l.ProductName}\nKey: {l.LicenseKey}\nExpired: {l.ExpiresAt.ToString("MMM dd, yyyy")}\nDays Overdue: {l.DaysExpired}\n"))}
";
            }

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

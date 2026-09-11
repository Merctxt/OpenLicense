using OpenLicenseApi.DTOs;

namespace OpenLicenseApi.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string token);
        Task SendReportEmailAsync(string toEmail, string subject, ReportDataDto report);
    }
}

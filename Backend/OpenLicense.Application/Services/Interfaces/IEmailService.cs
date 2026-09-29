using OpenLicense.Application.DTOs;

namespace OpenLicense.Application.Services.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string token);
    Task SendReportEmailAsync(string toEmail, string subject, ReportDataDto report);
}

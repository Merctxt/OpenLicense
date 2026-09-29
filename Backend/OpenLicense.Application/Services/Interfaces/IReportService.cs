using OpenLicense.Application.DTOs;

namespace OpenLicense.Application.Services.Interfaces;

public interface IReportService
{
    Task<ReportDataDto> GenerateReportForUserAsync(Guid userId, int expiringDaysWarning);
}

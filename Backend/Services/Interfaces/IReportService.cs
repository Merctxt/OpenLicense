using OpenLicenseApi.DTOs;

namespace OpenLicenseApi.Services
{
    public interface IReportService
    {
        Task<ReportDataDto> GenerateReportForUserAsync(Guid userId, int expiringDaysWarning);
    }
}

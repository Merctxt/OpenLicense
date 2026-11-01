using OpenLicense.Application.DTOs;

namespace OpenLicense.Application.Services.Interfaces;

public interface IPdfReportService
{
    Task<byte[]> GenerateReportPdfAsync(ReportDataDto report);
}

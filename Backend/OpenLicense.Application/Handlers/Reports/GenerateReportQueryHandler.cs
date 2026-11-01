using OpenLicense.Application.Queries.Reports;
using OpenLicense.Application.Services.Interfaces;
using MediatR;

namespace OpenLicense.Application.Handlers.Reports;

public class GenerateReportQueryHandler : IRequestHandler<GenerateReportQuery, byte[]>
{
    private readonly IReportService _reportService;
    private readonly IPdfReportService _pdfService;

    public GenerateReportQueryHandler(IReportService reportService, IPdfReportService pdfService)
    {
        _reportService = reportService;
        _pdfService = pdfService;
    }

    public async Task<byte[]> Handle(GenerateReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportService.GenerateReportForUserAsync(request.UserId, request.ExpiringDaysWarning);
        return await _pdfService.GenerateReportPdfAsync(report);
    }
}

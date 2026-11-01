using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenLicense.Api;
using OpenLicense.Application.Services.Interfaces;

namespace OpenLicense.Api.Controllers;

[ApiController]
[ApiVersion(ApiVersion.Current)]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/v{version:apiVersion}/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IPdfReportService _pdfService;

    public ReportsController(IReportService reportService, IPdfReportService pdfService)
    {
        _reportService = reportService;
        _pdfService = pdfService;
    }

    [Authorize]
    [HttpGet("generate")]
    public async Task<IActionResult> GenerateReport([FromQuery] int? expiringDaysWarning = 7)
    {
        var userId = User.GetUserId();
        var report = await _reportService.GenerateReportForUserAsync(userId, expiringDaysWarning ?? 7);
        var pdfBytes = await _pdfService.GenerateReportPdfAsync(report);

        var fileName = $"OpenLicense_Report_{DateTime.UtcNow:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenLicense.Api;
using OpenLicense.Application.Queries.Reports;

namespace OpenLicense.Api.Controllers;

[ApiController]
[ApiVersion(ApiVersion.Current)]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/v{version:apiVersion}/reports")]
public class ReportsController : ControllerBase
{
    private readonly ISender _mediator;

    public ReportsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("generate")]
    public async Task<IActionResult> Generate([FromQuery] int? expiringDaysWarning = 7)
    {
        var userId = User.GetUserId();
        var pdfBytes = await _mediator.Send(new GenerateReportQuery(userId, expiringDaysWarning ?? 7));

        var fileName = $"OpenLicense_Report_{DateTime.UtcNow:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}

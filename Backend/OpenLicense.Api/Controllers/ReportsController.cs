using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;

    public ReportsController(ISender mediator, IMemoryCache cache)
    {
        _mediator = mediator;
        _cache = cache;
    }

    [Authorize]
    [HttpGet("generate")]
    public async Task<IActionResult> Generate([FromQuery] int? expiringDaysWarning = 7)
    {
        var userId = User.GetUserId();
        var cacheKey = $"report:{userId}:{expiringDaysWarning ?? 7}";

        if (_cache.TryGetValue(cacheKey, out byte[]? cachedPdf))
        {
            var fileName = $"OpenLicense_Report_{DateTime.UtcNow:yyyy-MM-dd}.pdf";
            return File(cachedPdf, "application/pdf", fileName);
        }

        var pdfBytes = await _mediator.Send(new GenerateReportQuery(userId, expiringDaysWarning ?? 7));

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetPriority(CacheItemPriority.High);

        _cache.Set(cacheKey, pdfBytes, cacheEntryOptions);

        var fileNameNew = $"OpenLicense_Report_{DateTime.UtcNow:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileNameNew);
    }
}

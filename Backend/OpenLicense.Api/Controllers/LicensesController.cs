using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using OpenLicense.Api;
using OpenLicense.Application.Commands.Licenses;
using OpenLicense.Application.DTOs;
using OpenLicense.Application.Queries.Licenses;
using OpenLicense.Infrastructure.Auth;

namespace OpenLicense.Api.Controllers;

[ApiController]
[ApiVersion(ApiVersion.Current)]
[Route("api/v{version:apiVersion}/licenses")]
public class LicensesController : ControllerBase
{
    private readonly ISender _mediator;

    public LicensesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet]
    public async Task<IActionResult> GetLicensesByProductId([FromQuery] Guid? productId)
    {
        var userId = User.GetUserId();

        if (!productId.HasValue)
        {
            return BadRequest(new { message = "ProductId is required." });
        }

        var query = new GetLicensesByProductIdQuery(userId, productId.Value);
        var licenses = await _mediator.Send(query);
        return Ok(licenses);
    }

    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost]
    public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseRequest request)
    {
        var userId = User.GetUserId();

        if (request.ProductId == Guid.Empty)
        {
            return BadRequest(new { message = "ProductId is required." });
        }

        var command = new CreateLicenseCommand(userId, request.ProductId, request.Name, request.ExpiresAt, request.MaxActivations);
        var license = await _mediator.Send(command);
        return Ok(license);
    }

    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPut]
    public async Task<IActionResult> UpdateLicense([FromBody] UpdateLicenseRequest request)
    {
        var userId = User.GetUserId();

        if (request.LicenseId == Guid.Empty)
        {
            return BadRequest(new { message = "LicenseId is required." });
        }

        var command = new UpdateLicenseCommand(userId, request.LicenseId, request.Name, request.ExpiresAt, request.MaxActivations, request.Status);
        var license = await _mediator.Send(command);
        return Ok(license);
    }

    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete]
    public async Task<IActionResult> DeleteLicense([FromBody] DeleteLicenseRequest request)
    {
        var userId = User.GetUserId();

        if (request.LicenseId == Guid.Empty)
        {
            return BadRequest(new { message = "LicenseId is required." });
        }

        var command = new DeleteLicenseCommand(userId, request.LicenseId);
        await _mediator.Send(command);
        return Ok(new { message = "License deleted successfully." });
    }

    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("activations")]
    public async Task<IActionResult> GetLicenseActivations([FromQuery] Guid licenseId)
    {
        var userId = User.GetUserId();
        var query = new GetLicenseActivationsQuery(userId, licenseId);
        var activations = await _mediator.Send(query);
        return Ok(activations);
    }

    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateLicense([FromBody] ValidateLicenseRequest request)
    {
        var userId = User.GetUserId();
        var command = new ValidateLicenseCommand(userId, request.LicenseKey, request.HardwareId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
    [HttpPost("deactivate")]
    public async Task<IActionResult> DeactivateLicense([FromBody] DeactivateLicenseRequest request)
    {
        var userId = User.GetUserId();
        var command = new DeactivateLicenseCommand(userId, null, request.LicenseKey, request.HardwareId);
        await _mediator.Send(command);
        return Ok(new { message = "License deactivated successfully." });
    }

    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("deactivate-by-jwt")]
    public async Task<IActionResult> DeactivateLicenseByJwt([FromBody] DeactivateLicenseRequest request)
    {
        var userId = User.GetUserId();
        var command = new DeactivateLicenseCommand(userId, null, request.LicenseKey, request.HardwareId);
        await _mediator.Send(command);
        return Ok(new { message = "License deactivated successfully." });
    }

    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPut("activations/toggle")]
    public async Task<IActionResult> ToggleActivation([FromBody] ToggleActivationRequest request)
    {
        var userId = User.GetUserId();
        var command = new ToggleActivationCommand(userId, request.LicenseId, request.HardwareId);
        var isActive = await _mediator.Send(command);
        return Ok(new { isActive });
    }
}

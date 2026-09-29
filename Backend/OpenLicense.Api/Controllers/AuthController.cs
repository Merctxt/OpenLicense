using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using OpenLicense.Api;
using OpenLicense.Application.DTOs;
using OpenLicense.Application.Queries.Auth;
using OpenLicense.Application.Commands.Auth;

namespace OpenLicense.Api.Controllers;

[ApiController]
[ApiVersion(ApiVersion.Current)]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(ISender mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        bool registrationEnabled = _configuration.GetValue<bool>("RegistrationEnabled", true);
        if (!registrationEnabled)
        {
            return Forbid();
        }
        var command = new RegisterUserCommand(request.Name, request.Email, request.Password);
        var user = await _mediator.Send(command);
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var token = await _mediator.Send(command);
        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(30)
        });
        return Ok(new { token });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return Ok(new { ok = true });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var query = new GetMeQuery(User.GetUserId());
        var user = await _mediator.Send(query);
        return Ok(user);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRequest request)
    {
        var command = new UpdateUserCommand(User.GetUserId(), request.Name, request.Email, request.Password);
        var updatedUser = await _mediator.Send(command);
        return Ok(updatedUser);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var command = new DeleteUserCommand(User.GetUserId());
        await _mediator.Send(command);
        return NoContent();
    }

    [Authorize]
    [HttpPost("apikey")]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        var command = new CreateApiKeyCommand(User.GetUserId(), request.Name);
        var apiKey = await _mediator.Send(command);
        return Ok(apiKey);
    }

    [Authorize]
    [HttpDelete("apikey")]
    public async Task<IActionResult> DeleteApiKey([FromBody] DeleteApiKeyRequest request)
    {
        var command = new DeleteApiKeyCommand(User.GetUserId(), request.ApiKeyId);
        await _mediator.Send(command);
        return NoContent();
    }

    [Authorize]
    [HttpPut("apikey/toggle")]
    public async Task<IActionResult> ToggleApiKey([FromBody] ToggleApiKeyRequest request)
    {
        var command = new ToggleApiKeyCommand(User.GetUserId(), request.ApiKeyId);
        var isActive = await _mediator.Send(command);
        return Ok(new { isActive });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        await _mediator.Send(command);
        return Ok(new { message = "If the email exists, a recovery token has been sent." });
    }

    [HttpPost("reset-password/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyResetToken([FromBody] VerifyTokenRequest request)
    {
        var command = new VerifyResetTokenCommand(request.Email, request.Token);
        var isValid = await _mediator.Send(command);
        if (!isValid)
        {
            return BadRequest(new { message = "Invalid or expired token." });
        }
        return Ok(new { valid = true });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.Password);
        await _mediator.Send(command);
        return Ok(new { message = "Password reset successfully." });
    }

    [Authorize]
    [HttpPut("report-preferences")]
    public async Task<IActionResult> UpdateReportPreferences([FromBody] UpdateReportPreferencesRequest request)
    {
        var command = new UpdateReportPreferencesCommand(User.GetUserId(), request.ReportsOptIn);
        var user = await _mediator.Send(command);
        return Ok(new { reportsOptIn = user.ReportsOptIn });
    }
}

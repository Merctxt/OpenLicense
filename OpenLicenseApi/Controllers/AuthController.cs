using Microsoft.AspNetCore.Mvc;
using OpenLicenseApi.Services;
using OpenLicenseApi.DTOs;
using OpenLicenseApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OpenLicenseApi.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = await _authService.RegisterAsync(request.Name, request.Email, request.Password);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.LoginAsync(request.Email, request.Password);
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
            var userId = GetUserId();
            var user = await _authService.GetMeAsync(userId);
            return Ok(user);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRequest request)
        {
            var userId = GetUserId();
            var updatedUser = await _authService.UpdateAsync(userId, request.Name, request.Email, request.Password);
            return Ok(updatedUser);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var userId = GetUserId();
            await _authService.DeleteAsync(userId);
            return NoContent();
        }

        [Authorize]
        [HttpPost("apikey")]
        public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
        {
            var userId = GetUserId();
            var apiKey = await _authService.CreateApiKeyAsync(userId, request);
            return Ok(apiKey);
        }

        [Authorize]
        [HttpDelete("apikey")]
        public async Task<IActionResult> DeleteApiKey([FromBody] DeleteApiKeyRequest request)
        {
            var userId = GetUserId();
            await _authService.DeleteApiKeyAsync(userId, request.ApiKeyId);
            return NoContent();
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request.Email);
            return Ok(new { message = "If the email exists, a recovery token has been sent." });
        }

        [HttpPost("reset-password/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetToken([FromBody] VerifyTokenRequest request)
        {
            var isValid = await _authService.VerifyResetTokenAsync(request.Email, request.Token);
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
            await _authService.ResetPasswordAsync(request.Email, request.Token, request.Password);
            return Ok(new { message = "Password reset successfully." });
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid token user id.");
            }
            return userId;
        }
    }
}

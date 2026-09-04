using Microsoft.AspNetCore.Mvc;
using OpenLicenseApi.Services;
using OpenLicenseApi.DTOs;
using OpenLicenseApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OpenLicenseApi.Middleware;

namespace OpenLicenseApi.Controllers
{
    [ApiController]
    [Route("api/licenses")]
    public class LicensesController : ControllerBase
    {
        private readonly ILicenseService _licenseService;

        public LicensesController(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public async Task<IActionResult> GetLicensesByProductId([FromQuery] Guid? productId)
        {
            var userId = GetUserId();

            if (!productId.HasValue)
            {
                return BadRequest(new { message = "ProductId is required." });
            }

            var licenses = await _licenseService.GetLicensesByProductIdAsync(userId, productId.Value);
            return Ok(licenses);
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseRequest request)
        {
            var userId = GetUserId();

            if (request.ProductId == Guid.Empty)
            {
                return BadRequest(new { message = "ProductId is required." });
            }

            var license = await _licenseService.CreateLicenseAsync(userId, request.ProductId, request);
            return Ok(license);
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPut]
        public async Task<IActionResult> UpdateLicense([FromBody] UpdateLicenseRequest request)
        {
            var userId = GetUserId();

            if (request.LicenseId == Guid.Empty)
            {
                return BadRequest(new { message = "LicenseId is required." });
            }

            var license = await _licenseService.UpdateLicenseAsync(userId, request.LicenseId, request);
            return Ok(license);
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete]
        public async Task<IActionResult> DeleteLicense([FromBody] DeleteLicenseRequest request)
        {
            var userId = GetUserId();

            if (request.LicenseId == Guid.Empty)
            {
                return BadRequest(new { message = "LicenseId is required." });
            }

            await _licenseService.DeleteLicenseAsync(userId, request.LicenseId);
            return Ok(new { message = "License deleted successfully." });
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("activations")]
        public async Task<IActionResult> GetLicenseActivations([FromQuery] Guid licenseId)
        {
            var userId = GetUserId();
            var activations = await _licenseService.GetLicenseActivationsAsync(userId, licenseId);
            return Ok(activations);
        }

        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateLicense([FromBody] ValidateLicenseRequest request)
        {
            var userId = GetUserId();
            var result = await _licenseService.ValidateLicenseAsync(userId, request);
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
        [HttpPost("deactivate")]
        public async Task<IActionResult> DeactivateLicense([FromBody] DeactivateLicenseRequest request)
        {
            var userId = GetUserId();
            await _licenseService.DeactivateLicenseAsync(userId, null, request);
            return Ok(new { message = "License deactivated successfully." });
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("deactivate-by-jwt")]
        public async Task<IActionResult> DeactivateLicenseByJwt([FromBody] DeactivateLicenseRequest request)
        {
            var userId = GetUserId();
            await _licenseService.DeactivateLicenseAsync(userId, null, request);
            return Ok(new { message = "License deactivated successfully." });
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

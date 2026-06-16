/* 
POST   /api/auth/register <- registrar um novo usuário, deve receber nome, email e senha. Retorna os dados do usuário criado ou um erro se o email já estiver em uso.
POST   /api/auth/login <- autenticar um usuário, deve receber email e senha. Retorna um JWT se as credenciais forem válidas ou um erro se forem inválidas.

GET    /api/auth/me <- retorna os dados do usuário autenticado.

PUT    /api/auth <- atualizar os dados do usuário autenticado, deve receber nome, email e/ou senha. Retorna os dados atualizados do usuário ou um erro se o email já estiver em uso por outro usuário.
DELETE /api/auth <- deletar o usuário autenticado.

POST   /api/auth/apikey <- criar uma nova chave de API para o usuário autenticado.
DELETE /api/auth/apikey <- deletar uma chave de API do usuário autenticado.

JWT do usuário do dashboard.
*/

using Microsoft.AspNetCore.Mvc;
using OpenLicenseApi.Services;
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
            try
            {
                var user = await _authService.RegisterAsync(request.Name, request.Email, request.Password);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request.Email, request.Password);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                var user = await _authService.GetMeAsync(userId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                var updatedUser = await _authService.UpdateAsync(userId, request.Name, request.Email, request.Password);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                await _authService.DeleteAsync(userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("apikey")]
        public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                var apiKey = await _authService.CreateApiKeyAsync(userId, request);
                return Ok(apiKey);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("apikey")]
        public async Task<IActionResult> DeleteApiKey([FromBody] DeleteApiKeyRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                await _authService.DeleteApiKeyAsync(userId, request.ApiKeyId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
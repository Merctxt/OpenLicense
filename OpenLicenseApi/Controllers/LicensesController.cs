/* 
GET    /api/licenses <- lista as licenças do produto do usuário autenticado (com chave de api)

POST   /api/licenses <- criar uma licença para um produto do usuário autenticado (com chave de api)

PUT    /api/licenses <- atualizar os dados da licença (incluindo status bool: true=active, false=suspended), só se a licença pertencer a um produto do usuário autenticado (com chave de api)

DELETE /api/licenses <- deletar a licença, só se a licença pertencer a um produto do usuário autenticado (com chave de api)

-------------

POST /api/licenses/validate <- validar se a licença é válida, ativa e pertence ao produto, deve receber a chave de api do produto o código da licença e o hardware id do cliente. 
Esse endpoint será usado na implementação principal do software do cliente, deve validar se:
ApiKey existe?, ApiKey ativa? Não? Retornar erro de chave de api inválida ou inativa.
Licença existe?, Pertence ao produto da ApiKey? Não? Retornar erro de licença inválida.
Licença ativa? Não? Retornar erro de licença inativa.
Existe ativação com esse HardwareId? Sim, Atualiza LastSeen, Retorna válido.
Conta quantas ativações existem. Se for menor que o limite de ativações da licença, Cria nova ativação com esse HardwareId, Retorna válido.
Se for igual ou maior que o limite de ativações da licença, Retorna erro de limite.

POST /api/licenses/deactivate <- desativar uma licença, remove a ativação, deve receber a chave de api do produto ou jwt, o código da licença e o hardware id do cliente.

Cada licença pertence a um produto.
Padrão da licença: 45AH-4HJY-97MR-2O80 -> 4 blocos de 4 caracteres alfanuméricos separados por hífen.
Esses endpoints devem usar obrigatoriamente a chave de api do produto para autenticação. X-Api-Key: xxx
Esses endpoints são usados pelo software do cliente para validar e ativar a licença, serão os unicos a serem exibidos publicamente na ScalarUI.
Os outros endpoints de outras Contrllers são de uso interno do dashboard, para o usuário gerenciar seus produtos e licenças, e devem usar a autenticação JWT do usuário do dashboard.
*/

using Microsoft.AspNetCore.Mvc;
using OpenLicenseApi.Services;
using OpenLicenseApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OpenLicenseApi.Security;

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
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                if (!productId.HasValue)
                {
                    return BadRequest(new { message = "ProductId is required." });
                }

                var licenses = await _licenseService.GetLicensesByProductIdAsync(userId, productId.Value);
                return Ok(licenses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseRequest request)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                if (request.ProductId == Guid.Empty)
                {
                    return BadRequest(new { message = "ProductId is required." });
                }

                var license = await _licenseService.CreateLicenseAsync(userId, request.ProductId, request);
                return Ok(license);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPut]
        public async Task<IActionResult> UpdateLicense([FromBody] UpdateLicenseRequest request)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                if (request.LicenseId == Guid.Empty)
                {
                    return BadRequest(new { message = "LicenseId is required." });
                }

                var license = await _licenseService.UpdateLicenseAsync(userId, request.LicenseId, request);
                return Ok(license);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete]
        public async Task<IActionResult> DeleteLicense([FromBody] DeleteLicenseRequest request)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                if (request.LicenseId == Guid.Empty)
                {
                    return BadRequest(new { message = "LicenseId is required." });
                }

                await _licenseService.DeleteLicenseAsync(userId, request.LicenseId);
                return Ok(new { message = "License deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateLicense([FromBody] ValidateLicenseRequest request)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid API key." });
                }

                var result = await _licenseService.ValidateLicenseAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
        [HttpPost("deactivate")]
        public async Task<IActionResult> DeactivateLicense([FromBody] DeactivateLicenseRequest request)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }

                await _licenseService.DeactivateLicenseAsync(userId, null, request);
                return Ok(new { message = "License deactivated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private bool TryGetUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out userId);
        }
    }
}
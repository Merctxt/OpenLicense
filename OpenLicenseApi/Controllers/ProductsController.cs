/* 
GET    /api/products <- lista os produtos do usuário autenticado (com jwt)
GET    /api/products/by-id?productId=... <- detalhes do produto, só se o produto pertencer ao usuário autenticado (com jwt)

POST   /api/products <- criar um produto para o usuário autenticado (com jwt), deve receber nome e descrição. Retorna os dados do produto criado.

PUT    /api/products <- atualizar os dados do produto, só se o produto pertencer ao usuário autenticado (com jwt)

DELETE /api/products <- deletar o produto, só se o produto pertencer ao usuário autenticado (com jwt)

---------

POST   /api/products/apikey <- criar uma nova chave de API para o produto, só se o produto pertencer ao usuário autenticado (com jwt)
DELETE /api/products/apikey <- deletar uma chave de API do produto, só se o produto pertencer ao usuário autenticado (com jwt)

Cada produto pertence a um usuario.
Cada chave de api para usar os endpoints de licenças pertence a um produto.
*/

using Microsoft.AspNetCore.Mvc;
using OpenLicenseApi.Services;
using OpenLicenseApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OpenLicenseApi.Controllers
{
    [ApiController]
    //[ApiExplorerSettings(GroupName = "internal")] <-  Endpoints de produto e apikey não devem aparecer na documentação pública da API, pois são usados apenas internamente pelo dashboard.
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        #region Products Endpoints

        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }
                var products = await _productService.GetProductsByUserIdAsync(userId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [Authorize]
        [HttpGet("by-id")]
        public async Task<IActionResult> GetProductById([FromQuery] Guid productId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }
                var product = await _productService.GetProductByIdAsync(userId, productId);
                if (product == null) return NotFound();
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }
                var product = await _productService.CreateProductAsync(userId, request);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }
                var product = await _productService.UpdateProductAsync(userId, request.ProductId, request);
                if (product == null) return NotFound();
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }
                await _productService.DeleteProductAsync(userId, request.ProductId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        // ######################## API Keys Endpoints ########################
        #region API Keys Endpoints

        [Authorize]
        [HttpGet("apikeys")]
        public async Task<IActionResult> GetApiKeys([FromQuery] Guid productId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token user id." });
                }
                var product = await _productService.GetProductByIdAsync(userId, productId);
                if (product == null) return NotFound();
                return Ok(product.ApiKeys);
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
                var apiKey = await _productService.CreateApiKeyAsync(userId, request.ProductId, request);
                if (apiKey == null) return NotFound();
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
                await _productService.DeleteApiKeyAsync(userId, request.ApiKeyId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
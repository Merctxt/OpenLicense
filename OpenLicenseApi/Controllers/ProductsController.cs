/* 
GET    /api/products <- lista os produtos do usuário autenticado (com jwt)

POST   /api/products <- criar um produto para o usuário autenticado (com jwt), deve receber nome e descrição. Retorna os dados do produto criado.

PUT    /api/products <- atualizar os dados do produto, só se o produto pertencer ao usuário autenticado (com jwt)

DELETE /api/products <- deletar o produto, só se o produto pertencer ao usuário autenticado (com jwt)

Cada produto pertence a um usuario.
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
    }
}
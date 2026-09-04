using Microsoft.AspNetCore.Mvc;
using OpenLicenseApi.Services;
using OpenLicenseApi.DTOs;
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

        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetProducts()
        {
            var userId = GetUserId();
            var products = await _productService.GetProductsByUserIdAsync(userId);
            return Ok(products);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            var userId = GetUserId();
            var product = await _productService.CreateProductAsync(userId, request);
            return Ok(product);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request)
        {
            var userId = GetUserId();
            var product = await _productService.UpdateProductAsync(userId, request.ProductId, request);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductRequest request)
        {
            var userId = GetUserId();
            await _productService.DeleteProductAsync(userId, request.ProductId);
            return NoContent();
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

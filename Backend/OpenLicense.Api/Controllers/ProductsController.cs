using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using OpenLicense.Api;
using OpenLicense.Application.Commands.Products;
using OpenLicense.Application.DTOs;
using OpenLicense.Application.Queries.Products;

namespace OpenLicense.Api.Controllers;

[ApiController]
[ApiVersion(ApiVersion.Current)]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    private readonly ISender _mediator;

    public ProductsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetProducts()
    {
        var query = new GetProductsByUserIdQuery(User.GetUserId());
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(User.GetUserId(), request.Name, request.Description);
        var product = await _mediator.Send(command);
        return Ok(product);
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request)
    {
        var command = new UpdateProductCommand(User.GetUserId(), request.ProductId, request.Name, request.Description);
        var product = await _mediator.Send(command);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductRequest request)
    {
        var command = new DeleteProductCommand(User.GetUserId(), request.ProductId);
        await _mediator.Send(command);
        return NoContent();
    }
}

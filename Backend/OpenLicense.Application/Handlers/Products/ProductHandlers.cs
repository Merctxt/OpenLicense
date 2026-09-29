using MediatR;
using OpenLicense.Application.Commands.Products;
using OpenLicense.Application.DTOs;
using OpenLicense.Application.Queries.Products;
using OpenLicense.Application.Services.Interfaces;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Handlers.Products;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Product>
{
    private readonly IProductService _productService;

    public CreateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Product> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        return await _productService.CreateProductAsync(request.UserId, new CreateProductRequest { Name = request.Name, Description = request.Description });
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Product>
{
    private readonly IProductService _productService;

    public UpdateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Product> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        return await _productService.UpdateProductAsync(request.UserId, request.ProductId, new UpdateProductRequest { ProductId = request.ProductId, Name = request.Name, Description = request.Description });
    }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductService _productService;

    public DeleteProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        await _productService.DeleteProductAsync(request.UserId, request.ProductId);
    }
}

public class GetProductsByUserIdQueryHandler : IRequestHandler<GetProductsByUserIdQuery, IEnumerable<Product>>
{
    private readonly IProductService _productService;

    public GetProductsByUserIdQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IEnumerable<Product>> Handle(GetProductsByUserIdQuery request, CancellationToken cancellationToken)
    {
        return await _productService.GetProductsByUserIdAsync(request.UserId);
    }
}

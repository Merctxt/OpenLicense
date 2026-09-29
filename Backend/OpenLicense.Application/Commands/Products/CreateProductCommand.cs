using MediatR;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Commands.Products;

public record CreateProductCommand(Guid UserId, string Name, string? Description) : IRequest<Product>;
public record UpdateProductCommand(Guid UserId, Guid ProductId, string? Name, string? Description) : IRequest<Product>;
public record DeleteProductCommand(Guid UserId, Guid ProductId) : IRequest;

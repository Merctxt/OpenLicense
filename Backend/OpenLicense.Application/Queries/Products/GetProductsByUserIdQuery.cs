using MediatR;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Queries.Products;

public record GetProductsByUserIdQuery(Guid UserId) : IRequest<IEnumerable<Product>>;

using OpenLicense.Application.DTOs;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Services.Interfaces;

public interface IProductService
{
    Task<Product> CreateProductAsync(Guid userId, CreateProductRequest request);
    Task<IEnumerable<Product>> GetProductsByUserIdAsync(Guid userId);
    Task<Product> UpdateProductAsync(Guid userId, Guid productId, UpdateProductRequest request);
    Task DeleteProductAsync(Guid userId, Guid productId);
}

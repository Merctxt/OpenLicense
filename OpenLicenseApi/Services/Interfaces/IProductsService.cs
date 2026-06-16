using OpenLicenseApi.Models;

namespace OpenLicenseApi.Services
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(Guid userId, CreateProductRequest request);
        Task<IEnumerable<Product>> GetProductsByUserIdAsync(Guid userId);
        Task<Product> UpdateProductAsync(Guid userId, Guid productId, UpdateProductRequest request);
        Task DeleteProductAsync(Guid userId, Guid productId);
    }
}   
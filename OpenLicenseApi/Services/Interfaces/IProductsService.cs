using OpenLicenseApi.Models;

namespace OpenLicenseApi.Services
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(Guid userId, CreateProductRequest request);
        Task<Product> GetProductByIdAsync(Guid userId, Guid productId);
        Task<IEnumerable<Product>> GetProductsByUserIdAsync(Guid userId);
        Task<Product> UpdateProductAsync(Guid userId, Guid productId, UpdateProductRequest request);
        Task DeleteProductAsync(Guid userId, Guid productId);
        Task<CreateApiKeyResponse> CreateApiKeyAsync(Guid userId, Guid productId, CreateApiKeyRequest request);
        Task DeleteApiKeyAsync(Guid userId, Guid apiKeyId);
    }
}   
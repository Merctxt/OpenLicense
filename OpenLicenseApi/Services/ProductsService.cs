using OpenLicenseApi.Models;
using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Data;
using System.Security.Cryptography;
using System.Text;

namespace OpenLicenseApi.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _dbContext;
        private const string ApiKeyPrefix = "api_";
        private const string ApiKeyChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const int ApiKeyBodyLength = 64;

        public ProductService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetProductsByUserIdAsync(Guid userId)
        {
            return await _dbContext.Products.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(Guid userId, Guid productId)
        {
            var product = await _dbContext.Products
                .Include(p => p.ApiKeys)
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            return product;
        }

        public async Task<Product> CreateProductAsync(Guid userId, CreateProductRequest request)
        {
            // valida se bateu o limite de produtos por conta
            var Limit = await _dbContext.Users.Where(u => u.Id == userId).Select(u => u.ProductLimit).FirstOrDefaultAsync();
            
            if (await _dbContext.Products.CountAsync(p => p.UserId == userId) >= Limit)
            {
                throw new Exception(string.Format("Product limit reached. You can only create up to {0} products.", Limit));
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 40)
            {
                throw new Exception("Product name is too long.");
            }

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 200)
            {
                throw new Exception("Product description is too long.");
            }
            var product = new Product
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return product;
        }

        public async Task<Product> UpdateProductAsync(Guid userId, Guid productId, UpdateProductRequest request)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 40)
            {
                throw new Exception("Product name is too long.");
            }

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 200)
            {
                throw new Exception("Product description is too long.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                product.Name = request.Name;
            }

            if (request.Description != null)
            {
                product.Description = request.Description;
            }

            await _dbContext.SaveChangesAsync();

            return product;
        }

        public async Task DeleteProductAsync(Guid userId, Guid productId)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<CreateApiKeyResponse> CreateApiKeyAsync(Guid userId, Guid productId, CreateApiKeyRequest request)
        {
            var apiKeyCount = await _dbContext.ApiKeys.CountAsync(k => k.ProductId == productId);
            if (apiKeyCount >= 3)
            {
                throw new Exception("API key limit reached for this product.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 40)
            {
                throw new Exception("API key name is too long.");
            }

            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            var plainApiKey = GenerateApiKey();

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Name = request.Name,
                KeyHash = ComputeSha256Hex(plainApiKey),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ApiKeys.Add(apiKey);
            await _dbContext.SaveChangesAsync();

            return new CreateApiKeyResponse
            {
                Id = apiKey.Id,
                Name = apiKey.Name,
                ApiKey = plainApiKey,
                CreatedAt = apiKey.CreatedAt,
                IsActive = apiKey.IsActive
            };
        }

        public async Task DeleteApiKeyAsync(Guid userId, Guid apiKeyId)
        {
            var apiKey = await _dbContext.ApiKeys
                .FirstOrDefaultAsync(k => k.Id == apiKeyId && k.Product.UserId == userId);
            if (apiKey == null)
            {
                throw new KeyNotFoundException("API key not found.");
            }

            _dbContext.ApiKeys.Remove(apiKey);
            await _dbContext.SaveChangesAsync();
        }

        private static string GenerateApiKey()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(ApiKeyBodyLength);
            var keyBody = new char[ApiKeyBodyLength];

            for (var i = 0; i < ApiKeyBodyLength; i++)
            {
                keyBody[i] = ApiKeyChars[randomBytes[i] % ApiKeyChars.Length];
            }

            return $"{ApiKeyPrefix}{new string(keyBody)}";
        }

        private static string ComputeSha256Hex(string input)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash);
        }
    }
}
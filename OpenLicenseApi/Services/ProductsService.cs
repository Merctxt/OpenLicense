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

        public ProductService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetProductsByUserIdAsync(Guid userId)
        {
            return await _dbContext.Products
                .Include(p => p.Licenses)
                .Where(p => p.UserId == userId)
                .ToListAsync();
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

    }
}
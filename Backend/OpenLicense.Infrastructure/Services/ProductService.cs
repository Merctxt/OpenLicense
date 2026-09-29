using OpenLicense.Application.DTOs;
using OpenLicense.Application.Services.Interfaces;
using OpenLicense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OpenLicense.Infrastructure.Data;

namespace OpenLicense.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;

    public ProductService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Product>> GetProductsByUserIdAsync(Guid userId)
    {
        await EnsureUserActiveAsync(userId);

        var products = await _dbContext.Products
            .Where(p => p.UserId == userId)
            .AsNoTracking()
            .ToListAsync();

        if (products.Count == 0)
        {
            return products;
        }

        var productIds = products.Select(p => p.Id).ToList();

        var licenses = await _dbContext.Licenses
            .Where(l => productIds.Contains(l.ProductId))
            .AsNoTracking()
            .ToListAsync();

        var licensesByProduct = licenses.GroupBy(l => l.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var product in products)
        {
            product.Licenses = licensesByProduct.TryGetValue(product.Id, out var productLicenses)
                ? new List<License>(productLicenses)
                : new List<License>();
        }

        return products;
    }

    public async Task<Product> CreateProductAsync(Guid userId, CreateProductRequest request)
    {
        await EnsureUserActiveAsync(userId);

        var limit = await _dbContext.Users.Where(u => u.Id == userId).Select(u => u.ProductLimit).FirstOrDefaultAsync();

        if (await _dbContext.Products.CountAsync(p => p.UserId == userId) >= limit)
        {
            throw new InvalidOperationException($"Product limit reached. You can only create up to {limit} products.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 40)
        {
            throw new ArgumentException("Product name is too long.");
        }

        if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 200)
        {
            throw new ArgumentException("Product description is too long.");
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
        await EnsureUserActiveAsync(userId);

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 40)
        {
            throw new ArgumentException("Product name is too long.");
        }

        if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 200)
        {
            throw new ArgumentException("Product description is too long.");
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
        await EnsureUserActiveAsync(userId);

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
    }

    private async Task EnsureUserActiveAsync(Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }
        if (user.IsSuspended)
        {
            throw new UnauthorizedAccessException("Account is suspended.");
        }
    }
}

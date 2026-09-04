using OpenLicenseApi.Models;
using OpenLicenseApi.DTOs;
using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Data;
using System.Security.Cryptography;

namespace OpenLicenseApi.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly AppDbContext _dbContext;
        private const string LicenseKeyChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int LicenseKeyBlockLength = 4;
        private const int LicenseKeyLength = 16;
        public LicenseService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<License>> GetLicensesByProductIdAsync(Guid userId, Guid productId)
        {
            await EnsureUserActiveAsync(userId);

            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            return await _dbContext.Licenses.Where(l => l.ProductId == productId).ToListAsync();
        }

        public async Task<License> CreateLicenseAsync(Guid userId, Guid productId, CreateLicenseRequest request)
        {
            await EnsureUserActiveAsync(userId);

            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 40)
            {
                throw new ArgumentException("License name is too long.");
            }

            if (request.MaxActivations <= 0)
            {
                throw new ArgumentException("MaxActivations must be greater than zero.");
            }

            var license = new License
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Name = request.Name,
                LicenseKey = GenerateLicenseKey(),
                Status = true,
                ExpiresAt = request.ExpiresAt,
                MaxActivations = request.MaxActivations,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Licenses.Add(license);
            await _dbContext.SaveChangesAsync();

            return license;
        }

        public async Task<License> UpdateLicenseAsync(Guid userId, Guid licenseId, UpdateLicenseRequest request)
        {
            await EnsureUserActiveAsync(userId);

            var license = await _dbContext.Licenses
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l => l.Id == licenseId && l.Product.UserId == userId);

            if (license == null)
            {
                throw new KeyNotFoundException("License not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 40)
            {
                throw new ArgumentException("License name is too long.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                license.Name = request.Name;
            }

            if (request.ExpiresAt.HasValue)
            {
                license.ExpiresAt = request.ExpiresAt.Value;
            }

            if (request.MaxActivations.HasValue)
            {
                if (request.MaxActivations.Value <= 0)
                {
                    throw new ArgumentException("MaxActivations must be greater than zero.");
                }

                license.MaxActivations = request.MaxActivations.Value;
            }

            if (request.Status.HasValue)
            {
                var newStatus = request.Status.Value;

            if (license.Status == newStatus)
            {
                throw new InvalidOperationException($"License is already in status {(newStatus ? "active" : "suspended")}.");
            }

                license.Status = newStatus;
            }

            await _dbContext.SaveChangesAsync();

            return license;
        }

        public async Task DeleteLicenseAsync(Guid userId, Guid licenseId)
        {
            await EnsureUserActiveAsync(userId);

            var license = await _dbContext.Licenses
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l => l.Id == licenseId && l.Product.UserId == userId);

            if (license == null)
            {
                throw new KeyNotFoundException("License not found.");
            }

            _dbContext.Licenses.Remove(license);
            await _dbContext.SaveChangesAsync();
        }
    
        public async Task<IEnumerable<Activation>> GetLicenseActivationsAsync(Guid userId, Guid licenseId)
        {
            await EnsureUserActiveAsync(userId);

            var license = await _dbContext.Licenses
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l => l.Id == licenseId && l.Product.UserId == userId);

            if (license == null)
            {
                throw new KeyNotFoundException("License not found.");
            }

            return await _dbContext.Activations
                .Where(a => a.LicenseId == licenseId)
                .ToListAsync();
        }

        public async Task<bool> LicenseBelongsToScopeAsync(Guid userId, Guid licenseId, Guid? productId)
        {
            return await _dbContext.Licenses.AnyAsync(l =>
                l.Id == licenseId
                && l.Product.UserId == userId
                && (!productId.HasValue || l.ProductId == productId.Value));
        }

        public async Task<ValidateLicenseResponse> ValidateLicenseAsync(Guid userId, ValidateLicenseRequest request)
        {
            await EnsureUserActiveAsync(userId);

            var normalizedKey = NormalizeLicenseKey(request.LicenseKey);

            var license = await _dbContext.Licenses
                .Include(l => l.Activations)
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l => l.Product.UserId == userId && l.LicenseKey == normalizedKey);

            if (license == null)
            {
                throw new KeyNotFoundException("Invalid license.");
            }

            if (!license.Status)
            {
                throw new InvalidOperationException("Inactive license.");
            }

            if (license.ExpiresAt.HasValue && license.ExpiresAt.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Inactive license.");
            }

            var existingActivation = await _dbContext.Activations
                .FirstOrDefaultAsync(a => a.LicenseId == license.Id && a.HardwareId == request.HardwareId && a.IsActive);

            if (existingActivation != null)
            {
                existingActivation.LastSeenAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                var currentActivations = await _dbContext.Activations
                    .CountAsync(a => a.LicenseId == license.Id && a.IsActive);

                return new ValidateLicenseResponse
                {
                    IsValid = true,
                    Message = "License is valid.",
                    ReusedActivation = true,
                    CurrentActivations = currentActivations,
                    MaxActivations = license.MaxActivations,
                    ExpiresAt = license.ExpiresAt
                };
            }

            var activationCount = await _dbContext.Activations
                .CountAsync(a => a.LicenseId == license.Id && a.IsActive);

            if (activationCount >= license.MaxActivations)
            {
                throw new InvalidOperationException("Activation limit reached.");
            }

            var activation = new Activation
            {
                Id = Guid.NewGuid(),
                LicenseId = license.Id,
                ActivatedAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow,
                HardwareId = request.HardwareId,
                IsActive = true
            };

            _dbContext.Activations.Add(activation);
            await _dbContext.SaveChangesAsync();

            return new ValidateLicenseResponse
            {
                IsValid = true,
                Message = "License is valid.",
                ReusedActivation = false,
                CurrentActivations = activationCount + 1,
                MaxActivations = license.MaxActivations,
                ExpiresAt = license.ExpiresAt
            };
        }

        public async Task DeactivateLicenseAsync(Guid userId, Guid? productId, DeactivateLicenseRequest request)
        {
            await EnsureUserActiveAsync(userId);

            var normalizedKey = NormalizeLicenseKey(request.LicenseKey);

            var license = await _dbContext.Licenses
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l =>
                    l.LicenseKey == normalizedKey
                    && l.Product.UserId == userId
                    && (!productId.HasValue || l.ProductId == productId.Value));

            if (license == null)
            {
                throw new KeyNotFoundException("License not found.");
            }

            var activation = await _dbContext.Activations
                .FirstOrDefaultAsync(a =>
                    a.LicenseId == license.Id
                    && a.HardwareId == request.HardwareId
                    && a.IsActive);

            if (activation == null)
            {
                throw new KeyNotFoundException("Activation not found for this hardware id.");
            }

            _dbContext.Activations.Remove(activation);
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

        private static string GenerateLicenseKey()
        {
            var keyChars = new char[LicenseKeyLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[LicenseKeyLength];
                rng.GetBytes(bytes);
                for (int i = 0; i < LicenseKeyLength; i++)
                {
                    keyChars[i] = LicenseKeyChars[bytes[i] % LicenseKeyChars.Length];
                }
            }

            var block1 = new string(keyChars, 0, LicenseKeyBlockLength);
            var block2 = new string(keyChars, LicenseKeyBlockLength, LicenseKeyBlockLength);
            var block3 = new string(keyChars, LicenseKeyBlockLength * 2, LicenseKeyBlockLength);
            var block4 = new string(keyChars, LicenseKeyBlockLength * 3, LicenseKeyBlockLength);

            return $"{block1}-{block2}-{block3}-{block4}";
        }

        private static string NormalizeLicenseKey(string licenseKey)
        {
            return licenseKey.Trim().ToUpperInvariant();
        }
    }
}
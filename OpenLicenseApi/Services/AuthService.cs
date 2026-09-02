using OpenLicenseApi.Models;
using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Data;
using Microsoft.AspNetCore.Identity;
using OpenLicenseApi.Middleware;
using System.Security.Cryptography;
using System.Text;

namespace OpenLicenseApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;
        private const string ApiKeyPrefix = "api_";
        private const string ApiKeyChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const int ApiKeyBodyLength = 64;

        public AuthService(AppDbContext dbContext, IJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
        }

        public async Task DeleteAsync(Guid userId)
        {
            await EnsureUserActiveAsync(userId);

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Users> GetMeAsync(Guid userId)
        {
            await EnsureUserActiveAsync(userId);

            var user = await _dbContext.Users
                .Include(u => u.ApiKeys)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            return user;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (user == null)
            {
                throw new Exception("Invalid email or password.");
            }

            if (user.IsSuspended)
            {
                throw new Exception("Invalid email or password.");
            }

            var passwordHasher = new PasswordHasher<Users>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new Exception("Invalid email or password.");
            }

            return _jwtTokenService.GenerateToken(user);
        }

        public async Task<Users> RegisterAsync(string name, string email, string password)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (existingUser != null)
            {
                throw new Exception("User with the same email already exists.");
            }

            if (name.Length > 40)
            {
                throw new Exception("User has a name too long.");
            }

            var newUser = new Users
            {
                Name = name,
                Email = email.Trim().ToLower(),
                CreatedAt = DateTime.UtcNow
            };

            var passwordHasher = new PasswordHasher<Users>();
            newUser.PasswordHash = passwordHasher.HashPassword(newUser, password);

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return newUser;

        }

        public async Task<Users> UpdateAsync(Guid userId, string? name, string? email, string? password)
        {
            await EnsureUserActiveAsync(userId);

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (!string.IsNullOrWhiteSpace(name) && name.Length > 40)
            {
                throw new Exception("User has a name too long.");
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                user.Name = name;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower() && u.Id != userId);
                if (existingUser != null)
                {
                    throw new Exception("Another user with the same email already exists.");
                }
                user.Email = email.Trim().ToLower();
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                var passwordHasher = new PasswordHasher<Users>();
                user.PasswordHash = passwordHasher.HashPassword(user, password);
            }

            await _dbContext.SaveChangesAsync();

            return user;
        }
        #region Apikey
        public async Task<CreateApiKeyResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request)
        {
            await EnsureUserActiveAsync(userId);

            var apiKeyCount = await _dbContext.ApiKeys.CountAsync(k => k.UserId == userId);
            if (apiKeyCount >= 3)
            {
                throw new Exception("API key limit reached for this account.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 40)
            {
                throw new Exception("API key name is too long.");
            }

            var plainApiKey = GenerateApiKey();

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                UserId = userId,
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
            await EnsureUserActiveAsync(userId);

            var apiKey = await _dbContext.ApiKeys
                .FirstOrDefaultAsync(k => k.Id == apiKeyId && k.UserId == userId);
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
        #endregion

        private async Task EnsureUserActiveAsync(Guid userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("Invalid email or password.");
            }
            if (user.IsSuspended)
            {
                throw new Exception("Invalid email or password.");
            }
        }
    }
}
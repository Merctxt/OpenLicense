using OpenLicenseApi.Models;
using OpenLicenseApi.DTOs;
using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Data;
using Microsoft.AspNetCore.Identity;
using OpenLicenseApi.Middleware;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace OpenLicenseApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private const string ApiKeyPrefix = "api_";
        private const string ApiKeyChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const int ApiKeyBodyLength = 64;
        private const int TokenLength = 32;
        private const int TokenExpiryMinutes = 15;

        public AuthService(AppDbContext dbContext, IJwtTokenService jwtTokenService, IEmailService emailService)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
        }

        public async Task DeleteAsync(Guid userId)
        {
            await EnsureUserActiveAsync(userId);

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
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
                throw new KeyNotFoundException("User not found.");
            }

            return user;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (user.IsSuspended)
            {
                throw new UnauthorizedAccessException("Account is suspended.");
            }

            var passwordHasher = new PasswordHasher<Users>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return _jwtTokenService.GenerateToken(user);
        }

        public async Task<Users> RegisterAsync(string name, string email, string password)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with the same email already exists.");
            }

            if (name.Length > 40)
            {
                throw new ArgumentException("User name is too long.");
            }

            ValidatePassword(password);

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
                throw new KeyNotFoundException("User not found.");
            }

            if (!string.IsNullOrWhiteSpace(name) && name.Length > 40)
            {
                throw new ArgumentException("User name is too long.");
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
                    throw new InvalidOperationException("Another user with the same email already exists.");
                }
                user.Email = email.Trim().ToLower();
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                ValidatePassword(password);
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
                throw new InvalidOperationException("API key limit reached for this account.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Length > 40)
            {
                throw new ArgumentException("API key name is too long.");
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
                throw new UnauthorizedAccessException("Invalid credentials.");
            }
            if (user.IsSuspended)
            {
                throw new UnauthorizedAccessException("Account is suspended.");
            }
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password is required.");
            }

            if (password.Length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters long.");
            }

            if (password.Length > 128)
            {
                throw new ArgumentException("Password must not exceed 128 characters.");
            }

            bool hasUpper = password.Any(c => char.IsUpper(c));
            bool hasLower = password.Any(c => char.IsLower(c));
            bool hasDigit = password.Any(c => char.IsDigit(c));
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            var missing = new List<string>();
            if (!hasUpper) missing.Add("uppercase letter");
            if (!hasLower) missing.Add("lowercase letter");
            if (!hasDigit) missing.Add("digit");
            if (!hasSpecial) missing.Add("special character");

            if (missing.Count > 0)
            {
                throw new ArgumentException($"Password must contain at least one {string.Join(", ", missing)}.");
            }
        }

        #region Password Recovery
        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (user == null)
            {
                return;
            }

            var token = GenerateResetToken();

            await _emailService.SendPasswordResetEmailAsync(user.Email, token);

            user.PasswordResetToken = ComputeSha256Hex(token);
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(TokenExpiryMinutes);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> VerifyResetTokenAsync(string email, string token)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (user == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(user.PasswordResetToken) || user.PasswordResetTokenExpiry == null)
            {
                return false;
            }

            if (DateTime.UtcNow > user.PasswordResetTokenExpiry)
            {
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                await _dbContext.SaveChangesAsync();
                return false;
            }

            return user.PasswordResetToken == ComputeSha256Hex(token);
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (string.IsNullOrEmpty(user.PasswordResetToken) || user.PasswordResetTokenExpiry == null)
            {
                throw new UnauthorizedAccessException("Invalid or expired token.");
            }

            if (DateTime.UtcNow > user.PasswordResetTokenExpiry)
            {
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                await _dbContext.SaveChangesAsync();
                throw new UnauthorizedAccessException("Invalid or expired token.");
            }

            if (user.PasswordResetToken != ComputeSha256Hex(token))
            {
                throw new UnauthorizedAccessException("Invalid token.");
            }

            ValidatePassword(newPassword);

            var passwordHasher = new PasswordHasher<Users>();
            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _dbContext.SaveChangesAsync();
        }

        private static string GenerateResetToken()
        {
            var bytes = new byte[TokenLength];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes).ToLower().Replace("0O", "aB");
        }
        #endregion
    }
}
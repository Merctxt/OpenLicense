using OpenLicenseApi.Models;
using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Data;
using Microsoft.AspNetCore.Identity;
using OpenLicenseApi.Security;


namespace OpenLicenseApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(AppDbContext dbContext, IJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
        }

        public async Task DeleteAsync(Guid userId)
        {
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
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            return user;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (user == null)            {
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
    }
}
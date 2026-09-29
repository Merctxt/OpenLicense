using OpenLicense.Application.DTOs;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Application.Services.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(string name, string email, string password);
    Task<string> LoginAsync(string email, string password);
    Task<User> GetMeAsync(Guid userId);
    Task<User> UpdateAsync(Guid userId, string? name, string? email, string? password);
    Task DeleteAsync(Guid userId);
    Task<CreateApiKeyResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request);
    Task DeleteApiKeyAsync(Guid userId, Guid apiKeyId);
    Task<bool> ToggleApiKeyAsync(Guid userId, Guid apiKeyId);
    Task ForgotPasswordAsync(string email);
    Task<bool> VerifyResetTokenAsync(string email, string token);
    Task ResetPasswordAsync(string email, string token, string newPassword);
    Task<User> UpdateReportPreferencesAsync(Guid userId, bool reportsOptIn);
}

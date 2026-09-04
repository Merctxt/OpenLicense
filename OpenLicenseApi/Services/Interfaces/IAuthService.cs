using OpenLicenseApi.Models;

namespace OpenLicenseApi.Services
{
    public interface IAuthService
    {
        Task<Users> RegisterAsync(
            string name,
            string email,
            string password);

        Task<string> LoginAsync(
            string email,
            string password);

        Task<Users> GetMeAsync(Guid userId);

        Task<Users> UpdateAsync(
            Guid userId,
            string? name,
            string? email,
            string? password);

        Task DeleteAsync(Guid userId);

        Task<CreateApiKeyResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request);
        Task DeleteApiKeyAsync(Guid userId, Guid apiKeyId);

        Task ForgotPasswordAsync(string email);
        Task<bool> VerifyResetTokenAsync(string email, string token);
        Task ResetPasswordAsync(string email, string token, string newPassword);
    }
}
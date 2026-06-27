using OpenLicenseApi.Models;

namespace OpenLicenseApi.Services
{
    public interface ILicenseService
    {
        Task<IEnumerable<License>> GetLicensesByProductIdAsync(Guid userId, Guid productId);
        Task<License> CreateLicenseAsync(Guid userId, Guid productId, CreateLicenseRequest request);
        Task<License> UpdateLicenseAsync(Guid userId, Guid licenseId, UpdateLicenseRequest request);
        Task DeleteLicenseAsync(Guid userId, Guid licenseId);
        Task<IEnumerable<Activation>> GetLicenseActivationsAsync(Guid userId, Guid licenseId);
        Task<bool> LicenseBelongsToScopeAsync(Guid userId, Guid licenseId, Guid? productId);
        Task<ValidateLicenseResponse> ValidateLicenseAsync(Guid userId, ValidateLicenseRequest request);
        Task DeactivateLicenseAsync(Guid userId, Guid? productId, DeactivateLicenseRequest request);
    }
}
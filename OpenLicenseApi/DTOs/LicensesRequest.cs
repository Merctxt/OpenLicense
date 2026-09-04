using System;
using System.ComponentModel.DataAnnotations;

namespace OpenLicenseApi.DTOs
{
    public class CreateLicenseRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxActivations must be greater than zero.")]
        public int MaxActivations { get; set; }
    }

    public class UpdateLicenseRequest
    {
        [Required]
        public Guid LicenseId { get; set; }

        public string? Name { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public int? MaxActivations { get; set; }

        public bool? Status { get; set; }
    }

    public class DeleteLicenseRequest
    {
        [Required]
        public Guid LicenseId { get; set; }
    }

    public class ValidateLicenseRequest
    {
        [Required]
        public string LicenseKey { get; set; } = string.Empty;

        [Required]
        public string HardwareId { get; set; } = string.Empty;
    }

    public class DeactivateLicenseRequest
    {
        [Required]
        public string LicenseKey { get; set; } = string.Empty;

        [Required]
        public string HardwareId { get; set; } = string.Empty;
    }

    public class ValidateLicenseResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool ReusedActivation { get; set; }
        public int CurrentActivations { get; set; }
        public int MaxActivations { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
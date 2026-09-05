using System.ComponentModel.DataAnnotations;

namespace OpenLicenseApi.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyTokenRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}

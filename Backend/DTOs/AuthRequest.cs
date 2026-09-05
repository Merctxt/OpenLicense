using System.ComponentModel.DataAnnotations;

namespace OpenLicenseApi.DTOs
{
      public class RegisterRequest
      {
          [Required]
          public string Name { get; set; } = string.Empty;

          [Required]
          [EmailAddress] // Valida formato de e-mail automaticamente
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
      {
          [Required]
          [EmailAddress] // Valida formato de e-mail automaticamente
          public string Email { get; set; } = string.Empty;

          [Required]
          public string Password { get; set; } = string.Empty;
      }

      public class DeleteRequest
      {
          [Required]
          public int UserId { get; set; }
      }

      public class UpdateRequest
      {
          public string? Name { get; set; }

        [EmailAddress] // Valida formato de e-mail automaticamente
        public string? Email { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string? Password { get; set; }
      }

    public class CreateApiKeyRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }

    public class DeleteApiKeyRequest
    {
        [Required]
        public Guid ApiKeyId { get; set; }
    }

    public class CreateApiKeyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
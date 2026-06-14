using System.ComponentModel.DataAnnotations;

  namespace OpenLicenseApi.Models
  {
      public class RegisterRequest
      {
          [Required]
          public string Name { get; set; } = string.Empty;

          [Required]
          [EmailAddress] // Valida formato de e-mail automaticamente
          public string Email { get; set; } = string.Empty;

          [Required]
          [MinLength(8, ErrorMessage = "A senha deve conter pelo menos 8 caracteres.")]
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
          [Required]
          public int UserId { get; set; }

          public string? Name { get; set; }

          [EmailAddress] // Valida formato de e-mail automaticamente
          public string? Email { get; set; }

          [MinLength(8, ErrorMessage = "A senha deve conter pelo menos 8 caracteres.")]
          public string? Password { get; set; }
      }
  }
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenLicenseApi.Models
{
    public class CreateProductRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class UpdateProductRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
    }

    public class DeleteProductRequest
    {
        [Required]
        public Guid ProductId { get; set; }
    }

    public class CreateApiKeyRequest
    {
        [Required]
        public Guid ProductId { get; set; }

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
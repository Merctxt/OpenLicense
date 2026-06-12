using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenLicenseApi.Models
{
    public class ApiKey
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string KeyHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
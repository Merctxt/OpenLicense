using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenLicenseApi.Models
{
    public class License
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ProductId { get; set; }
        [Required]
        public Product Product { get; set; } = null!;

        public string Name { get; set; } = null!;

        [Required]
        public string LicenseKey { get; set; } = null!;

        [Required]
        public LicenseStatus Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public int MaxActivations { get; set; }

        public ICollection<Activation> Activations { get; set; } = new List<Activation>();
    }

    public enum LicenseStatus
    {
        Active,
        Suspended,
        Expired,
        Revoked
    }
}

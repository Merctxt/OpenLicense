using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OpenLicenseApi.Models
{
    public class License
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [JsonIgnore]
        [Required]
        public Product Product { get; set; } = null!;

        public string Name { get; set; } = null!;

        [Required]
        public string LicenseKey { get; set; } = null!;

        [Required]
        public bool Status { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public int MaxActivations { get; set; }

        [JsonIgnore]
        public ICollection<Activation> Activations { get; set; } = new List<Activation>();
    }
}

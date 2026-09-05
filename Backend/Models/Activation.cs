using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OpenLicenseApi.Models
{
    public class Activation
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid LicenseId { get; set; }

        [JsonIgnore]
        [Required]
        public License License { get; set; } = null!;
        [Required]
        public string HardwareId { get; set; } = null!;
        [Required]
        public DateTime ActivatedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
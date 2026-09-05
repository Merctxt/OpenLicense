using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OpenLicenseApi.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        [JsonIgnore]
        public Users User { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<License> Licenses { get; set; } = new List<License>();
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenLicenseApi.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Users User { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<License> Licenses { get; set; } = new List<License>();
        public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    }
}
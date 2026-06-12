using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenLicenseApi.Models
{
    public class Users
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int ProductLimit { get; set; } = 3;
        public int LicenseLimit { get; set; } = 450;
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
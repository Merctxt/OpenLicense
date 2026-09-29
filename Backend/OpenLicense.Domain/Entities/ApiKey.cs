using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OpenLicense.Domain.Entities;

public class ApiKey
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [JsonIgnore]
    public User User { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [JsonIgnore]
    public string KeyHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

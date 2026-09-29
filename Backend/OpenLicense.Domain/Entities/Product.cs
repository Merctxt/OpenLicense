using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OpenLicense.Domain.Entities;

public class Product
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<License> Licenses { get; set; } = new List<License>();
}

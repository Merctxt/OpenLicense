namespace OpenLicenseApi.Models
{
    public class ApiKey
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Users User { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string KeyHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
namespace OpenLicenseApi.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Users User { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public ICollection<License> Licenses { get; set; } = new List<License>();
    }
}
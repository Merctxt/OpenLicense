namespace OpenLicenseApi.Models
{
    public class License
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Key { get; set; } = null!;

        public LicenseStatus Status { get; set; }

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

namespace OpenLicenseApi.Models
{
    public class Activation
    {
        public Guid Id { get; set; }
        public Guid LicenseId { get; set; }
        public License License { get; set; } = null!;
        public string HardwareId { get; set; } = null!;
        public DateTime ActivatedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
    }
}
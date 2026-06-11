namespace OpenLicenseApi.Models
{
    public class Users
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public PlanType Plan { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    }

    public enum PlanType
    {
        Free,
        Starter,
        Pro
    }

}
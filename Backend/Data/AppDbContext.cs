using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Models;

namespace OpenLicenseApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Users> Users => Set<Users>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<License> Licenses => Set<License>();
        public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
        public DbSet<Activation> Activations => Set<Activation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationships and constraints for the entities
            modelBuilder.Entity<License>()
                .HasIndex(x => x.LicenseKey)
                .IsUnique();

            modelBuilder.Entity<ApiKey>()
                .HasIndex(x => x.KeyHash)
                .IsUnique();
        }
    }
}
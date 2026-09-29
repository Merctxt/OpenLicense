using Microsoft.EntityFrameworkCore;
using OpenLicense.Domain.Entities;

namespace OpenLicense.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.Entities.User> Users => Set<Domain.Entities.User>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<License> Licenses => Set<License>();
        public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
        public DbSet<Activation> Activations => Set<Activation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<License>()
                .HasIndex(x => x.LicenseKey)
                .IsUnique();

            modelBuilder.Entity<ApiKey>()
                .HasIndex(x => x.KeyHash)
                .IsUnique();
        }
    }
}

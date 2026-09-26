using OpenLicenseApi.Data;
using OpenLicenseApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace OpenLicenseApi.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _dbContext;

        public ReportService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ReportDataDto> GenerateReportForUserAsync(Guid userId, int expiringDaysWarning)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var products = await _dbContext.Products
                .Where(p => p.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var productIds = products.Select(p => p.Id).ToList();

            var licenses = await _dbContext.Licenses
                .Where(l => productIds.Contains(l.ProductId))
                .Include(l => l.Product)
                .AsNoTracking()
                .ToListAsync();

            var licenseIds = licenses.Select(l => l.Id).ToList();

            var activations = await _dbContext.Activations
                .Where(a => licenseIds.Contains(a.LicenseId))
                .AsNoTracking()
                .ToListAsync();

            var activationsByLicense = activations.GroupBy(a => a.LicenseId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var report = new ReportDataDto
            {
                UserName = user.Name,
                UserEmail = user.Email,
                GeneratedAt = DateTime.UtcNow
            };

            var allLicenses = licenses.ToList();

            report.TotalLicenses = allLicenses.Count;
            report.ActiveLicenses = allLicenses.Count(l => l.Status);
            report.SuspendedLicenses = allLicenses.Count(l => !l.Status);

            var productLookup = products.ToDictionary(p => p.Id);
            var licenseLookup = allLicenses.ToDictionary(l => l.Id);

            foreach (var product in products)
            {
                var productLicenses = allLicenses.Where(l => l.ProductId == product.Id).ToList();
                var total = productLicenses.Count;
                var active = productLicenses.Count(l => l.Status);
                var suspended = productLicenses.Count(l => !l.Status);

                report.LicenseSummaries.Add(new LicenseSummaryDto
                {
                    ProductName = product.Name,
                    TotalLicenses = total,
                    MaxLicenses = user.LicenseLimit,
                    ActiveLicenses = active,
                    SuspendedLicenses = suspended
                });
            }

            var now = DateTime.UtcNow;
            var expiringThreshold = now.AddDays(expiringDaysWarning);

            var expiringLicenses = allLicenses
                .Where(l => l.Status && l.ExpiresAt.HasValue && l.ExpiresAt.Value <= expiringThreshold && l.ExpiresAt.Value > now)
                .OrderBy(l => l.ExpiresAt)
                .ToList();

            foreach (var license in expiringLicenses)
            {
                var daysRemaining = (license.ExpiresAt.Value - now).Days;
                report.ExpiringLicenses.Add(new ExpiringLicenseDto
                {
                    LicenseName = license.Name,
                    LicenseKey = license.LicenseKey,
                    ProductName = productLookup.TryGetValue(license.ProductId, out var product) ? product.Name : "Unknown",
                    ExpiresAt = license.ExpiresAt.Value,
                    DaysRemaining = daysRemaining
                });
            }

            report.ExpiringCount = report.ExpiringLicenses.Count;

            var expiredLicenses = allLicenses
                .Where(l => l.ExpiresAt.HasValue && l.ExpiresAt.Value < now)
                .OrderBy(l => l.ExpiresAt)
                .ToList();

            foreach (var license in expiredLicenses)
            {
                var daysExpired = (now - license.ExpiresAt.Value).Days;
                report.ExpiredLicenses.Add(new ExpiredLicenseDto
                {
                    LicenseName = license.Name,
                    LicenseKey = license.LicenseKey,
                    ProductName = productLookup.TryGetValue(license.ProductId, out var product) ? product.Name : "Unknown",
                    ExpiresAt = license.ExpiresAt.Value,
                    DaysExpired = daysExpired
                });
            }

            report.ExpiredCount = report.ExpiredLicenses.Count;

            return report;
        }
    }
}

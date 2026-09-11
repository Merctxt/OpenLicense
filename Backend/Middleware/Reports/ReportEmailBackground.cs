using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenLicenseApi.Data;
using OpenLicenseApi.DTOs;
using OpenLicenseApi.Services;

namespace OpenLicenseApi.Middleware.Reports
{
    public class ReportSettings
    {
        public int IntervalMinutes { get; set; }
        public int ExpiryWarningDays { get; set; }
        public bool Enabled { get; set; }
    }

    public class ReportEmailBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ReportSettings _settings;

        public ReportEmailBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<ReportSettings> options)
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
            {
                return;
            }

            var interval = TimeSpan.FromMinutes(_settings.IntervalMinutes);

            using var timer = new PeriodicTimer(interval);

            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var usersWithReportsOptIn = await dbContext.Users
                        .Where(u => u.ReportsOptIn == true)
                        .ToListAsync(stoppingToken);

                    foreach (var user in usersWithReportsOptIn)
                    {
                        try
                        {
                            var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                            var report = await reportService.GenerateReportForUserAsync(user.Id, _settings.ExpiryWarningDays);

                            var subject = $"License Report - {report.GeneratedAt.ToString("MMM dd, yyyy")}";
                            await emailService.SendReportEmailAsync(user.Email, subject, report);
                        }
                        catch
                        {
                            // Silently handle per-user errors
                        }
                    }
                }
                catch
                {
                    // Silently handle loop errors
                }
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenLicense.Application.Services.Interfaces;
using OpenLicense.Infrastructure.Data;

namespace OpenLicense.Infrastructure;

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
    private readonly ILogger<ReportEmailBackgroundService> _logger;

    public ReportEmailBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<ReportSettings> options,
        ILogger<ReportEmailBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
        _logger = logger;
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
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate/send report for user {UserId} ({UserEmail})", user.Id, user.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in report email background service loop iteration");
            }
        }
    }
}

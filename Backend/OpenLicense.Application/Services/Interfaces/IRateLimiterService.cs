namespace OpenLicense.Application.Services.Interfaces;

public interface IRateLimiterService
{
    bool IsAllowed(string key, int maxRequests, TimeSpan window);
}

namespace OpenLicenseApi.Services
{
    public interface IRateLimiterService
    {
        bool IsAllowed(string key, int maxRequests, TimeSpan window);
    }
}

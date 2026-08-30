using System.Collections.Concurrent;

namespace OpenLicenseApi.Services
{
    public interface IRateLimiterService
    {
        bool IsAllowed(string key, int maxRequests, TimeSpan window);
    }

    /// <summary>
    /// In-memory sliding-window rate limiter. Thread-safe. No external dependencies.
    /// </summary>
    public class RateLimiterService : IRateLimiterService
    {
        // key -> list of timestamps when requests were made
        private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
        private readonly object _cleanupLock = new();

        // How often to run cleanup of expired entries (every 5 minutes)
        private DateTime _lastCleanup = DateTime.UtcNow;

        public bool IsAllowed(string key, int maxRequests, TimeSpan window)
        {
            var now = DateTime.UtcNow;

            // Cleanup old data periodically to avoid unbounded memory growth
            if ((now - _lastCleanup).TotalMinutes > 5)
            {
                lock (_cleanupLock)
                {
                    if ((now - _lastCleanup).TotalMinutes > 5)
                    {
                        var expiredKeys = _requests
                            .Where(kvp => kvp.Value.LastOrDefault() < now.Add(-window))
                            .Select(kvp => kvp.Key)
                            .ToList();
                        foreach (var k in expiredKeys)
                        {
                            _requests.TryRemove(k, out _);
                        }
                        _lastCleanup = now;
                    }
                }
            }

            // Get or create the timestamp list for this key
            var timestamps = _requests.GetOrAdd(key, new List<DateTime>());

            // Filter to only requests within the window
            int countInWindow;
            lock (timestamps)
            {
                var cutoff = now.Add(-window);
                var idx = 0;
                while (idx < timestamps.Count && timestamps[idx] < cutoff)
                    idx++;

                if (idx > 0)
                {
                    // Remove expired entries from the front
                    timestamps.RemoveRange(0, idx);
                }

                countInWindow = timestamps.Count;
            }

            if (countInWindow >= maxRequests)
            {
                return false;
            }

            lock (timestamps)
            {
                timestamps.Add(now);
            }

            return true;
        }
    }
}

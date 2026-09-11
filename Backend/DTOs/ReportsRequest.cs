namespace OpenLicenseApi.DTOs
{
    public class ReportDataDto
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public List<LicenseSummaryDto> LicenseSummaries { get; set; } = new();
        public List<ExpiringLicenseDto> ExpiringLicenses { get; set; } = new();
        public List<ExpiredLicenseDto> ExpiredLicenses { get; set; } = new();
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int SuspendedLicenses { get; set; }
        public int ExpiringCount { get; set; }
        public int ExpiredCount { get; set; }
    }

    public class LicenseSummaryDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalLicenses { get; set; }
        public int MaxLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int SuspendedLicenses { get; set; }
    }

    public class ExpiringLicenseDto
    {
        public string LicenseName { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class ExpiredLicenseDto
    {
        public string LicenseName { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int DaysExpired { get; set; }
    }
}

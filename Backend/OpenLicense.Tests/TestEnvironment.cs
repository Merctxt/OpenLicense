using System.Runtime.CompilerServices;

namespace OpenLicense.Tests;

internal static class TestEnvironment
{
    internal const string JwtSecret = "OpenLicense.Tests-only-signing-key-123456";
    internal const string JwtIssuer = "OpenLicenseApi";
    internal const string JwtAudience = "OpenLicenseApiUsers";
    internal const string DatabaseConnection = "Host=localhost;Database=OpenLicenseTests;Username=test;Password=test";

    [ModuleInitializer]
    internal static void Initialize()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", DatabaseConnection);
        Environment.SetEnvironmentVariable("Jwt__SecretKey", JwtSecret);
        Environment.SetEnvironmentVariable("Jwt__Issuer", JwtIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", JwtAudience);
        Environment.SetEnvironmentVariable("RegistrationEnabled", "true");
    }
}

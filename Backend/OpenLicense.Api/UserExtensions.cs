using System.Security.Claims;

namespace OpenLicense.Api;

public static class CurrentUserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (value is null)
        {
            throw new UnauthorizedAccessException("Invalid or missing user id in token.");
        }
        return Guid.Parse(value);
    }
}

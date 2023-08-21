using System.Security.Claims;

namespace FakeAuth.Shared;

public static class ClaimIdentityHelper
{
    public static string? GetUserId(this ClaimsIdentity identity)
    {
        return identity.getValueOrNull(ClaimTypes.NameIdentifier);
    }

    public static string? GetUserEmail(this ClaimsIdentity identity)
    {
        return identity.getValueOrNull(ClaimTypes.Email);
    }

    public static string? GetUserName(this ClaimsIdentity identity)
    {
        return identity.getValueOrNull(ClaimTypes.Name);
    }

    public static string? getValueOrNull(this ClaimsIdentity identity, string claimType)
    {
        return identity.FindFirst(claimType)?.Value;
    }
}
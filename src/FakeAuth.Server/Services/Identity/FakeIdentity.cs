using System.Security.Claims;
using FakeAuth.Shared;

namespace FakeAuth.Server.Services.Identity;

public class FakeIdentity
{
    private const string AnonymousName = "Anonymous";

    private const string AnonymousRole = "Anonymous";

    public string Name { get; set; } = AnonymousName;

    public string Role { get; set; } = AnonymousRole;

    public List<UserClaim> Claims { get; set; } = new();

    public ClaimsIdentity ToClaimsIdentity(string? authenticationType = null)
    {
        var claims = Claims.Select(x => x.ToClaim()).ToList();
        claims.Add(new Claim(ClaimTypes.Name, Name));
        claims.Add(new Claim(ClaimTypes.Role, Role));

        return new ClaimsIdentity(claims, authenticationType, Name, Role);
    }

    public bool IsAnonymous()
    {
        return Role == AnonymousRole;
    }

    public FakeIdentityDto.Index ToIndexDto()
    {
        return new FakeIdentityDto.Index(Name, Role);
    }
}

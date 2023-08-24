using System.Security.Claims;
using FakeAuth.Shared;

namespace FakeAuth.Server.Services.Identity;

public class FakeIdentity
{
    private const string _anonymousName = "Anonymous";

    private const string _anonymousRole = "Anonymous";

    public string Name { get; set; } = _anonymousName;

    public string Role { get; set; } = _anonymousRole;

    public List<UserClaim> Claims { get; set; } = new();

    public ClaimsIdentity ToClaimsIdentity(string? authenticationType = null)
    {
        var claims = Claims.Select(x => x.ToClaim()).ToList();
        claims.Add(new Claim(ClaimTypes.Name, Name));
        claims.Add(new Claim(ClaimTypes.Role, Role));

        return new ClaimsIdentity(claims, authenticationType);
    }

    public bool IsAnonymous()
    {
        return Role == _anonymousRole;
    }

    public FakeIdentityDto.Index ToIndexDto()
    {
        return new FakeIdentityDto.Index(Name, Role);
    }
}

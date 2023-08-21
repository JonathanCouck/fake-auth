using FakeAuth.Shared;

namespace FakeAuth.Server.Services;

using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;

public class FakeIdentityService
{
    public const string FakeIdentitiesProperty = "FakeIdentities";


    public List<Dictionary<string, string>> Identities;

    public FakeIdentityService(List<Dictionary<string, string>> identities)
    {
        Identities = identities;
        // Always add an anonymous identity by default
        CreateAnonymousIdentityIfNeeded();
    }

    public List<ClaimsIdentity> ToClaimsIdentities()
    {
        return Identities.Select(id => ToClaimsIdentity(id, Scheme.Name))
            .ToList();
    }

    public ClaimsIdentity ToClaimsIdentity(Dictionary<string, string> identity, string scheme)
    {
        var claims = identity.Select(pair =>
        {
            var key = pair.Key;
            var value = pair.Value;
            return pair.Key switch
            {
                "Identifier" => new Claim(ClaimTypes.NameIdentifier, value),
                "Name" => new Claim(ClaimTypes.Name, value),
                "Email" => new Claim(ClaimTypes.Email, value),
                "Role" => new Claim(ClaimTypes.Role, value),
                _ => new Claim(key, value)
            };
        });

        return new ClaimsIdentity(claims, scheme);
    }

    public static FakeIdentityService CreateFromConfiguration(IConfiguration configuration)
    {
        var sections = configuration.GetSection(FakeIdentitiesProperty).GetChildren();
        var identities = sections.Select(section => section.Get<Dictionary<string, string>>())
            .Where(sectionDictionary => sectionDictionary != null && sectionDictionary.Any())
            .ToList();


        return new FakeIdentityService(identities!);
    }

    public Dictionary<string, string>? FindIdentityForIdentifier(string identifier)
    {
        return Identities.Find(dictionary => dictionary["Identifier"] == identifier);
    }

    private void CreateAnonymousIdentityIfNeeded()
    {
        if (Identities.Exists(identity => identity["Name"].Equals("Anonymous"))) return;

        var anonymousIdentity = new Dictionary<string, string>
        {
            ["Name"] = "Anonymous",
            ["Identifier"] = "0",
            ["Role"] = "Anonymous"
        };

        Identities.Add(anonymousIdentity);
    }
}
namespace FakeAuth.Server.Services.Identity;

public class FakeIdentityService
{
    public readonly List<FakeIdentity> Identities;

    public FakeIdentityService(List<FakeIdentity> identities)
    {
        Identities = identities;
        // Always add an anonymous identity by default
        CreateAnonymousIdentityIfNeeded();
    }

    public FakeIdentity? FindIdentityForName(string name)
    {
        // We assume that identity name is case insensitive for ease of use purposes. This is fake auth after all
        return Identities.Find(identity => string.Equals(name, identity.Name, StringComparison.InvariantCultureIgnoreCase));
    }

    private void CreateAnonymousIdentityIfNeeded()
    {
        if (Identities.Exists(identity => identity.IsAnonymous())) return;
        Identities.Add(new FakeIdentity());
    }
}

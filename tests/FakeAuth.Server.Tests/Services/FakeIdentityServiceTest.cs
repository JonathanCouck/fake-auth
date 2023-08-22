using FakeAuth.Server.Services.Identity;
using Microsoft.Extensions.Configuration;

namespace FakeAuth.Server.Tests.Services;

public class FakeIdentityServiceTest
{
    private readonly IConfiguration _configuration;

    public FakeIdentityServiceTest()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "FakeIdentities:0:Name", "Administrator" },
            { "FakeIdentities:0:Role", "AdministratorRole" },
            { "FakeIdentities:0:Claims:0:Type", "Email" },
            { "FakeIdentities:0:Claims:0:Value", "fake-administrator@gmail.com" },
            { "FakeIdentities:0:Claims:1:Type", "Identifier" },
            { "FakeIdentities:0:Claims:1:Value", "1" },
            { "FakeIdentities:1:Name", "Customer" },
            { "FakeIdentities:1:Role", "CustomerRole" },
            { "FakeIdentities:1:Claims:0:Type", "Email" },
            { "FakeIdentities:1:Claims:0:Value", "fake-customer@gmail.com" },
            { "FakeIdentities:1:Claims:1:Type", "Identifier" },
            { "FakeIdentities:1:Claims:1:Value", "2" },
            { "FakeIdentities:1:Claims:2:Type", "RandomClaim" },
            { "FakeIdentities:1:Claims:2:Value", "RandomClaimValue" },
        };


        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void Test1()
    {
        var fakeIdentities = _configuration.GetSection("FakeIdentities").Get<List<FakeIdentity>>();

        Assert.Equal(2, fakeIdentities.Count);

        var secondIdentity = fakeIdentities[1];
        Assert.Equal(3, secondIdentity.Claims.Count);
        Assert.Equal("Customer", secondIdentity.Name);
        Assert.Equal("CustomerRole", secondIdentity.Role);

        Assert.True(secondIdentity.Claims.Exists(c => c is { Type: "Email", Value: "fake-customer@gmail.com" }));
        Assert.True(secondIdentity.Claims.Exists(c => c is { Type: "RandomClaim", Value: "RandomClaimValue" }));
    }
}

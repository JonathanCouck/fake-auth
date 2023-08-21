using FakeAuth.Server.Services;
using FakeAuth.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeAuth.Server.Extensions;

public static class FakeAuthServerExtensions
{
    public static void AddFakeAuthentication(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var fakeIdentities = FakeIdentityService.CreateFromConfiguration(configuration);
        builder.Services.AddSingleton(fakeIdentities);


        var jwtConfig = configuration.GetSection("JWT").Get<JWTConfig>();
        if (jwtConfig != null)
        {
            builder.Services.AddSingleton(jwtConfig);
        }

        // (Fake) Authentication
        builder.Services.AddAuthentication(Scheme.Name)
            .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(Scheme.Name, null);
    }
}
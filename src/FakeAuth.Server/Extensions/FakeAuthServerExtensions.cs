using System.Dynamic;
using FakeAuth.Server.Services;
using FakeAuth.Server.Services.Identity;
using FakeAuth.Server.Services.Token;
using FakeAuth.Server.Services.Token.JWT;
using FakeAuth.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeAuth.Server.Extensions;

public static class FakeAuthServerExtensions
{
    public static void AddFakeAuthentication<T, A>(this WebApplicationBuilder builder)
        where T : ITokenGeneratorService
        where A : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        var configuration = builder.Configuration;

        var identities = configuration.GetSection("FakeIdentities").Get<List<FakeIdentity>>();
        if (identities != null)
            builder.Services.AddSingleton(identities);

        builder.Services.AddSingleton<FakeIdentityService>();

        var jwtConfig = configuration.GetSection("JWT").Get<JwtConfig>();
        if (jwtConfig != null)
            builder.Services.AddSingleton(jwtConfig);

        builder.Services.AddSingleton<ITokenGeneratorService>(sp => sp.GetRequiredService<T>());

        // (Fake) Authentication
        builder.Services.AddAuthentication(Scheme.Name)
            .AddScheme<AuthenticationSchemeOptions, A>(Scheme.Name, null);
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FakeAuth.Server.Services;

public class FakeAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly FakeIdentityService fakeIdentityService;

    private readonly JWTConfig jwtConfig;

    public FakeAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        FakeIdentityService fakeIdentityService,
        JWTConfig jwtConfig
    ) : base(options, logger, encoder, clock)
    {
        this.fakeIdentityService = fakeIdentityService;
        this.jwtConfig = jwtConfig;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            return await Task.FromResult(AuthenticateResult.NoResult());

        var accessToken = authHeader.FirstOrDefault()?[7..];
        if (accessToken == null)
            return await Task.FromResult(AuthenticateResult.NoResult());

        var jwtHandler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(jwtConfig.GetKeyAsBytes());
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
        try
        {
            var claimsPrincipal = jwtHandler.ValidateToken(accessToken, tokenValidationParameters, out var validatedToken);
            // Token is valid
            var nameIdentifier = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var fakeIdentity = fakeIdentityService.FindIdentityForIdentifier(nameIdentifier);
            if (fakeIdentity == null)
                return await Task.FromResult(AuthenticateResult.NoResult());


            var identity = fakeIdentityService.ToClaimsIdentity(fakeIdentity, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (SecurityTokenException)
        {
            return await Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}
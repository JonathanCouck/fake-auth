using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using FakeAuth.Server.Services.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FakeAuth.Server.Services.Token.Basic;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly FakeIdentityService fakeIdentityService;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        FakeIdentityService fakeIdentityService
    ) : base(options, logger, encoder, clock)
    {
        this.fakeIdentityService = fakeIdentityService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            return await Task.FromResult(AuthenticateResult.NoResult());

        var encodedUsernamePassword = authHeader.FirstOrDefault()?[6..]?.Trim();
        if (encodedUsernamePassword == null)
            return await Task.FromResult(AuthenticateResult.NoResult());

        var decodedBytes = Convert.FromBase64String(encodedUsernamePassword);
        var usernamePassword = Encoding.UTF8.GetString(decodedBytes);

        var delimiterIndex = usernamePassword.IndexOf(':');
        if (delimiterIndex == -1) throw new InvalidOperationException("Invalid decoded authentication header");

        var username = usernamePassword.Substring(0, delimiterIndex);

        var fakeIdentity = fakeIdentityService.FindIdentityForName(username);
        if (fakeIdentity == null)
            return await Task.FromResult(AuthenticateResult.NoResult());

        var identity = fakeIdentity.ToClaimsIdentity(Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return await Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

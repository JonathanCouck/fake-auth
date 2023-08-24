using FakeAuth.Server.Services.Identity;
using FakeAuth.Server.Services.Token;
using FakeAuth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeAuth.Server.Controllers;

[ApiController]
[Route("api/fake-login")]
public class FakeLoginController : ControllerBase
{
    private readonly FakeIdentityService fakeIdentityService;

    private readonly ITokenGeneratorService tokenGeneratorService;

    public FakeLoginController(FakeIdentityService fakeIdentityService, ITokenGeneratorService tokenGeneratorService)
    {
        this.fakeIdentityService = fakeIdentityService;
        this.tokenGeneratorService = tokenGeneratorService;
    }

    [HttpGet("identities")]
    [AllowAnonymous]
    public IEnumerable<FakeIdentityDto.Index> GetIdentities()
    {
        return fakeIdentityService.Identities.Select(identity => identity.ToIndexDto());
    }

    [HttpPost("login/{name}")]
    [AllowAnonymous]
    public IActionResult Login(string name)
    {
        var fakeIdentity = fakeIdentityService.FindIdentityForName(name);
        if (fakeIdentity == null) return Unauthorized();

        var token = tokenGeneratorService.GenerateToken(fakeIdentity);

        var loginObject = new FakeIdentityDto.Credentials(
            token.TokenString,
            token.TokenType,
            token.Duration,
            token.IssuedFor.Name,
            token.IssuedFor.Role
        );

        return Ok(loginObject);
    }
}

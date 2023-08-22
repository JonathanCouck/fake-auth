using System.IdentityModel.Tokens.Jwt;
using FakeAuth.Server.Extensions;
using FakeAuth.Server.Services;
using FakeAuth.Server.Services.Identity;
using FakeAuth.Server.Services.Token;
using FakeAuth.Server.Services.Token.JWT;
using FakeAuth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FakeAuth.Server.Controllers;

// using Swashbuckle.AspNetCore.Annotations;

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

    // [SwaggerOperation("Returns a list of products available in the bogus catalog.")]
    [HttpGet("identities"), AllowAnonymous]
    public IEnumerable<FakeIdentityDto.Index> GetIdentities()
    {
        return fakeIdentityService.Identities.Select(identity => identity.ToIndexDto());
    }

    [HttpGet("identities/me"), Authorize(AuthenticationSchemes = Scheme.Name)]
    public IActionResult GetIdentity()
    {
        var user = HttpContext.User;
        var identifier = user.Identities.First(identity => identity.AuthenticationType == Scheme.Name).Name;

        if (identifier == null) return NotFound();

        var fakeIdentity = fakeIdentityService.FindIdentityForName(identifier);
        if (fakeIdentity == null) return NotFound();

        return Ok(fakeIdentity);
    }

    [HttpPost("login/{identifier}"), AllowAnonymous]
    public IActionResult Login(string identifier)
    {
        var fakeIdentity = fakeIdentityService.FindIdentityForName(identifier);
        if (fakeIdentity == null)
        {
            return Unauthorized();
        }

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
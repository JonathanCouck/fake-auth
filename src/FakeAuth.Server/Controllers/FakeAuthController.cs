using System.IdentityModel.Tokens.Jwt;
using FakeAuth.Server.Extensions;
using FakeAuth.Server.Services;
using FakeAuth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FakeAuth.Server.Controllers;

// using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/fake-login")]
public class FakeLoginController2 : ControllerBase
{
    private readonly FakeIdentityService fakeIdentityService;

    private readonly JWTConfig jwtConfig;

    public FakeLoginController2(FakeIdentityService fakeIdentityService, JWTConfig jwtConfig)
    {
        this.fakeIdentityService = fakeIdentityService;
        this.jwtConfig = jwtConfig;
    }

    // [SwaggerOperation("Returns a list of products available in the bogus catalog.")]
    [HttpGet("identities"), AllowAnonymous]
    public IEnumerable<FakeIdentityDto.Index> GetIdentities()
    {
        return fakeIdentityService.ToClaimsIdentities().Select(FakeIdentityDto.Index.FromClaimsIdentity);
    }

    [HttpGet("identities/me"), Authorize(AuthenticationSchemes = Scheme.Name)]
    public IActionResult GetIdentity()
    {
        var user = HttpContext.User;
        var identifier = user.Identities.First().GetUserId();

        if (identifier == null)
        {
            return NotFound();
        }

        var fakeIdentity = fakeIdentityService.FindIdentityForIdentifier(identifier);
        if (fakeIdentity == null)
        {
            return NotFound();
        }

        return Ok(fakeIdentityService.FindIdentityForIdentifier(identifier));
    }

    [HttpPost("login/{identifier}"), AllowAnonymous]
    public IActionResult Login(string identifier)
    {
        var fakeIdentity = fakeIdentityService.FindIdentityForIdentifier(identifier);
        if (fakeIdentity == null)
        {
            return Unauthorized();
        }

        var issuer = jwtConfig.Issuer;
        var audience = jwtConfig.Audience;
        var durationInSeconds = jwtConfig.Duration;
        var key = jwtConfig.GetKeyAsBytes();
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = fakeIdentityService.ToClaimsIdentity(fakeIdentity, Scheme.Name),
            Expires = DateTime.UtcNow.AddSeconds(durationInSeconds),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = signingCredentials
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var stringToken = tokenHandler.WriteToken(token);

        var loginObject = new FakeIdentityDto.Login(
            stringToken,
            "Bearer",
            durationInSeconds,
            identifier,
            // TODO Update
            fakeIdentity["Name"],
            fakeIdentity["Role"]
        );

        return Ok(loginObject);
    }
}
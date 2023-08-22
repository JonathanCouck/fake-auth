using System.IdentityModel.Tokens.Jwt;
using FakeAuth.Server.Services.Identity;
using Microsoft.IdentityModel.Tokens;

namespace FakeAuth.Server.Services.Token.JWT;

public class JwtTokenGeneratorService : ITokenGeneratorService
{
    private readonly JwtConfig jwtConfig;

    public JwtTokenGeneratorService(JwtConfig jwtConfig)
    {
        this.jwtConfig = jwtConfig;
    }

    public Token GenerateToken(FakeIdentity identity)
    {
        var issuer = jwtConfig.Issuer;
        var audience = jwtConfig.Audience;
        var durationInSeconds = jwtConfig.Duration;
        var key = jwtConfig.GetKeyAsBytes();
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity.ToClaimsIdentity(),
            Expires = DateTime.UtcNow.AddSeconds(durationInSeconds),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = signingCredentials
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        var tokenString = tokenHandler.WriteToken(token);

        return new Token(tokenString, durationInSeconds, identity, "Bearer");
    }
}

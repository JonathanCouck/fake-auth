using System.Security.Claims;

namespace FakeAuth.Shared;

public abstract class FakeIdentityDto
{
    public const string AnonymousRole = "Anonymous";

    public class Index
    {
        public Index(string name, string role)
        {
            Name = name;
            Role = role;
        }

        public string Name { get; set; }

        public string Role { get; set; }

        public bool IsAnonymous()
        {
            return Role == AnonymousRole;
        }
    }

    public class Credentials
    {
        public Credentials(string accessToken, string tokenType, int expiresIn, string name, string role)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
            Name = name;
            Role = role;
        }

        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public ClaimsIdentity ToClaimsIdentity(string? authenticationType = null)
        {
            return new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, Name),
                new Claim(ClaimTypes.Role, Role),
                new Claim("AccessToken", AccessToken),
                new Claim("TokenType", TokenType),
                new Claim("ExpiresIn", ExpiresIn.ToString())
            }, authenticationType);
        }
    }
}

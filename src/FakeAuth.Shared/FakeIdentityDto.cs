using System.Security.Claims;

namespace FakeAuth.Shared;


public abstract class FakeIdentityDto
{
    public class Index
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Index(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public static Index FromClaimsIdentity(ClaimsIdentity identity)
        {
            var id = identity.GetUserId();
            var name = identity.GetUserName();

            if (id != null && name != null)
            {
                return new Index(id, name);
            }

            throw new InvalidOperationException("Unable to create FakeIdentityDto. Either 'id' or 'name' was null");
        }
    }

    public class Login
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public Login(string accessToken, string tokenType, int expiresIn, string identifier, string name, string role)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
            Identifier = identifier;
            Name = name;
            Role = role;
        }
    }
}
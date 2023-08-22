using System.Text;
using FakeAuth.Server.Services.Identity;

namespace FakeAuth.Server.Services.Token.Header;

public class BasicTokenGeneratorService : ITokenGeneratorService
{
    public Token GenerateToken(FakeIdentity identity)
    {
        var username = identity.Name;
        var credentials = $"{username}:";
        var credentialsBytes = Encoding.UTF8.GetBytes(credentials);
        var base64Credentials = Convert.ToBase64String(credentialsBytes);

        return new Token(
            base64Credentials,
            int.MaxValue,
            identity,
            "Basic"
        );


        throw new NotImplementedException();
    }
}

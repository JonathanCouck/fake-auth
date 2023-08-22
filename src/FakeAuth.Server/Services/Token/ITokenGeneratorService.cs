using FakeAuth.Server.Services.Identity;

namespace FakeAuth.Server.Services.Token;

public interface ITokenGeneratorService
{
    public Token GenerateToken(FakeIdentity identity);
}

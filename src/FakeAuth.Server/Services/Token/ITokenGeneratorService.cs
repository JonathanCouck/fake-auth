using FakeAuth.Server.Services.Identity;
using Microsoft.AspNetCore.Authentication;

namespace FakeAuth.Server.Services.Token;

public interface ITokenGeneratorService
{
    public Token GenerateToken(FakeIdentity identity);
}
using FakeAuth.Server.Services.Identity;

namespace FakeAuth.Server.Services.Token;

public class Token
{
    public Token(string tokenString, int duration, FakeIdentity issuedFor, string tokenType)
    {
        TokenString = tokenString;
        Duration = duration;
        IssuedFor = issuedFor;
        TokenType = tokenType;
    }

    public string TokenString { get; set; }

    public int Duration { get; set; }

    public FakeIdentity IssuedFor { get; set; }

    public string TokenType { get; set; }
}

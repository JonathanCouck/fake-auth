using System.Security.Claims;

namespace FakeAuth.Server.Services.Identity;

public class UserClaim
{
    public string Type { get; set; }
    public string Value { get; set; }

    public Claim ToClaim()
    {
        return Type switch
        {
            "Identifier" => new Claim(ClaimTypes.NameIdentifier, Value),
            "Name" => new Claim(ClaimTypes.Name, Value),
            "Email" => new Claim(ClaimTypes.Email, Value),
            "Role" => new Claim(ClaimTypes.Role, Value),
            _ => new Claim(Type, Value)
        };
    }
}
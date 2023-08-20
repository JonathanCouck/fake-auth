using BogusStore.Shared.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace BogusStore.Server.Authentication
{
    public enum AuthenticationMethod
    {
        Jwt,
    }

    public class FakeAuthSchemeOptions: AuthenticationSchemeOptions
    {
        private IEnumerable<ClaimsIdentity> _personas = new List<ClaimsIdentity>
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "0"),
                new Claim(ClaimTypes.Name, "Anonymous"),
                new Claim(ClaimTypes.Role, Roles.Anonymous),
            }),
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Administrator"),
                new Claim(ClaimTypes.Role, Roles.Administrator),
                new Claim(ClaimTypes.Role, Roles.Customer),
            }),
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "2"),
                new Claim(ClaimTypes.Name, "Customer"),
                new Claim(ClaimTypes.Role, Roles.Customer),
            }),
        };
        public IEnumerable<ClaimsIdentity> Personas 
        { 
            get => _personas; 
            set
            {
                foreach (var persona in value)
                {
                    // Check if every persona has a Name, Role and NameIdentifier
                    if (persona.FindAll(ClaimTypes.NameIdentifier).IsNullOrEmpty() || persona.FindAll(ClaimTypes.Name).IsNullOrEmpty() || persona.FindAll(ClaimTypes.Role).IsNullOrEmpty())
                    {
                        throw new FakeAuthPersonaException("Persona should contain at least one claim of type ClaimTypes.Name and one of type ClaimTypes.Role");
                    }
                }
                _personas = value;
                Console.WriteLine();
            }
        }

        private AuthenticationMethod _authenticationMethod = AuthenticationMethod.Jwt;
        public AuthenticationMethod AuthenticationMethod
        {
            get => _authenticationMethod;
            set
            {
                _authenticationMethod = value;
            }
        }

        public FakeAuthSchemeOptions(): base() { }
    }
}

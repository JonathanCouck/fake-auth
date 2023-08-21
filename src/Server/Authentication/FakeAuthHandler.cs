using BogusStore.Shared.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace BogusStore.Server.Authentication
{
	public class FakeAuthHandler: AuthenticationHandler<FakeAuthSchemeOptions>
    {
        private static IEnumerable<ClaimsIdentity> _personas = new List<ClaimsIdentity>
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
        public FakeAuthHandler(
			IOptionsMonitor<FakeAuthSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock)
        : base(options, logger, encoder, clock) { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
			List<Claim> claims = new();

            if (Context.Request.Headers.TryGetValue("Authorization", out var authToken))
            {
                var authTokenNoBearer = authToken.ToString().Substring("Bearer ".Length);
                IEnumerable<Claim> newClaims = new JwtSecurityTokenHandler().ReadJwtToken(authTokenNoBearer).Claims;
                claims.AddRange(newClaims);
            }
            
            if (claims.Any())
            {
                var identity = new ClaimsIdentity(new[]
				{
					new Claim(ClaimTypes.Role, Roles.Administrator)
				}, "Fake Authentication");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Fake Authentication");
                return await Task.FromResult(AuthenticateResult.Success(ticket));
			}

			return await Task.FromResult(AuthenticateResult.NoResult());
        }

		public static void MapAuthenticationRoutes(WebApplicationBuilder builder, WebApplication app)
		{
            // Route for getting an array of all persona names
            app.MapGet("/api/security/personas",
            [HttpGet, AllowAnonymous] () =>
            {
                return Results.Ok(_personas
					.SelectMany(a => a.Claims.Where(c => c.Type == ClaimTypes.Name && !string.IsNullOrEmpty(c.Value)))
					.Select(c => c.Value)
					.ToList());
            });

			// Route for creating a token given the persona name
			app.MapGet("/api/security/createToken",
			[HttpGet, AllowAnonymous] ([FromQuery] string personaName) =>
			{
				ClaimsIdentity? personaWithMatchingName = _personas.FirstOrDefault(a =>
					a.FindFirst(ClaimTypes.Name)?.Value == personaName);
				if (personaWithMatchingName != null)
				{
					var issuer = builder.Configuration["Jwt:Issuer"];
					var audience = builder.Configuration["Jwt:Audience"];
					var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
					var claims = personaWithMatchingName.Claims.ToArray();
					
					var tokenDescriptor = new SecurityTokenDescriptor
					{
						Subject = new ClaimsIdentity(claims, "Fake Authentication"),
						Expires = DateTime.UtcNow.AddMinutes(5),
						Issuer = issuer,
						Audience = audience,
						SigningCredentials = new SigningCredentials
						(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
					};
                    var tokenHandler = new JwtSecurityTokenHandler();
                    tokenHandler.InboundClaimTypeMap.Clear();
					
                    var token = tokenHandler.CreateToken(tokenDescriptor);
					var stringToken = tokenHandler.WriteToken(token);
					return Results.Ok(stringToken);
				}

				return Results.BadRequest($"The persona with name {personaName} wasn't found");
			});
		}

        private class PersonaNamesResult
		{
			public List<string> PersonaNames { get; set; } = default!;
			public PersonaNamesResult(List<string> actors)
			{
				PersonaNames = actors;
			}
		}
		private class TokenRequest
		{
			public string PersonaToken { get; set; } = default!;
		}
	}
}

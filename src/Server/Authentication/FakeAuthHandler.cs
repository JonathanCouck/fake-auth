using Azure.Core;
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
	public class FakeAuthHandler: AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private static IEnumerable<ClaimsIdentity> _actors => new List<ClaimsIdentity>
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Actor, "Anonymous"),
            }),
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Actor, "Administrator"),
                new Claim(ClaimTypes.Role, Roles.Administrator),
            }, "Fake Authentication"),
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "2"),
                new Claim(ClaimTypes.Actor, "Customer"),
                new Claim(ClaimTypes.Role, Roles.Customer),
            }, "Fake Authentication")
        };

        private static List<string> _actorNames = _actors
            .SelectMany(a => a.Claims.Where(c => c.Type == ClaimTypes.Actor && !string.IsNullOrEmpty(c.Value)))
            .Select(c => c.Value)
            .ToList();

        public FakeAuthHandler(
          IOptionsMonitor<AuthenticationSchemeOptions> options,
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
                Console.WriteLine();
            }

            if (Context.Request.Headers.TryGetValue("UserId", out var userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId!));
            }

            if (Context.Request.Headers.TryGetValue("Role", out var roles))
            {
                claims.Add(new Claim(ClaimTypes.Role, roles!));
            }

            if (Context.Request.Headers.TryGetValue("Email", out var email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email!));
            }

            if (Context.Request.Headers.TryGetValue("Name", out var name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name!));
            }

            if (claims.Any())
            {
                var identity = new ClaimsIdentity(claims, "Fake Authentication");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Fake Authentication");
                return await Task.FromResult(AuthenticateResult.Success(ticket));
			}

			return await Task.FromResult(AuthenticateResult.NoResult());
        }

		public static void MapAuthenticationRoutes(WebApplicationBuilder builder, WebApplication app)
		{
			// Route for getting an array of all actor names
			app.MapGet("/api/security/actors",
			[HttpGet, AllowAnonymous] () =>
			{
				return Results.Ok(_actorNames);
			});

			// Route for creating a token given the actor
			app.MapGet("/api/security/createToken",
			[HttpGet, AllowAnonymous] ([FromQuery] string actorName) =>
			{
				ClaimsIdentity? actorWithMatchingClaim = _actors.FirstOrDefault(a =>
					a.FindFirst(ClaimTypes.Actor)?.Value == actorName);
				if (actorWithMatchingClaim != null)
				{
					var issuer = builder.Configuration["Jwt:Issuer"];
					var audience = builder.Configuration["Jwt:Audience"];
					var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
					var claims = actorWithMatchingClaim.Claims.ToArray();
					var tokenDescriptor = new SecurityTokenDescriptor
					{
						Subject = new ClaimsIdentity(claims),
						Expires = DateTime.UtcNow.AddMinutes(5),
						Issuer = issuer,
						Audience = audience,
						SigningCredentials = new SigningCredentials
						(new SymmetricSecurityKey(key),
						SecurityAlgorithms.HmacSha512Signature)
					};
					var tokenHandler = new JwtSecurityTokenHandler();
					var token = tokenHandler.CreateToken(tokenDescriptor);
					var stringToken = tokenHandler.WriteToken(token);
					return Results.Ok(stringToken);
				}

				return Results.BadRequest("The actor wasn't found");
			});
		}

        private class ActorsResult
		{
			public List<string> Actors { get; set; } = default!;
			public ActorsResult(List<string> actors)
			{
				Actors = actors;
			}
		}
		private class TokenRequest
		{
			public string Actor { get; set; } = default!;
		}
	}
}

using Azure.Core;
using BogusStore.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BogusStore.Server.Authentication
{
	public class FakeAuth
	{
		public static IEnumerable<ClaimsIdentity> Actors => new List<ClaimsIdentity>
		{
			new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.Actor, "Anonymous"),
			}),
			new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, "1"),
				new Claim(ClaimTypes.Actor, "Administrator"),
				new Claim(ClaimTypes.Email, "fake-administrator@gmail.com"),
				new Claim(ClaimTypes.Role, Roles.Administrator),
			}),
			new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, "2"),
				new Claim(ClaimTypes.Actor, "Customer"),
				new Claim(ClaimTypes.Email, "fake-customer@gmail.com"),
				new Claim(ClaimTypes.Role, Roles.Customer),
			})
		};
		private static List<string> _actorNames = Actors
			.SelectMany(a => a.Claims.Where(c => c.Type == ClaimTypes.Actor && !string.IsNullOrEmpty(c.Value)))
			.Select(c => c.Value)
			.ToList();

		public static void MapAuthenticationRoutes(WebApplicationBuilder builder, WebApplication app)
		{
			// Route for getting an array of all actor names
			app.MapGet("/api/security/actors",
			[AllowAnonymous] () =>
			{
				return Results.Ok(_actorNames);
			});

			// Route for creating a token given the actor
			app.MapPost("/api/security/createToken",
			[AllowAnonymous] ([FromQuery] string actorName) =>
			{
				ClaimsIdentity? actorWithMatchingClaim = Actors.FirstOrDefault(a =>
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
					var jwtToken = tokenHandler.WriteToken(token);
					var stringToken = tokenHandler.WriteToken(token);
					return Results.Ok(new TokenResult(stringToken));
				}

				return Results.BadRequest();
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
        private class TokenResult
        {
            public string Token { get; set; } = default!;
            public TokenResult(string token)
            {
                Token = token;
            }
        }
	}
}

using BogusStore.Domain.Users;
using BogusStore.Shared.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BogusStore.Server.Authentication
{
    public class FakeAuthentication
	{
		public static IEnumerable<ClaimsIdentity> Users => new List<ClaimsIdentity>
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

		public static void AddAuthentication(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true
                };
            });
            builder.Services.AddAuthorization();
        }

        public static void MapAuthenticationRoutes(WebApplicationBuilder builder, WebApplication app)
        {
            app.MapGet("/api/security/actors", 
            [AllowAnonymous] (Request req) =>
            {
                List<string> actors = Users
	                .SelectMany(u => u.Claims.Where(c => c.Type == ClaimTypes.Actor && !string.IsNullOrEmpty(c.Value)))
	                .Select(c => c.Value)
	                .ToList();

                return Results.Ok(new ActorsResult(actors));
			});

            app.MapPost("/api/security/createToken",
            [AllowAnonymous] (Request reqParam) =>
            {
                if (reqParam.Name == "Regular User")
                {
                    var issuer = builder.Configuration["Jwt:Issuer"];
                    var audience = builder.Configuration["Jwt:Audience"];
                    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
                    var claims = new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, Roles.Administrator),
                        new Claim(JwtRegisteredClaimNames.Name, reqParam.Name),
                        new Claim(JwtRegisteredClaimNames.Email, "jonathan.couck@student.hogent.be"),
                        new Claim(JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString())
                    };
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
                return Results.Unauthorized();
            });

            app.UseAuthentication();
            app.UseAuthorization();
        }

        private class Request
        {
            public string Name { get; set; } = default!;
        }

        private class ActorsResult
        {
            public List<string> Actors { get; set; } = default!;
			public ActorsResult(List<string> actors)
			{
				Actors = actors;
			}
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

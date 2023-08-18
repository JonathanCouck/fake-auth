using BogusStore.Domain.Users;
using BogusStore.Shared.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BogusStore.Server.Helper
{
	public class JwtHelper
	{
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

		public static void MapTokenPost(WebApplicationBuilder builder, WebApplication app)
		{
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
					return Results.Ok(new Result(stringToken));
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

		private class Result
		{
			public string Token { get; set; } = default!;
			public Result(string token)
			{
				Token = token;
			}
		}
	}
}

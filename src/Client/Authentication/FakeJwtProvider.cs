using BogusStore.Shared.Authentication;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace BogusStore.Client.Authentication
{
	public class FakeJwtProvider
	{
		public static ClaimsPrincipal Anonymous => new(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.Name, "Anonymous"),
		}));

		public static ClaimsPrincipal Administrator => new(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, "1"),
			new Claim(ClaimTypes.Name, "Administrator"),
			new Claim(ClaimTypes.Email, "fake-administrator@gmail.com"),
			new Claim(ClaimTypes.Role, Roles.Administrator),
		}, "Fake Authentication"));

		public static ClaimsPrincipal Customer => new(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, "2"),
			new Claim(ClaimTypes.Name, "Customer"),
			new Claim(ClaimTypes.Email, "fake-customer@gmail.com"),
			new Claim(ClaimTypes.Role, Roles.Customer),
		}, "Fake Authentication"));

		public static IEnumerable<ClaimsPrincipal> ClaimsPrincipals =>
			new List<ClaimsPrincipal>() { Anonymous, Customer, Administrator };

		public ClaimsPrincipal Current { get; private set; } = Administrator;

		public static string CreateJwtToken()
		{
			var issuer = "com.couck.jonathan";
			var audience = "*";
			var key = Encoding.ASCII.GetBytes("S3cretK3y");
			var claims = new[]
			{
				new Claim("Id", Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Sub, Roles.Administrator),
				new Claim(JwtRegisteredClaimNames.Name, "Jonathan Couck"),
				new Claim(JwtRegisteredClaimNames.Email, "jonathan.couck@student.hogent.be"),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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

			Console.WriteLine(stringToken);
			return stringToken;
		}
	}
}

using System.Net.Http.Json;
using System.Security.Claims;
using FakeAuth.Shared;
using Microsoft.AspNetCore.Components.Authorization;

namespace FakeAuth.Client.Authentication;

public class FakeAuthenticationProvider : AuthenticationStateProvider
{
    private const string Endpoint = "api/fake-login";

    private readonly Lazy<HttpClient> client;

    public FakeAuthenticationProvider(IHttpClientFactory clientFactory, string httpClientName)
    {
        // Issues with initializing HTTPClient via DI 
        // Manually lazy loading
        // See https://github.com/dotnet/aspnetcore/issues/33787
        client = new Lazy<HttpClient>(() => clientFactory.CreateClient(httpClientName));
    }

    public static ClaimsPrincipal Anonymous => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, "Anonymous"),
        new Claim(ClaimTypes.NameIdentifier, "0"),
    }));

    public IEnumerable<ClaimsPrincipal> ClaimsPrincipals { get; set; } = new[] { Anonymous };
    public ClaimsPrincipal Current { get; private set; } = Anonymous;


    public async Task<List<ClaimsPrincipal>> GetIdentities()
    {
        var response = await client.Value.GetFromJsonAsync<List<FakeIdentityDto.Index>>($"{Endpoint}/identities");

        var claimsPrincipals = response.Select(dto =>
            {
                return new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, dto.Id),
                    new Claim(ClaimTypes.Name, dto.Name),
                }, Scheme.Name));
            }
        ).ToList();

        ClaimsPrincipals = claimsPrincipals;

        return claimsPrincipals;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var idClaim = Current.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{Endpoint}/login/{idClaim.Value}");
        HttpResponseMessage response = await client.Value.SendAsync(request);

        var responseBody = await response.Content.ReadFromJsonAsync<FakeIdentityDto.Login>();
        var newClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, responseBody.Identifier),
            new Claim(ClaimTypes.Name, responseBody.Name),
            new Claim(ClaimTypes.Role, responseBody.Role),
            new Claim("AccessToken", responseBody.AccessToken),
            new Claim("TokenType", responseBody.TokenType),
            new Claim("ExpiresIn", responseBody.ExpiresIn.ToString())
        }, Scheme.Name));

        Current = newClaimsPrincipal;

        return new AuthenticationState(newClaimsPrincipal);
    }

    public void ChangeAuthenticationState(ClaimsPrincipal claimsPrincipal)
    {
        Current = claimsPrincipal;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
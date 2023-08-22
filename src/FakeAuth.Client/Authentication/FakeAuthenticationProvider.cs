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

        FakeIdentities = new List<FakeIdentityDto.Index>();
    }

    public IEnumerable<FakeIdentityDto.Index> FakeIdentities { get; set; }

    public FakeIdentityDto.Credentials? CurrentCredentials { get; private set; }

    public FakeIdentityDto.Index? CurrentIdentity { get; private set; }


    public async Task<List<FakeIdentityDto.Index>?> GetIdentities()
    {
        var response = await client.Value.GetFromJsonAsync<List<FakeIdentityDto.Index>>($"{Endpoint}/identities");

        if (response == null) return response;

        FakeIdentities = response;
        if (CurrentIdentity != null) return response;

        // If no CurrentIdentity is set, set it to a user with an anonymous role
        // A user with anonymous role is always set through the backend
        CurrentIdentity = FakeIdentities.First(identity => identity.IsAnonymous());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return response;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var name = CurrentIdentity?.Name;
        // If name is null, CurrentIdentity is probably not set yet because the /identities call hasn't happened.
        // Creating an AuthenticationState with an empty principal
        if (name == null) return new AuthenticationState(new ClaimsPrincipal());

        var request = new HttpRequestMessage(HttpMethod.Post, $"{Endpoint}/login/{name}");
        var response = await client.Value.SendAsync(request);

        CurrentCredentials = await response.Content.ReadFromJsonAsync<FakeIdentityDto.Credentials>();
        var claimsIdentity = CurrentCredentials!.ToClaimsIdentity();
        return new AuthenticationState(new ClaimsPrincipal(claimsIdentity));
    }

    public void ChangeAuthenticationState(FakeIdentityDto.Index identity)
    {
        CurrentIdentity = identity;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

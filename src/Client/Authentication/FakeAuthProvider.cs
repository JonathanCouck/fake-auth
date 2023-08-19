using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BogusStore.Client.Authentication
{
    public class FakeAuthProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;

        public string[] ActorNames = default!;
        public Actor Current { get; private set; } = default!;

        public FakeAuthProvider(HttpClient http)
        {
            _http = http;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (Current != null)
            {
                Console.WriteLine(Current.ActorName);
                return Task.FromResult(new AuthenticationState(new(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, Current.ActorName),
                }, "Fake Authentication"))));
            }
            Console.WriteLine("No current");
            return Task.FromResult(new AuthenticationState(new(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Name, "Anonymous")
            }, "Fake Authentication"))));
        }

        public async Task SetActorNamesAsync()
        {
            HttpResponseMessage response = await _http.GetAsync("https://localhost:5001/api/security/actors");
            if (response.IsSuccessStatusCode)
            {
                ActorNames = await response.Content.ReadFromJsonAsync<string[]>();
                await ChangeAuthenticationStateAsync("Customer");
            }
        }

        public async Task ChangeAuthenticationStateAsync(string actorName)
        {
            HttpResponseMessage response = await _http.GetAsync($"https://localhost:5001/api/security/createToken?actorName={actorName}");
            if (response.IsSuccessStatusCode)
            {
                string token = await response.Content.ReadFromJsonAsync<string>();
                if (token != null)
                {
                    Current = new(actorName, token);
                }
            }
        }
    }

    public class Actor
    {
        public string ActorName { get; set; } = default!;
        public string Token { get; set; } = default!;
        public Actor(string actorName, string token)
        {
            ActorName = actorName;
            Token = token;
        }
    }
}

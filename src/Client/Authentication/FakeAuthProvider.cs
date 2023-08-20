using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BogusStore.Client.Authentication
{
    public class FakeAuthProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;

        public string[] PersonaNames = default!;
        public Persona Current { get; private set; } = default!;

        public FakeAuthProvider(HttpClient http)
        {
            _http = http;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (Current != null)
            {
                return Task.FromResult(new AuthenticationState(new(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, Current.Name),
                }, "Fake Authentication"))));
            }
            return Task.FromResult(new AuthenticationState(new(new ClaimsIdentity(new Claim[] {}, "Fake Authentication"))));
        }

        public async Task SetPersonaNamesAsync()
        {
            HttpResponseMessage response = await _http.GetAsync("https://localhost:5001/api/security/personas");
            if (response.IsSuccessStatusCode)
            {
                PersonaNames = await response.Content.ReadFromJsonAsync<string[]>();
                await ChangeAuthenticationStateAsync("Customer");
            }
        }

        public async Task ChangeAuthenticationStateAsync(string name)
        {
            HttpResponseMessage response = await _http.GetAsync($"https://localhost:5001/api/security/createToken?personaName={name}");
            if (response.IsSuccessStatusCode)
            {
                string personaToken = await response.Content.ReadFromJsonAsync<string>();
                if (personaToken != null)
                {
                    Current = new(name, personaToken);
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                }
            }
        }
    }

    public class Persona
    {
        public string Name { get; set; } = default!;
        public string Token { get; set; } = default!;
        public Persona(string name, string token)
        {
            Name = name;
            Token = token;
        }
    }
}

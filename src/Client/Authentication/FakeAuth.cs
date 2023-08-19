using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace BogusStore.Client.Authentication
{
    public class FakeAuth : AuthenticationStateProvider
    {
        public string[] ActorNames = default!;
        public Actor Current { get; private set; } = default!;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            throw new NotImplementedException();
        }

        public async Task SetActorNamesAsync(HttpClient http)
        {
            HttpResponseMessage response = await http.GetAsync("https://localhost:5001/api/security/actors");
            if (response.IsSuccessStatusCode)
            {
                ActorNames = await response.Content.ReadFromJsonAsync<string[]>();
                await ChangeAuthenticationState(http, ActorNames[0]);
            }
        }

        public async Task ChangeAuthenticationState(HttpClient http, string actorName)
        {
            HttpResponseMessage response = await http.GetAsync($"https://localhost:5001/api/security/createToken?actorName={actorName}");
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

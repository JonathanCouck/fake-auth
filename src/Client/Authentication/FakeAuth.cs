using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace BogusStore.Client.Authentication
{
    public class FakeAuth : AuthenticationStateProvider
    {
        public string[] actorNames = default!;
        public Actor Current { get; private set; } = default!;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            throw new NotImplementedException();
        }

        public async Task ChangeAuthenticationState(HttpClient http, string actorName)
        {
            HttpResponseMessage response = await http.GetAsync($"https://localhost:5001/api/security/createToken?actorName={actorName}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            //Current = await response.Content.ReadFromJsonAsync<Actor>();
        }

        public async Task InitActorNamesAsync(HttpClient http)
        {
            HttpResponseMessage response = await http.GetAsync("https://localhost:5001/api/security/actors");
            if (response.IsSuccessStatusCode)
            {
                actorNames = await response.Content.ReadFromJsonAsync<string[]>();
                ChangeAuthenticationState(http, actorNames[0]);
            }
        }
    }

    public class Actor
    {
        public string ActorName { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}

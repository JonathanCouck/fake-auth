using System;
using System.Security.Claims;
using BogusStore.Client.Authentication;
using Microsoft.AspNetCore.Components;

namespace BogusStore.Client.Layout;

public partial class AccessControl
{
    [Inject] public FakeAuthProvider TempFakeAuth { get; set; } = default!;
    [Inject] public HttpClient HttpClient { get; set; } = default!;

    public string[] ActorNames => TempFakeAuth.ActorNames;

    protected override async Task OnInitializedAsync()
    {
        await TempFakeAuth.SetActorNamesAsync();
    }
    
    private string? IsActive(string name)
    {
        return TempFakeAuth.Current.ActorName == name ? "is-active" : null;
    }

    private async Task ChangePrincipal(string name)
    {
        await TempFakeAuth.ChangeAuthenticationStateAsync(name);
    }
}


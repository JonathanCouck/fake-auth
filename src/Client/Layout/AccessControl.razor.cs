using System;
using System.Security.Claims;
using BogusStore.Client.Authentication;
using Microsoft.AspNetCore.Components;

namespace BogusStore.Client.Layout;

public partial class AccessControl
{
    [Inject] public FakeAuthenticationProvider FakeAuthenticationProvider { get; set; } = default!;
    [Inject] public FakeAuth TempFakeAuth { get; set; } = default!;
    [Inject] public HttpClient HttpClient { get; set; } = default!;

    public string[] ActorNames => TempFakeAuth.ActorNames;

    protected override async Task OnInitializedAsync()
    {
        await TempFakeAuth.SetActorNamesAsync(HttpClient);
    }

    private string? IsActive(string name)
    {
        return TempFakeAuth.Current.ActorName == name ? "is-active" : null;
    }

    private async Task ChangePrincipal(string name)
    {
        Console.WriteLine(name);
        // await TempFakeAuth.GetAuthenticationStateAsync(HttpClient, name);
    }
}


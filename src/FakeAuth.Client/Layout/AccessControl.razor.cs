using System.Security.Claims;
using FakeAuth.Client.Authentication;
using FakeAuth.Shared;
using Microsoft.AspNetCore.Components;

namespace FakeAuth.Client.Layout;

public partial class AccessControl
{
    [Inject] public FakeAuthenticationProvider FakeAuthenticationProvider { get; set; } = default!;

    private string? IsActive(FakeIdentityDto.Index identity)
    {
        return FakeAuthenticationProvider.CurrentIdentity?.Name == identity.Name ? "is-active" : null;
    }

    private void ChangeIdentity(FakeIdentityDto.Index identity)
    {
        FakeAuthenticationProvider.ChangeAuthenticationState(identity);
    }

    protected override Task OnParametersSetAsync()
    {
        return FakeAuthenticationProvider.GetIdentities();
    }
}
using BogusStore.Shared.Authentication;
using System;
using System.Security.Claims;

namespace BogusStore.Client.Authentication;

public class FakeAuthorizationMessageHandler : DelegatingHandler
{
    private readonly FakeAuthProvider fakeAuthProvider;

    public FakeAuthorizationMessageHandler(FakeAuthProvider fakeAuthProvider)
    {
        this.fakeAuthProvider = fakeAuthProvider;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        if(fakeAuthProvider.Current == null || fakeAuthProvider.Current.Name == "Unauthorized")
        {
            return base.SendAsync(request, cancellationToken);
        }

        request.Headers.Add("Authorization", $"Bearer {fakeAuthProvider.Current.Token}");
        return base.SendAsync(request, cancellationToken);
    }
}

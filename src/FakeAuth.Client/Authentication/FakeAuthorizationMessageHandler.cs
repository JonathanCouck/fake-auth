namespace FakeAuth.Client.Authentication;

public class FakeAuthorizationMessageHandler : DelegatingHandler
{
    private readonly FakeAuthenticationProvider fakeAuthenticationProvider;

    public FakeAuthorizationMessageHandler(FakeAuthenticationProvider fakeAuthenticationProvider)
    {
        this.fakeAuthenticationProvider = fakeAuthenticationProvider;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        AddAccessTokenToRequest(request);

        return base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        AddAccessTokenToRequest(request);

        return base.Send(request, cancellationToken);
    }

    private void AddAccessTokenToRequest(HttpRequestMessage request)
    {
        var currentCredentials = fakeAuthenticationProvider.CurrentCredentials;
        if (currentCredentials == null) return;
        
        var accessToken = currentCredentials.AccessToken;
        var tokenType = currentCredentials.TokenType;
        request.Headers.Add("Authorization", $"{tokenType} {accessToken}");
    }
}
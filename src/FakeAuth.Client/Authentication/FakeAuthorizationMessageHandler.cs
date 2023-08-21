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
        var accessToken = fakeAuthenticationProvider.Current.FindFirst("AccessToken")?.Value;
        if (accessToken != null)
        {
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
        }

        return base.SendAsync(request, cancellationToken);
    }
}
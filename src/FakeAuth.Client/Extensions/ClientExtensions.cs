using FakeAuth.Client.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FakeAuth.Client.Extensions;

public static class ClientExtensions
{
    public static void AddClientFakeAuthentication(
        this WebAssemblyHostBuilder builder,
        IHttpClientBuilder httpClientBuilder
    )
    {
        // A bit of a clunky implementation but needed since there appears to be an issue injecting Singleton HTTPClients
        // https://github.com/dotnet/aspnetcore/issues/33787
        // Manually instantiating the FakeAuthenticationProvider so we can pass on the name of the HttpClient
        // (needed to ensure the right base URL)
        builder.Services.AddSingleton<FakeAuthenticationProvider>(sp =>
            new FakeAuthenticationProvider(sp.GetRequiredService<IHttpClientFactory>(), httpClientBuilder.Name)
        );

        builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<FakeAuthenticationProvider>());
        builder.Services.AddTransient<FakeAuthorizationMessageHandler>();
        httpClientBuilder.AddHttpMessageHandler<FakeAuthorizationMessageHandler>();
    }
}

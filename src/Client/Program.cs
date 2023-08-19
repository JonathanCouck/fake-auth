using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BogusStore.Client;
using Microsoft.AspNetCore.Components.Authorization;
using BogusStore.Client.Authentication;
using BogusStore.Shared.Products;
using BogusStore.Client.Products;
using Append.Blazor.Sidepanel;
using BogusStore.Client.Tags;
using BogusStore.Client.Files;
using BogusStore.Client.Orders;
using BogusStore.Shared.Customers;
using BogusStore.Client.Customers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();

builder.Services.AddHttpClient("Project.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<FakeAuthorizationMessageHandler>();

builder.Services.AddSingleton<FakeAuthProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<FakeAuthProvider>());
builder.Services.AddTransient<FakeAuthorizationMessageHandler>();

builder.Services.AddSidepanel();

builder.Services.AddScoped<Cart>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddHttpClient<IStorageService,AzureBlobStorageService>();

await builder.Build().RunAsync();

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BogusStore.Client;
using BogusStore.Shared.Products;
using BogusStore.Client.Products;
using Append.Blazor.Sidepanel;
using BogusStore.Client.Tags;
using BogusStore.Client.Files;
using BogusStore.Client.Orders;
using BogusStore.Shared.Customers;
using BogusStore.Client.Customers;
using FakeAuth.Client.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();

var httpClientBuilder = builder.Services.AddHttpClient(
    "Project.ServerAPI",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
);

if (builder.HostEnvironment.IsDevelopment())
{
    builder.AddClientFakeAuthentication(httpClientBuilder);
}

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Project.ServerAPI"));

builder.Services.AddSidepanel();

builder.Services.AddScoped<Cart>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddHttpClient<IStorageService, AzureBlobStorageService>();

await builder.Build().RunAsync();
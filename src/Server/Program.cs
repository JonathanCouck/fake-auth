using BogusStore.Persistence;
using BogusStore.Server.Middleware;
using BogusStore.Services;
using BogusStore.Shared.Products;
using FakeAuth.Server.Extensions;
using FakeAuth.Server.Services.Token.Basic;
using FakeAuth.Server.Services.Token.Header;
using FakeAuth.Server.Services.Token.JWT;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBogusServices();

// Fluentvalidation
builder.Services.AddValidatorsFromAssemblyContaining<ProductDto.Mutate.Validator>();
builder.Services.AddFluentValidationAutoValidation();

// Swagger | OAS 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Since we subclass our dto's we need a more unique id.
    options.CustomSchemaIds(type => type.DeclaringType is null ? $"{type.Name}" : $"{type.DeclaringType?.Name}.{type.Name}");
    options.EnableAnnotations();
}).AddFluentValidationRulesToSwagger();

// Database
builder.Services.AddDbContext<BogusDbContext>();

// Fake Authentication
if (builder.Environment.IsDevelopment())
{
    // Uncomment to authenticate using Basic Authentication
    builder.Services.AddSingleton<BasicTokenGeneratorService>();
    builder.AddFakeAuthentication<BasicTokenGeneratorService, BasicAuthenticationHandler>();

    // The following instantiates the Token Generation Service and Authentication Handler that can work with JWT
    //builder.Services.AddSingleton<JwtTokenGeneratorService>();
    //builder.AddFakeAuthentication<JwtTokenGeneratorService, JwtAuthenticationHandler>();
}

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers().RequireAuthorization();
app.MapFallbackToFile("index.html");


using (var scope = app.Services.CreateScope())
{
    // Require a DbContext from the service provider and seed the database.
    var dbContext = scope.ServiceProvider.GetRequiredService<BogusDbContext>();
    FakeSeeder seeder = new(dbContext);
    seeder.Seed();
}

app.Run();
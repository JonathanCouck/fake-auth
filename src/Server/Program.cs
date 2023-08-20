using BogusStore.Persistence;
using BogusStore.Server.Authentication;
using BogusStore.Server.Middleware;
using BogusStore.Services;
using BogusStore.Shared.Authentication;
using BogusStore.Shared.Products;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

// (Fake) Authentication
builder.Services.AddAuthentication("Fake Authentication")
    .AddScheme<FakeAuthSchemeOptions, FakeAuthHandler>("Fake Authentication", options =>
    {
        options.Personas = new List<ClaimsIdentity>
        {
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "0"),
                new Claim(ClaimTypes.Name, "Anoniem"),
                new Claim(ClaimTypes.Role, Roles.Anonymous),
            }),
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Role, Roles.Administrator),
            }),
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "2"),
                new Claim(ClaimTypes.Name, "Klant"),
                new Claim(ClaimTypes.Role, Roles.Customer),
            }),
        };
        Console.WriteLine();
    });
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

// Fake Authentication routing
FakeAuthHandler.MapAuthenticationRoutes(builder, app);

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers().RequireAuthorization();
app.MapFallbackToFile("index.html");


using (var scope = app.Services.CreateScope())
{ // Require a DbContext from the service provider and seed the database.
    var dbContext = scope.ServiceProvider.GetRequiredService<BogusDbContext>();
    FakeSeeder seeder = new(dbContext);
    seeder.Seed();
}

app.Run();

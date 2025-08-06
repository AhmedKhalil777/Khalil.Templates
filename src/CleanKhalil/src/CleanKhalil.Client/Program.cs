using CleanKhalil.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
#if (UseADFS)
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using CleanKhalil.Client.Services;
#endif

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CleanKhalil.Client.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for API communication
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Add MudBlazor services
builder.Services.AddMudServices();

builder.Services.AddScoped(sp => 
{
    var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7041/") };
    return httpClient;
});

#if (UseADFS)
// Configure ADFS authentication for Blazor WASM
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("ADFS", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
});

builder.Services.AddScoped<AuthenticationService>();
#elif (UseJWT)
// Configure JWT authentication for Blazor WASM
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<JwtAuthenticationService>();
#endif

// Add application services
builder.Services.AddScoped<TodoService>();

await builder.Build().RunAsync(); 
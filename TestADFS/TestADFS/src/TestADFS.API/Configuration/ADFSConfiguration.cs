using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace TestCorp.API.Configuration;

public static class ADFSConfiguration
{
    public static IServiceCollection AddADFSAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var adfsSettings = configuration.GetSection("ADFS");
        
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = adfsSettings["Authority"];
            options.ClientId = adfsSettings["ClientId"];
            options.ClientSecret = adfsSettings["ClientSecret"];
            
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            
            options.GetClaimsFromUserInfoEndpoint = true;
            options.SaveTokens = true;
            
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = "role",
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };

            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    // Customize redirect to ADFS if needed
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    context.Response.Redirect("/Home/Error");
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
} 
#if (UseJWT)
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;

namespace CleanKhalil.Client.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationState _anonymous;

    public JwtAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            
            if (string.IsNullOrWhiteSpace(token))
                return _anonymous;

            var claims = ParseClaimsFromJwt(token);
            var expiry = claims.FirstOrDefault(x => x.Type.Equals("exp"))?.Value;
            
            if (expiry != null && DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry)) < DateTimeOffset.UtcNow)
            {
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                return _anonymous;
            }

            // Set the token in the HTTP client default headers
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }
        catch
        {
            return _anonymous;
        }
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorage.SetItemAsync("authToken", token);
        
        var claims = ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        
        _httpClient.DefaultRequestHeaders.Authorization = null;
        
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
}

public interface ILocalStorageService
{
    Task<T> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            return string.IsNullOrEmpty(json) ? default(T) : JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default(T);
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));
    }

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}

public class JwtAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly JwtAuthenticationStateProvider _authStateProvider;
    private readonly ILocalStorageService _localStorage;
    private readonly JsonSerializerOptions _jsonOptions;

    public JwtAuthenticationService(
        HttpClient httpClient, 
        AuthenticationStateProvider authStateProvider,
        ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _authStateProvider = (JwtAuthenticationStateProvider)authStateProvider;
        _localStorage = localStorage;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
                
                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    await _localStorage.SetItemAsync("refreshToken", loginResponse.RefreshToken);
                    await _authStateProvider.MarkUserAsAuthenticated(loginResponse.Token);
                    return true;
                }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RegisterAsync(string email, string password, string confirmPassword, string name)
    {
        try
        {
            var registerRequest = new { Email = email, Password = password, ConfirmPassword = confirmPassword, Name = name };
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
                
                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    await _localStorage.SetItemAsync("refreshToken", loginResponse.RefreshToken);
                    await _authStateProvider.MarkUserAsAuthenticated(loginResponse.Token);
                    return true;
                }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _httpClient.PostAsync("api/auth/logout", null);
        }
        catch
        {
            // Ignore errors on logout
        }
        finally
        {
            await _authStateProvider.MarkUserAsLoggedOut();
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var refreshRequest = new { RefreshToken = refreshToken };
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", refreshRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
                
                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    await _localStorage.SetItemAsync("refreshToken", loginResponse.RefreshToken);
                    await _authStateProvider.MarkUserAsAuthenticated(loginResponse.Token);
                    return true;
                }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public UserInfoDto User { get; set; } = new();
}

public class UserInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}
#endif 
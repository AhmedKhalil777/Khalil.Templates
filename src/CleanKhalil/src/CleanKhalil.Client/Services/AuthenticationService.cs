#if (UseADFS)
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Security.Claims;

namespace CleanKhalil.Client.Services;

public class AuthenticationService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IAccessTokenProvider _accessTokenProvider;

    public AuthenticationService(
        AuthenticationStateProvider authenticationStateProvider,
        IAccessTokenProvider accessTokenProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _accessTokenProvider = accessTokenProvider;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated ?? false;
    }

    public async Task<ClaimsPrincipal> GetUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var tokenResult = await _accessTokenProvider.RequestAccessToken();
        return tokenResult.TryGetToken(out var token) ? token.Value : null;
    }

    public async Task<string?> GetUserNameAsync()
    {
        var user = await GetUserAsync();
        return user.Identity?.Name;
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync()
    {
        var user = await GetUserAsync();
        return user.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }
}
#endif 
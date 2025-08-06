#if (UseJWT)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanKhalil.API.Models;
using CleanKhalil.API.Services;
using System.Security.Claims;

namespace CleanKhalil.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtTokenService tokenService, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // In a real application, you would validate credentials against a database
            // For this template, we'll use demo credentials
            var user = await ValidateUserCredentials(request.Email, request.Password);
            
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Expires = DateTime.UtcNow.AddMinutes(60), // Should match token expiry
                User = user
            };

            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // In a real application, you would save the user to a database
            // For this template, we'll simulate user creation
            var existingUser = await GetUserByEmail(request.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "User with this email already exists" });
            }

            var newUser = new UserInfo
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                Name = request.Name,
                Roles = new[] { "User" } // Default role
            };

            // Simulate saving user (in real app, hash password and save to DB)
            await CreateUser(newUser, request.Password);

            var accessToken = _tokenService.GenerateAccessToken(newUser);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Expires = DateTime.UtcNow.AddMinutes(60),
                User = newUser
            };

            _logger.LogInformation("User {Email} registered successfully", request.Email);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            if (!_tokenService.ValidateRefreshToken(request.RefreshToken))
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            // In a real application, you would get the user from the refresh token stored in DB
            // For this template, we'll use a demo user
            var user = new UserInfo
            {
                Id = "demo-user-id",
                Email = "demo@example.com",
                Name = "Demo User",
                Roles = new[] { "User" }
            };

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            var response = new LoginResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Expires = DateTime.UtcNow.AddMinutes(60),
                User = user
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            // In a real application, you would invalidate the refresh token in the database
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} logged out", userId);
            
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        try
        {
            var user = new UserInfo
            {
                Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                Name = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
                Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
            };

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting current user");
            return StatusCode(500, new { message = "An error occurred while retrieving user information" });
        }
    }

    // Demo methods - In a real application, these would interact with a user repository/database
    private async Task<UserInfo?> ValidateUserCredentials(string email, string password)
    {
        await Task.Delay(100); // Simulate async operation
        
        // Demo credentials for testing
        if (email == "admin@example.com" && password == "password123")
        {
            return new UserInfo
            {
                Id = "admin-user-id",
                Email = email,
                Name = "Admin User",
                Roles = new[] { "Admin", "User" }
            };
        }
        
        if (email == "user@example.com" && password == "password123")
        {
            return new UserInfo
            {
                Id = "regular-user-id",
                Email = email,
                Name = "Regular User",
                Roles = new[] { "User" }
            };
        }

        return null; // Invalid credentials
    }

    private async Task<UserInfo?> GetUserByEmail(string email)
    {
        await Task.Delay(50); // Simulate async operation
        
        // Demo - check against hardcoded users
        if (email == "admin@example.com" || email == "user@example.com")
        {
            return new UserInfo { Email = email };
        }
        
        return null;
    }

    private async Task CreateUser(UserInfo user, string password)
    {
        await Task.Delay(100); // Simulate async operation
        // In a real application, hash the password and save to database
        _logger.LogInformation("User {Email} created (demo)", user.Email);
    }
}
#endif 
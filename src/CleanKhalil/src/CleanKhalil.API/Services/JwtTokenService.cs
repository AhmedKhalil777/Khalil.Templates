#if (UseJWT)
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CleanKhalil.API.Models;

namespace CleanKhalil.API.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(UserInfo user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateRefreshToken(string refreshToken);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is required");
        _issuer = _configuration["JWT:Issuer"] ?? "CleanKhalil";
        _audience = _configuration["JWT:Audience"] ?? "CleanKhalil-Client";
        _accessTokenExpiryMinutes = int.Parse(_configuration["JWT:AccessTokenExpiryMinutes"] ?? "60");
        _refreshTokenExpiryDays = int.Parse(_configuration["JWT:RefreshTokenExpiryDays"] ?? "7");
    }

    public string GenerateAccessToken(UserInfo user)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        }.Concat(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = !string.IsNullOrEmpty(_audience),
            ValidateIssuer = !string.IsNullOrEmpty(_issuer),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = false, // We don't care about expiry here
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public bool ValidateRefreshToken(string refreshToken)
    {
        // In a real application, you would store refresh tokens in a database
        // and validate against that. For this template, we'll do basic validation
        if (string.IsNullOrEmpty(refreshToken))
            return false;

        try
        {
            var bytes = Convert.FromBase64String(refreshToken);
            return bytes.Length == 64; // Our refresh tokens are 64 bytes
        }
        catch
        {
            return false;
        }
    }
}
#endif 
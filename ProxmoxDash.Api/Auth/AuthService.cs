using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ProxmoxDash.Api.Auth;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public AuthService(
        IOptions<JwtSettings> jwtSettings,
        IConfiguration configuration,
        IRefreshTokenStore refreshTokenStore)
    {
        _jwtSettings = jwtSettings.Value;
        _configuration = configuration;
        _refreshTokenStore = refreshTokenStore;
    }

    public AuthTokens? Login(string username, string password)
    {
        var configuredUsername = _configuration["Auth:Username"];
        var configuredHash = _configuration["Auth:PasswordHash"];

        if (string.IsNullOrEmpty(configuredUsername) || string.IsNullOrEmpty(configuredHash))
        {
            return null;
        }

        if (username != configuredUsername)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, configuredHash))
        {
            return null;
        }

        return IssueTokens(username);
    }

    public AuthTokens? Refresh(string refreshToken)
    {
        var username = _refreshTokenStore.GetUsername(refreshToken);
        if (username is null)
        {
            return null;
        }

        // Rotation — invalidate the old refresh token, issue new pair
        _refreshTokenStore.Revoke(refreshToken);

        return IssueTokens(username);
    }

    public void Logout(string refreshToken)
    {
        _refreshTokenStore.Revoke(refreshToken);
    }

    private AuthTokens IssueTokens(string username)
    {
        var accessToken = GenerateAccessToken(username);
        var refreshToken = GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        _refreshTokenStore.Store(refreshToken, username, refreshExpiry);

        return new AuthTokens(accessToken, refreshToken);
    }

    private string GenerateAccessToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
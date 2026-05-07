using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ProxmoxDash.Api.Auth;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IConfiguration _configuration;

    public AuthService(IOptions<JwtSettings> jwtSettings, IConfiguration configuration)
    {
        _jwtSettings = jwtSettings.Value;
        _configuration = configuration;
    }

    public string? Login(string username, string password)
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

        return GenerateToken(username);
    }

    private string GenerateToken(string username)
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
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
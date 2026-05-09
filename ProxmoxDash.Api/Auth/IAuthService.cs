namespace ProxmoxDash.Api.Auth;

public interface IAuthService
{
    AuthTokens? Login(string username, string password);
    AuthTokens? Refresh(string refreshToken);
    void Logout(string refreshToken);
}

public record AuthTokens(string AccessToken, string RefreshToken);
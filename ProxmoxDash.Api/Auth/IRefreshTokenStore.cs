namespace ProxmoxDash.Api.Auth;

public interface IRefreshTokenStore
{
    void Store(string token, string username, DateTime expiresAt);
    string? GetUsername(string token);
    void Revoke(string token);
}
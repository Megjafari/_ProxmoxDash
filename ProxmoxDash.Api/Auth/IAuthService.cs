namespace ProxmoxDash.Api.Auth;

public interface IAuthService
{
    string? Login(string username, string password);
}
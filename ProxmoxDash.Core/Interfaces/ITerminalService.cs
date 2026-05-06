namespace ProxmoxDash.Core.Interfaces;

public interface ITerminalService
{
    Task<string> CreateSessionAsync(string host, int port, string username, string password);
    Task SendInputAsync(string sessionId, string input);
    Task CloseSessionAsync(string sessionId);
}
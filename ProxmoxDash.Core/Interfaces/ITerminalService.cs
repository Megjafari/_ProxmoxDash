namespace ProxmoxDash.Core.Interfaces;

public interface ITerminalService
{
    Task<string> CreateSessionAsync(TerminalConnectionRequest request, Func<string, Task> onOutput);
    Task SendInputAsync(string sessionId, string input);
    Task ResizeAsync(string sessionId, int columns, int rows);
    Task CloseSessionAsync(string sessionId);
}

public record TerminalConnectionRequest(
    string Host,
    int Port,
    string Username,
    string Password,
    int Columns,
    int Rows
);
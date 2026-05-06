namespace ProxmoxDash.Core.Models;

public record TerminalSession(
    string SessionId,
    string Host,
    int Port,
    string Username
);
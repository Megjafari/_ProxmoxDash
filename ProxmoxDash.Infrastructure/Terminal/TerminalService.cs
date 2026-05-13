using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProxmoxDash.Core.Interfaces;
using Renci.SshNet;

namespace ProxmoxDash.Infrastructure.Terminal;

public class TerminalService : ITerminalService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TerminalService> _logger;
    private readonly ConcurrentDictionary<string, TerminalSession> _sessions = new();

    public TerminalService(IConfiguration configuration, ILogger<TerminalService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<string> CreateSessionAsync(TerminalConnectionRequest request, Func<string, Task> onOutput)
    {
        var privateKeyPath = _configuration["Ssh:PrivateKeyPath"]
            ?? throw new InvalidOperationException("Ssh:PrivateKeyPath is not configured");
        var username = _configuration["Ssh:Username"]
            ?? throw new InvalidOperationException("Ssh:Username is not configured");

        var sessionId = Guid.NewGuid().ToString();

        var keyFile = new PrivateKeyFile(privateKeyPath);
        var connectionInfo = new ConnectionInfo(
            request.Host,
            request.Port,
            username,
            new PrivateKeyAuthenticationMethod(username, keyFile)
        );

        var client = new SshClient(connectionInfo);
        client.Connect();

        var shell = client.CreateShellStream(
            terminalName: "xterm-256color",
            columns: (uint)request.Columns,
            rows: (uint)request.Rows,
            width: 800,
            height: 600,
            bufferSize: 1024
        );

        var session = new TerminalSession(sessionId, client, shell, onOutput);
        _sessions[sessionId] = session;

        session.StartOutputPump();

        _logger.LogInformation("Terminal session {SessionId} opened to {Host}:{Port}",
            sessionId, request.Host, request.Port);

        return Task.FromResult(sessionId);
    }

    public Task SendInputAsync(string sessionId, string input)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Shell.Write(input);
        }
        return Task.CompletedTask;
    }

    public Task ResizeAsync(string sessionId, int columns, int rows)
    {
        // ShellStream resize is not supported in all SSH.NET versions.
        // Tracked as known limitation; client-side wraps text instead.
        return Task.CompletedTask;
    }

    public Task CloseSessionAsync(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            session.Dispose();
            _logger.LogInformation("Terminal session {SessionId} closed", sessionId);
        }
        return Task.CompletedTask;
    }
}
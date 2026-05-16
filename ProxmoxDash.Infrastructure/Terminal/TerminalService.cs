using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProxmoxDash.Core.Interfaces;

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

        var psi = new ProcessStartInfo("ssh")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("-tt");
        psi.ArgumentList.Add("-i");
        psi.ArgumentList.Add(privateKeyPath);
        psi.ArgumentList.Add("-p");
        psi.ArgumentList.Add(request.Port.ToString());
        psi.ArgumentList.Add("-o");
        psi.ArgumentList.Add("StrictHostKeyChecking=accept-new");
        psi.ArgumentList.Add("-o");
        psi.ArgumentList.Add("ServerAliveInterval=30");
        psi.ArgumentList.Add($"{username}@{request.Host}");

        var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start ssh process");

        var session = new TerminalSession(sessionId, process, onOutput, _logger);
        _sessions[sessionId] = session;

        session.StartOutputPump();

        _logger.LogInformation("Terminal session {SessionId} opened to {Host}:{Port} (pid {Pid})",
            sessionId, request.Host, request.Port, process.Id);

        return Task.FromResult(sessionId);
    }

    public Task SendInputAsync(string sessionId, string input)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.WriteInput(input);
        }
        else
        {
            _logger.LogWarning("SendInputAsync: session {SessionId} not found", sessionId);
        }
        return Task.CompletedTask;
    }

    public Task ResizeAsync(string sessionId, int columns, int rows)
    {
        // Resize requires sending SIGWINCH to the ssh client process; OpenSSH propagates
        // it to the remote PTY. Not strictly required for a usable terminal — xterm
        // re-wraps client-side. Future improvement: P/Invoke ioctl(TIOCSWINSZ).
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
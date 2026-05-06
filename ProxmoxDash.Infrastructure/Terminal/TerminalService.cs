using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ProxmoxDash.Core.Interfaces;
using ProxmoxDash.Core.Models;
using Renci.SshNet;

namespace ProxmoxDash.Infrastructure.Terminal;

public class TerminalService : ITerminalService
{
    private readonly ConcurrentDictionary<string, SshClient> _sessions = new();
    private readonly ConcurrentDictionary<string, ShellStream> _streams = new();
    private readonly ILogger<TerminalService> _logger;

    public TerminalService(ILogger<TerminalService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateSessionAsync(string host, int port, string username, string password)
    {
        var sessionId = Guid.NewGuid().ToString();
        var client = new SshClient(host, port, username, password);

        await Task.Run(() => client.Connect());

        var stream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

        _sessions[sessionId] = client;
        _streams[sessionId] = stream;

        _logger.LogInformation("Terminal session {SessionId} created for {Host}", sessionId, host);

        return sessionId;
    }

    public async Task SendInputAsync(string sessionId, string input)
    {
        if (_streams.TryGetValue(sessionId, out var stream))
        {
            await Task.Run(() => stream.Write(input));
        }
    }

    public async Task<string> ReadOutputAsync(string sessionId)
    {
        if (_streams.TryGetValue(sessionId, out var stream))
        {
            return await Task.Run(() => stream.Read());
        }
        return string.Empty;
    }

    public async Task CloseSessionAsync(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var client))
        {
            _streams.TryRemove(sessionId, out var stream);
            stream?.Dispose();
            await Task.Run(() => client.Disconnect());
            client.Dispose();
            _logger.LogInformation("Terminal session {SessionId} closed", sessionId);
        }
    }
}
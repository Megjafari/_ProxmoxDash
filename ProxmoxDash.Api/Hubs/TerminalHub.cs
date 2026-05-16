using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProxmoxDash.Core.Interfaces;

namespace ProxmoxDash.Api.Hubs;

[Authorize]
public class TerminalHub : Hub
{
    private readonly ITerminalService _terminalService;
    private readonly IHubContext<TerminalHub> _hubContext;
    private readonly ILogger<TerminalHub> _logger;

    // Maps SignalR connection id → terminal session id, so we can clean up on disconnect
    private static readonly ConcurrentDictionary<string, string> ConnectionSessions = new();

    public TerminalHub(
        ITerminalService terminalService,
        IHubContext<TerminalHub> hubContext,
        ILogger<TerminalHub> logger)
    {
        _terminalService = terminalService;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<string> Connect(TerminalConnectionRequest request)
    {
        var connectionId = Context.ConnectionId;

        // One session per SignalR connection — close any previous one
        if (ConnectionSessions.TryRemove(connectionId, out var existingSessionId))
        {
            await _terminalService.CloseSessionAsync(existingSessionId);
        }

        // Capture the hub context (long-lived) instead of `this` (per-invocation),
        // since the Hub instance is disposed once Connect returns.
        var hubContext = _hubContext;

        var sessionId = await _terminalService.CreateSessionAsync(request, async output =>
        {
            await hubContext.Clients.Client(connectionId).SendAsync("Output", output);
        });

        ConnectionSessions[connectionId] = sessionId;

        _logger.LogInformation("SignalR connection {ConnectionId} bound to terminal session {SessionId}",
            connectionId, sessionId);

        return sessionId;
    }

    public Task Input(string data)
    {
        if (ConnectionSessions.TryGetValue(Context.ConnectionId, out var sessionId))
        {
            return _terminalService.SendInputAsync(sessionId, data);
        }
        _logger.LogWarning("TerminalHub.Input: no session for connection {ConnectionId}", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task Resize(int columns, int rows)
    {
        if (ConnectionSessions.TryGetValue(Context.ConnectionId, out var sessionId))
        {
            return _terminalService.ResizeAsync(sessionId, columns, rows);
        }
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectionSessions.TryRemove(Context.ConnectionId, out var sessionId))
        {
            await _terminalService.CloseSessionAsync(sessionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
using System.Collections.Concurrent;

namespace ProxmoxDash.Api.Auth;

public class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private record RefreshTokenEntry(string Username, DateTime ExpiresAt);

    private readonly ConcurrentDictionary<string, RefreshTokenEntry> _tokens = new();

    public void Store(string token, string username, DateTime expiresAt)
    {
        _tokens[token] = new RefreshTokenEntry(username, expiresAt);
    }

    public string? GetUsername(string token)
    {
        if (!_tokens.TryGetValue(token, out var entry))
        {
            return null;
        }

        if (entry.ExpiresAt < DateTime.UtcNow)
        {
            _tokens.TryRemove(token, out _);
            return null;
        }

        return entry.Username;
    }

    public void Revoke(string token)
    {
        _tokens.TryRemove(token, out _);
    }
}
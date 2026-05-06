namespace ProxmoxDash.Core.Interfaces;

public interface IOllamaClient
{
    Task<string> ChatAsync(string prompt, string context);
}
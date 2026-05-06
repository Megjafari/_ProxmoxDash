namespace ProxmoxDash.Core.Models;

public record ChatMessage(
    string Role,
    string Content,
    DateTime Timestamp
);
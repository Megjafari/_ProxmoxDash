namespace ProxmoxDash.Core.Models;

public record NodeStatus(
    string Name,
    double CpuUsage,
    long MemoryUsed,
    long MemoryTotal,
    long UptimeSeconds,
    string Status
);
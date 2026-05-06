namespace ProxmoxDash.Core.Models;

public record VmInfo(
    int VmId,
    string Name,
    string Status,
    double CpuUsage,
    long MemoryUsed,
    long MemoryMax,
    int CpuCount,
    string Type,
    string Node
);
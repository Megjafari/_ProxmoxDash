namespace ProxmoxDash.Core.Models;

public record StorageInfo(
    string StorageId,
    long DiskUsed,
    long DiskTotal,
    string Type,
    bool Active
);
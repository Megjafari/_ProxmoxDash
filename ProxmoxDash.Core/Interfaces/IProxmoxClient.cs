using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Core.Interfaces;

public interface IProxmoxClient
{
    Task<IEnumerable<NodeStatus>> GetNodesAsync();
    Task<NodeStatus> GetNodeStatusAsync(string node);
    Task<IEnumerable<VmInfo>> GetVmsAsync(string node);
    Task<IEnumerable<VmInfo>> GetLxcsAsync(string node);
    Task StartVmAsync(string node, int vmId);
    Task StopVmAsync(string node, int vmId);
    Task RestartVmAsync(string node, int vmId);
    Task StartLxcAsync(string node, int vmId);
    Task StopLxcAsync(string node, int vmId);
    Task RestartLxcAsync(string node, int vmId);
    Task<IEnumerable<StorageInfo>> GetStorageAsync(string node);
}
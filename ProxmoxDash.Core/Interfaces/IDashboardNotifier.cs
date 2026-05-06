using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Core.Interfaces;

public interface IDashboardNotifier
{
    Task NotifyNodesUpdatedAsync(IEnumerable<NodeStatus> nodes);
    Task NotifyVmsUpdatedAsync(IEnumerable<VmInfo> vms);
    Task NotifyLxcsUpdatedAsync(IEnumerable<VmInfo> lxcs);
    Task NotifyStorageUpdatedAsync(IEnumerable<StorageInfo> storage);
}
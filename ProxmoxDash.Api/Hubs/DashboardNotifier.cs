using Microsoft.AspNetCore.SignalR;
using ProxmoxDash.Core.Interfaces;
using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Api.Hubs;

public class DashboardNotifier : IDashboardNotifier
{
    private readonly IHubContext<DashboardHub> _hubContext;

    public DashboardNotifier(IHubContext<DashboardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyNodesUpdatedAsync(IEnumerable<NodeStatus> nodes) =>
        _hubContext.Clients.All.SendAsync("NodesUpdated", nodes);

    public Task NotifyVmsUpdatedAsync(IEnumerable<VmInfo> vms) =>
        _hubContext.Clients.All.SendAsync("VmsUpdated", vms);

    public Task NotifyLxcsUpdatedAsync(IEnumerable<VmInfo> lxcs) =>
        _hubContext.Clients.All.SendAsync("LxcsUpdated", lxcs);

    public Task NotifyStorageUpdatedAsync(IEnumerable<StorageInfo> storage) =>
        _hubContext.Clients.All.SendAsync("StorageUpdated", storage);
}
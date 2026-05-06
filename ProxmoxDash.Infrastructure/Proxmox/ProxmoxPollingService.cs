using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProxmoxDash.Core.Interfaces;

namespace ProxmoxDash.Infrastructure.Proxmox;

public class ProxmoxPollingService : BackgroundService
{
    private readonly IProxmoxClient _proxmoxClient;
    private readonly IMemoryCache _cache;
    private readonly IDashboardNotifier _notifier;
    private readonly ILogger<ProxmoxPollingService> _logger;

    public ProxmoxPollingService(
        IProxmoxClient proxmoxClient,
        IMemoryCache cache,
        IDashboardNotifier notifier,
        ILogger<ProxmoxPollingService> logger)
    {
        _proxmoxClient = proxmoxClient;
        _cache = cache;
        _notifier = notifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nodes = (await _proxmoxClient.GetNodesAsync()).ToList();
                _cache.Set("nodes", nodes, TimeSpan.FromSeconds(30));
                await _notifier.NotifyNodesUpdatedAsync(nodes);

                var allVms = new List<Core.Models.VmInfo>();
                var allLxcs = new List<Core.Models.VmInfo>();
                var allStorage = new List<Core.Models.StorageInfo>();

                foreach (var node in nodes)
                {
                    var vms = await _proxmoxClient.GetVmsAsync(node.Name);
                    allVms.AddRange(vms);

                    var lxcs = await _proxmoxClient.GetLxcsAsync(node.Name);
                    allLxcs.AddRange(lxcs);

                    var storage = await _proxmoxClient.GetStorageAsync(node.Name);
                    allStorage.AddRange(storage);
                }

                _cache.Set("vms", allVms, TimeSpan.FromSeconds(30));
                await _notifier.NotifyVmsUpdatedAsync(allVms);

                _cache.Set("lxcs", allLxcs, TimeSpan.FromSeconds(30));
                await _notifier.NotifyLxcsUpdatedAsync(allLxcs);

                _cache.Set("storage", allStorage, TimeSpan.FromSeconds(60));
                await _notifier.NotifyStorageUpdatedAsync(allStorage);

                _logger.LogInformation("Proxmox data refreshed at {Time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to poll Proxmox");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
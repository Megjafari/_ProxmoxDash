
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProxmoxDash.Core.Interfaces;

namespace ProxmoxDash.Infrastructure.Proxmox;

public class ProxmoxPollingService : BackgroundService
{
    private readonly IProxmoxClient _proxmoxClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProxmoxPollingService> _logger;
    private const string NodeName = "pve";

    public ProxmoxPollingService(
        IProxmoxClient proxmoxClient,
        IMemoryCache cache,
        ILogger<ProxmoxPollingService> logger)
    {
        _proxmoxClient = proxmoxClient;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nodes = await _proxmoxClient.GetNodesAsync();
                var vms = await _proxmoxClient.GetVmsAsync(NodeName);
                var lxcs = await _proxmoxClient.GetLxcsAsync(NodeName);
                var storage = await _proxmoxClient.GetStorageAsync(NodeName);

                _cache.Set("nodes", nodes, TimeSpan.FromSeconds(30));
                _cache.Set("vms", vms, TimeSpan.FromSeconds(30));
                _cache.Set("lxcs", lxcs, TimeSpan.FromSeconds(30));
                _cache.Set("storage", storage, TimeSpan.FromSeconds(60));

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
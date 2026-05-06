using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProxmoxDash.Core.Interfaces;
using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Infrastructure.Proxmox;

public class ProxmoxClient : IProxmoxClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProxmoxClient> _logger;

    public ProxmoxClient(HttpClient httpClient, ILogger<ProxmoxClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<NodeStatus>> GetNodesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ProxmoxResponse<List<ProxmoxNode>>>("/api2/json/nodes");
        return response?.Data?.Select(n => new NodeStatus(
            n.Node,
            n.Cpu,
            n.Mem,
            n.Maxmem,
            n.Uptime,
            n.Status
        )) ?? [];
    }

    public async Task<NodeStatus> GetNodeStatusAsync(string node)
    {
        var response = await _httpClient.GetFromJsonAsync<ProxmoxResponse<ProxmoxNodeStatus>>($"/api2/json/nodes/{node}/status");
        var d = response!.Data!;
        return new NodeStatus(
            node,
            d.Cpu,
            d.Memory.Used,
            d.Memory.Total,
            d.Uptime,
            "online"
        );
    }

    public async Task<IEnumerable<VmInfo>> GetVmsAsync(string node)
    {
        var response = await _httpClient.GetFromJsonAsync<ProxmoxResponse<List<ProxmoxVm>>>($"/api2/json/nodes/{node}/qemu");
        return response?.Data?.Select(v => new VmInfo(
            v.Vmid,
            v.Name,
            v.Status,
            v.Cpu,
            v.Mem,
            v.Maxmem,
            v.Cpus,
            "qemu",
            node
        )) ?? [];
    }

    public async Task<IEnumerable<VmInfo>> GetLxcsAsync(string node)
    {
        var response = await _httpClient.GetFromJsonAsync<ProxmoxResponse<List<ProxmoxVm>>>($"/api2/json/nodes/{node}/lxc");
        return response?.Data?.Select(v => new VmInfo(
            v.Vmid,
            v.Name,
            v.Status,
            v.Cpu,
            v.Mem,
            v.Maxmem,
            v.Cpus,
            "lxc",
            node
        )) ?? [];
    }

    public async Task StartVmAsync(string node, int vmId) =>
        await _httpClient.PostAsync($"/api2/json/nodes/{node}/qemu/{vmId}/status/start", null);

    public async Task StopVmAsync(string node, int vmId) =>
        await _httpClient.PostAsync($"/api2/json/nodes/{node}/qemu/{vmId}/status/stop", null);

    public async Task RestartVmAsync(string node, int vmId) =>
        await _httpClient.PostAsync($"/api2/json/nodes/{node}/qemu/{vmId}/status/reboot", null);

    public async Task StartLxcAsync(string node, int vmId) =>
        await _httpClient.PostAsync($"/api2/json/nodes/{node}/lxc/{vmId}/status/start", null);

    public async Task StopLxcAsync(string node, int vmId) =>
        await _httpClient.PostAsync($"/api2/json/nodes/{node}/lxc/{vmId}/status/stop", null);

    public async Task RestartLxcAsync(string node, int vmId) =>
        await _httpClient.PostAsync($"/api2/json/nodes/{node}/lxc/{vmId}/status/reboot", null);

    public async Task<IEnumerable<StorageInfo>> GetStorageAsync(string node)
    {
        var response = await _httpClient.GetFromJsonAsync<ProxmoxResponse<List<ProxmoxStorage>>>($"/api2/json/nodes/{node}/storage");
        return response?.Data?.Select(s => new StorageInfo(
            s.Storage,
            s.Used,
            s.Total,
            s.Type,
            s.Active == 1
        )) ?? [];
    }
}

// Proxmox API response wrappers
record ProxmoxResponse<T>(T? Data);
record ProxmoxNode(string Node, double Cpu, long Mem, long Maxmem, long Uptime, string Status);
record ProxmoxNodeStatus(double Cpu, long Uptime, ProxmoxMemory Memory);
record ProxmoxMemory(long Used, long Total);
record ProxmoxVm(int Vmid, string Name, string Status, double Cpu, long Mem, long Maxmem, int Cpus);
record ProxmoxStorage(string Storage, long Used, long Total, string Type, int Active);
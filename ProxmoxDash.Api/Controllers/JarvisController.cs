using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProxmoxDash.Core.Interfaces;
using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JarvisController : ControllerBase
{
    private readonly IOllamaClient _ollamaClient;
    private readonly IMemoryCache _cache;

    public JarvisController(IOllamaClient ollamaClient, IMemoryCache cache)
    {
        _ollamaClient = ollamaClient;
        _cache = cache;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<string>> Chat([FromBody] ChatRequest request)
    {
        var context = BuildContext();
        var response = await _ollamaClient.ChatAsync(request.Prompt, context);
        return Ok(new { response });
    }

    private string BuildContext()
    {
        var nodes = _cache.Get<IEnumerable<NodeStatus>>("nodes") ?? [];
        var vms = _cache.Get<IEnumerable<VmInfo>>("vms") ?? [];
        var lxcs = _cache.Get<IEnumerable<VmInfo>>("lxcs") ?? [];

        return $"""
        You are Jarvis, an assistant for a Proxmox homelab dashboard.
        You have access to the current state of the homelab.

        Nodes: {string.Join(", ", nodes.Select(n => $"{n.Name} (CPU {n.CpuUsage:P0}, Status {n.Status})"))}
        VMs: {string.Join(", ", vms.Select(v => $"{v.Name} ({v.Status})"))}
        LXCs: {string.Join(", ", lxcs.Select(l => $"{l.Name} ({l.Status})"))}

        Answer concisely and helpfully.
        """;
    }
}

public record ChatRequest(string Prompt);
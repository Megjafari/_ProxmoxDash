// Jarvis AI chat endpoint — currently disabled.
//
// The OllamaClient implementation works, but llama3.1:8b on CPU
// produces unacceptable response times (60-180s per request).
//
// To re-enable in the future:
//   1. Uncomment this controller
//   2. Re-add builder.Services.AddOllamaClient(builder.Configuration) in Program.cs
//   3. Either run Ollama on a GPU host, switch to a smaller model,
//      or swap IOllamaClient to a cloud provider (OpenAI, Anthropic, etc.)

/*
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
*/
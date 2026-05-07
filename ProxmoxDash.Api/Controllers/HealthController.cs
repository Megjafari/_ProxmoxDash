using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IMemoryCache _cache;

    public HealthController(IMemoryCache cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var hasNodes = _cache.TryGetValue("nodes", out IEnumerable<NodeStatus>? _);

        var status = new
        {
            status = "ok",
            proxmoxConnected = hasNodes,
            timestamp = DateTimeOffset.UtcNow
        };

        return Ok(status);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NodesController : ControllerBase
{
    private readonly IMemoryCache _cache;

    public NodesController(IMemoryCache cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public ActionResult<IEnumerable<NodeStatus>> GetNodes()
    {
        if (_cache.TryGetValue("nodes", out IEnumerable<NodeStatus>? nodes) && nodes is not null)
        {
            return Ok(nodes);
        }

        return Ok(Array.Empty<NodeStatus>());
    }
}
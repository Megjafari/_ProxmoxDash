using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LxcsController : ControllerBase
{
    private readonly IMemoryCache _cache;

    public LxcsController(IMemoryCache cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public ActionResult<IEnumerable<VmInfo>> GetLxcs()
    {
        if (_cache.TryGetValue("lxcs", out IEnumerable<VmInfo>? lxcs) && lxcs is not null)
        {
            return Ok(lxcs);
        }

        return Ok(Array.Empty<VmInfo>());
    }
}
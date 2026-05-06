using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VmsController : ControllerBase
{
    private readonly IMemoryCache _cache;

    public VmsController(IMemoryCache cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public ActionResult<IEnumerable<VmInfo>> GetVms()
    {
        if (_cache.TryGetValue("vms", out IEnumerable<VmInfo>? vms) && vms is not null)
        {
            return Ok(vms);
        }

        return Ok(Array.Empty<VmInfo>());
    }
}
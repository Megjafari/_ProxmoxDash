using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProxmoxDash.Core.Interfaces;
using ProxmoxDash.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProxmoxDash.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LxcsController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly IProxmoxClient _proxmoxClient;

    public LxcsController(IMemoryCache cache, IProxmoxClient proxmoxClient)
    {
        _cache = cache;
        _proxmoxClient = proxmoxClient;
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

    [HttpPost("{node}/{vmId:int}/start")]
    public async Task<IActionResult> StartLxc(string node, int vmId)
    {
        await _proxmoxClient.StartLxcAsync(node, vmId);
        return Accepted();
    }

    [HttpPost("{node}/{vmId:int}/stop")]
    public async Task<IActionResult> StopLxc(string node, int vmId)
    {
        await _proxmoxClient.StopLxcAsync(node, vmId);
        return Accepted();
    }

    [HttpPost("{node}/{vmId:int}/restart")]
    public async Task<IActionResult> RestartLxc(string node, int vmId)
    {
        await _proxmoxClient.RestartLxcAsync(node, vmId);
        return Accepted();
    }
}
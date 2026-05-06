using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProxmoxDash.Core.Interfaces;
using ProxmoxDash.Core.Models;

namespace ProxmoxDash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VmsController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly IProxmoxClient _proxmoxClient;

    public VmsController(IMemoryCache cache, IProxmoxClient proxmoxClient)
    {
        _cache = cache;
        _proxmoxClient = proxmoxClient;
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

    [HttpPost("{node}/{vmId:int}/start")]
    public async Task<IActionResult> StartVm(string node, int vmId)
    {
        await _proxmoxClient.StartVmAsync(node, vmId);
        return Accepted();
    }

    [HttpPost("{node}/{vmId:int}/stop")]
    public async Task<IActionResult> StopVm(string node, int vmId)
    {
        await _proxmoxClient.StopVmAsync(node, vmId);
        return Accepted();
    }

    [HttpPost("{node}/{vmId:int}/restart")]
    public async Task<IActionResult> RestartVm(string node, int vmId)
    {
        await _proxmoxClient.RestartVmAsync(node, vmId);
        return Accepted();
    }
}
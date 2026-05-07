using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProxmoxDash.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProxmoxDash.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly IMemoryCache _cache;

    public StorageController(IMemoryCache cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public ActionResult<IEnumerable<StorageInfo>> GetStorage()
    {
        if (_cache.TryGetValue("storage", out IEnumerable<StorageInfo>? storage) && storage is not null)
        {
            return Ok(storage);
        }

        return Ok(Array.Empty<StorageInfo>());
    }
}
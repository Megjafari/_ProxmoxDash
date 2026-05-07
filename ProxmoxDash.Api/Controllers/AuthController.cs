using Microsoft.AspNetCore.Mvc;
using ProxmoxDash.Api.Auth;

namespace ProxmoxDash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var token = _authService.Login(request.Username, request.Password);

        if (token is null)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        return Ok(new LoginResponse(token));
    }
}

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token);
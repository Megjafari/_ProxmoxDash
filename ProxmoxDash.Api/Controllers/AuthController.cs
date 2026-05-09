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
    public ActionResult<AuthTokens> Login([FromBody] LoginRequest request)
    {
        var tokens = _authService.Login(request.Username, request.Password);

        if (tokens is null)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        return Ok(tokens);
    }

    [HttpPost("refresh")]
    public ActionResult<AuthTokens> Refresh([FromBody] RefreshRequest request)
    {
        var tokens = _authService.Refresh(request.RefreshToken);

        if (tokens is null)
        {
            return Unauthorized(new { error = "Invalid or expired refresh token." });
        }

        return Ok(tokens);
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] RefreshRequest request)
    {
        _authService.Logout(request.RefreshToken);
        return NoContent();
    }
}

public record LoginRequest(string Username, string Password);
public record RefreshRequest(string RefreshToken);
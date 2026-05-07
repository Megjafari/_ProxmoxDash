using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProxmoxDash.Api.Filters;

public class ProxmoxExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ProxmoxExceptionFilter> _logger;

    public ProxmoxExceptionFilter(ILogger<ProxmoxExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;

        // Timeout — Proxmox is unreachable or too slow
        if (ex is TaskCanceledException || ex is TimeoutException)
        {
            _logger.LogError(ex, "Proxmox request timed out");
            context.Result = new ObjectResult(new { error = "Proxmox did not respond in time. The service may be unavailable." })
            {
                StatusCode = StatusCodes.Status504GatewayTimeout
            };
            context.ExceptionHandled = true;
            return;
        }

        // HTTP-level errors from Proxmox
        if (ex is HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Proxmox API call failed");

            var statusCode = httpEx.StatusCode switch
            {
                HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
                HttpStatusCode.Unauthorized => StatusCodes.Status502BadGateway,
                HttpStatusCode.Forbidden => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status503ServiceUnavailable
            };

            var message = statusCode switch
            {
                StatusCodes.Status404NotFound => "The requested Proxmox resource was not found.",
                StatusCodes.Status502BadGateway => "Proxmox authentication failed. Check the API token configuration.",
                _ => "Unable to reach Proxmox. The service may be unavailable."
            };

            context.Result = new ObjectResult(new { error = message })
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
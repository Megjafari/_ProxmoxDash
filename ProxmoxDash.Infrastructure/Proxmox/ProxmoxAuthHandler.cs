using Microsoft.Extensions.Configuration;

namespace ProxmoxDash.Infrastructure.Proxmox;

public class ProxmoxAuthHandler : DelegatingHandler
{
    private readonly string _token;

    public ProxmoxAuthHandler(IConfiguration configuration)
    {
        _token = configuration["Proxmox:Token"]!;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation("Authorization", $"PVEAPIToken={_token}");
        return base.SendAsync(request, cancellationToken);
    }
}
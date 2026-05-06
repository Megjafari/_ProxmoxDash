using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProxmoxDash.Core.Interfaces;

namespace ProxmoxDash.Infrastructure.Proxmox;

public static class ProxmoxServiceExtensions
{
    public static IServiceCollection AddProxmoxClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ProxmoxAuthHandler>();

        services.AddHttpClient<IProxmoxClient, ProxmoxClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Proxmox:Host"]!);
        })
        .AddHttpMessageHandler<ProxmoxAuthHandler>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        return services;
    }
}
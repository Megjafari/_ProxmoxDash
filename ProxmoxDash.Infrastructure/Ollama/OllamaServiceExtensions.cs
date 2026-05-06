using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProxmoxDash.Core.Interfaces;

namespace ProxmoxDash.Infrastructure.Ollama;

public static class OllamaServiceExtensions
{
    public static IServiceCollection AddOllamaClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IOllamaClient, OllamaClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Ollama:Host"]!);
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        return services;
    }
}
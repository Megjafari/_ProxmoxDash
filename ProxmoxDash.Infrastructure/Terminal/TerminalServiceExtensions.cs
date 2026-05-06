using Microsoft.Extensions.DependencyInjection;
using ProxmoxDash.Core.Interfaces;

namespace ProxmoxDash.Infrastructure.Terminal;

public static class TerminalServiceExtensions
{
    public static IServiceCollection AddTerminalService(this IServiceCollection services)
    {
        services.AddSingleton<ITerminalService, TerminalService>();
        return services;
    }
}
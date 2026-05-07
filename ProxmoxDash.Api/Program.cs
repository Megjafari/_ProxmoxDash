using ProxmoxDash.Api.Hubs;
using ProxmoxDash.Core.Interfaces;
using ProxmoxDash.Infrastructure.Proxmox;
using ProxmoxDash.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ProxmoxExceptionFilter>();
});
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

builder.Services.AddProxmoxClient(builder.Configuration);
builder.Services.AddSingleton<IDashboardNotifier, DashboardNotifier>();
builder.Services.AddHostedService<ProxmoxPollingService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapHub<DashboardHub>("/hubs/dashboard");

app.Run();
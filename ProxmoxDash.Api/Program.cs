using ProxmoxDash.Api.Hubs;
using ProxmoxDash.Core.Interfaces;
using ProxmoxDash.Infrastructure.Ollama;
using ProxmoxDash.Infrastructure.Proxmox;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

builder.Services.AddProxmoxClient(builder.Configuration);
builder.Services.AddOllamaClient(builder.Configuration);
builder.Services.AddSingleton<IDashboardNotifier, DashboardNotifier>();
builder.Services.AddHostedService<ProxmoxPollingService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<DashboardHub>("/hubs/dashboard");

app.Run();
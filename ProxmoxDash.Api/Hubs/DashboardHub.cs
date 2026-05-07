using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace ProxmoxDash.Api.Hubs;

[Authorize]
public class DashboardHub : Hub
{
}
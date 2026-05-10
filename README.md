# ProxmoxDash

A self-hosted dashboard for Proxmox homelabs. Real-time view of nodes,
VMs, LXCs and storage with start/stop/restart controls.

> **Status:** Work in progress. Backend complete, frontend in development.

## Stack

- **Backend:** ASP.NET Core (.NET 10), SignalR, JWT auth
- **Frontend:** React + TypeScript + Tailwind (coming soon)
- **Deployment:** Docker via Coolify, exposed through Cloudflare Tunnel

## License

MIT — see [LICENSE](LICENSE).
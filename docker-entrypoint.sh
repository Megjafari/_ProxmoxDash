#!/usr/bin/env bash
set -euo pipefail

# Decode SSH private key from env into /app/.ssh/proxmoxdash_ed25519
# This is the key the backend uses to SSH into Proxmox VMs for the terminal feature.
if [[ -n "${SSH_PRIVATE_KEY_BASE64:-}" ]]; then
    mkdir -p /app/.ssh
    chmod 700 /app/.ssh
    echo "$SSH_PRIVATE_KEY_BASE64" | base64 -d > /app/.ssh/proxmoxdash_ed25519
    chmod 600 /app/.ssh/proxmoxdash_ed25519
    echo "[entrypoint] SSH private key written to /app/.ssh/proxmoxdash_ed25519"
else
    echo "[entrypoint] WARNING: SSH_PRIVATE_KEY_BASE64 not set; terminal feature will fail" >&2
fi

# Optional: pre-populate known_hosts to skip TOFU prompts on first connect.
# Generate locally with:
#   ssh-keyscan -t ed25519 192.168.8.50 192.168.8.142 192.168.8.143 ... | base64 -w 0
if [[ -n "${SSH_KNOWN_HOSTS_BASE64:-}" ]]; then
    echo "$SSH_KNOWN_HOSTS_BASE64" | base64 -d > /app/.ssh/known_hosts
    chmod 644 /app/.ssh/known_hosts
    echo "[entrypoint] known_hosts written to /app/.ssh/known_hosts"
fi

exec "$@"
# syntax=docker/dockerfile:1.7

# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for layer caching
COPY ProxmoxDash.Core/*.csproj ProxmoxDash.Core/
COPY ProxmoxDash.Infrastructure/*.csproj ProxmoxDash.Infrastructure/
COPY ProxmoxDash.Api/*.csproj ProxmoxDash.Api/
RUN dotnet restore ProxmoxDash.Api/ProxmoxDash.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish ProxmoxDash.Api/ProxmoxDash.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---------- Stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# OpenSSH client for ITerminalService (Process.Start "ssh -tt")
RUN apt-get update \
    && apt-get install -y --no-install-recommends openssh-client \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./
COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["docker-entrypoint.sh"]
CMD ["dotnet", "ProxmoxDash.Api.dll"]
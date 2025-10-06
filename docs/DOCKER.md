# Docker Guide

Spin up the full stack with containers when you want a production-like environment.

## Quick start

```powershell
cd docker
docker compose up --build
```

When the stack is healthy:
- Frontend: http://localhost:5188
- API & Swagger: http://localhost:5100 (Swagger at `/swagger`, health at `/health`)

Stop everything with `docker compose down`. Add `-d` to run in the background.

## Services at a glance

| Container        | Tech stack                 | Host port |
|------------------|----------------------------|-----------|
| `airguard-api`   | ASP.NET Core 8 (Kestrel)   | 5100 → 8080 |
| `airguard-client`| Nginx + Blazor WebAssembly | 5188 → 80   |

Both containers live on the `airguard-network` bridge and share the `outbox` volume for demo report exports.

## Health & logs

- Status: `docker compose ps`
- Follow logs: `docker compose logs -f [service]`
- Health check: the API reports at `http://localhost:5100/health` every 30 seconds

Keep logs short when attaching to reports: `docker compose logs --tail 100`.

## Configuration essentials

Environment variables in `docker-compose.yml`:

```yaml
environment:
  - ASPNETCORE_URLS=http://+:8080
  - ASPNETCORE_ENVIRONMENT=Development
  - CORS__AllowedOrigins=http://localhost:5188,https://localhost:5188
  - API_BASE_URL=http://localhost:5100
```

**Environment behavior**

- Development (default): HTTP endpoints for faster local iteration.
- Production: set `ASPNETCORE_ENVIRONMENT=Production` and enable HTTPS + HSTS (see `docs/SECURITY.md`).

Adjust ports by editing the `ports` section if 5100/5188 are taken.

## Troubleshooting quick list

- **Ports already bound:** `netstat -ano | findstr :5100` (or `:5188`) then `taskkill /PID <id> /F`, or remap ports in the compose file.
- **Build keeps failing:** `docker compose down`, `docker system prune -a`, then rebuild.
- **Client cannot reach API:** ensure `airguard-api` is healthy (`docker compose ps`), CORS origins match the client URL, and the client `appsettings.client.json` points to `http://localhost:5100`.

## Developer workflow tips

1. Update code under `src/`.
2. Rebuild the stack: `docker compose up --build` (or `docker compose restart` for quick restarts).
3. Test UI routes (`/dashboard`, `/map`, `/reports`, `/alerts`) and the API via Swagger.
4. Use `docker compose down -v` to reset persisted demo data in the `outbox` volume.

For security hardening steps and HTTPS guidance, jump to `docs/SECURITY.md`.

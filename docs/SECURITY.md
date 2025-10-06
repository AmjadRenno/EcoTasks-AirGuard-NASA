# Security Overview

## TL;DR

- Docker builds succeed after fixing `.dockerignore` and CORS origins.
- Public docs now use placeholder API keys only—secrets belong in user secrets or managed vaults.
- Health checks and security headers are wired into the API pipeline.
- HTTPS + HSTS stay disabled in dev/Docker so local HTTP flows continue to work.

## What changed

| Area | Update | Impact |
|------|--------|--------|
| Build pipeline | Removed `**/Dockerfile*` from `.dockerignore`. | Docker images build reliably in every context. |
| API gateway | Tightened `CORS__AllowedOrigins` to `http://localhost:5188,https://localhost:5188`. | Only the Blazor client is trusted. |
| Observability | Added ASP.NET Core health checks and mapped `/health`. | Container orchestration can wait for a healthy API. |
| Security headers | Applied conditional HTTPS middleware plus `X-Content-Type-Options` and `X-Frame-Options`. | Mitigates sniffing and clickjacking vectors. |
| Documentation | Replaced hard-coded API keys with placeholders and guidance. | Prevents accidental credential leakage. |

## Best practices in this repo

- **HTTPS policy:**

  ```csharp
  if (!app.Environment.IsDevelopment())
  {
      app.UseHsts();
      app.UseHttpsRedirection();
  }
  ```

  Development and Docker profiles remain HTTP-only; production turns on HTTPS + HSTS.

- **CORS:** keep `CORS__AllowedOrigins` synced with the actual frontend host (`http://localhost:5188,https://localhost:5188`).
- **Secrets:** store keys via user secrets, environment variables, or a vault (Azure Key Vault, AWS Secrets Manager). Never commit real keys.
- **Headers:** The API adds `X-Content-Type-Options=nosniff` and `X-Frame-Options=SAMEORIGIN`; extend with additional headers (CSP, X-XSS-Protection) as needed.
- **Health checks:** monitor `http://localhost:5100/health` in Docker to gate client startup or external probes.

## Next steps

- Add authentication + authorization (JWT or OAuth) before exposing protected endpoints.
- Enable HTTPS certificates inside Docker or behind a reverse proxy for demo environments.
- Layer on rate limiting and structured logging for production traffic.
- Evaluate secrets automation (Key Vault, AWS Secrets Manager) to remove manual steps.

## 🔐 API Key Management

Real API keys are intentionally not stored in the repository.

| Service | Environment Variable | Config Section |
|---------|----------------------|----------------|
| OpenWeather | `OpenWeather__ApiKey` | `OpenWeather:ApiKey` |
| AirNow | `AirNow__ApiKey` | `AirNow:ApiKey` |

### Local (PowerShell)
```powershell
$env:OpenWeather__ApiKey = "<your-openweather-key>"
$env:AirNow__ApiKey = "<your-airnow-key>"
dotnet run --project src/AirGuard.Server
```

### Docker (.env)
```
OpenWeather__ApiKey=your-openweather-key
AirNow__ApiKey=your-airnow-key
```
`docker-compose` loads these automatically and passes them into `airguard-api`.

### Why rotation was done
Previous sample keys appeared in `appsettings*.json`; they were blanked and must be rotated server‑side (generate new keys in each provider dashboard). Historical keys should be considered revoked.

### Hard fail behavior
`OpenWeatherService` now throws on startup if the key is missing; `AirNowService` already enforced this. This prevents silent use of a placeholder.

### Recommendations
1. Enforce rotation every 90 days.
2. Introduce a secrets vault for CI/CD.
3. Add a health sub-check validating key presence (without calling providers).
4. Avoid logging response bodies that might include sensitive quota metadata.


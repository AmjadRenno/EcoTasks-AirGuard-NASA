# Architecture

High-level view of the system and data flow.

## Diagram
```
Browser (Blazor WASM UI)
   |
   | HTTP (fetch API, charts JSON)
   v
AirGuard API (ASP.NET Core)
   |
   | Outbound HTTP (scheduled + on-demand)
   v
External Providers (AirNow, OpenWeather)
```

## Layers

| Layer | Responsibility |
|-------|----------------|
| Client (Blazor WASM) | UI pages (Dashboard, Map, Reports, Alerts) + HTTP calls to API. |
| Shared DTOs | Strongly typed models reused by client & server to avoid duplication. |
| API (Server) | Aggregates external data, applies simple transforms, exposes REST endpoints. |
| External APIs | Air quality and weather sources (AirNow, OpenWeather). |

## Request flow (example: weekly trend)
1. User opens Dashboard.
2. Client calls `GET /api/reports/weekly-trend`.
3. API fetches / caches recent AQI & PM data per city.
4. API returns normalized DTO array.
5. Client renders Chart.js visualization.

## Key endpoints
- `/api/airquality/current?location=City` – latest AQI & pollutants.
- `/api/airquality/forecast?location=City` – forecasted PM2.5 values.
- `/api/reports/weekly-trend` – 7‑day AQI trend.
- `/api/reports/monthly-pdf` – on-demand PDF generation.
- `/health` – liveness/readiness probe.

## Cross-cutting concerns
- CORS restricted to known client origins.
- Health checks integrated for Docker orchestration.
- Security headers added at middleware layer.
- PDF generation isolated via service to keep controllers thin.

## Environment behavior
- Dev / Docker: HTTP only for simplicity.
- Production: HTTPS + HSTS (see `docs/SECURITY.md`).

## Future enhancements
- Add caching layer (Redis) for rate-limited APIs.
- Introduce authentication (JWT) for privileged actions.
- Add persistence (PostgreSQL) for historical analytics.
- Centralized logging + tracing (OpenTelemetry).

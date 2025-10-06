# Run EcoTasks AirGuard Locally

Get the API and Blazor client running without Docker for fast iteration.

## Requirements

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extensions
- Optional: Docker Desktop (for the container workflow)

## Start the services

```powershell
# Terminal 1 – API (served on http://localhost:5100)
cd src\AirGuard.Server
dotnet run --urls http://localhost:5100
```

```powershell
# Terminal 2 – Client
cd src\AirGuard.Client
dotnet run
```

## Where to connect

| Service   | URL                          |
|-----------|------------------------------|
| Frontend  | http://localhost:5188        |
| API       | http://localhost:5100        |
| Swagger   | http://localhost:5100/swagger |
| Health    | http://localhost:5100/health |

## Hot reload

Use `dotnet watch run` in either project folder to enable automatic rebuilds while coding.

## Quick troubleshooting

- **Ports busy:** run `netstat -ano | findstr :5100` or `:5188`, then `taskkill /PID <id> /F` to free them.
- **NuGet restore issues:** `dotnet restore` inside `src/AirGuard.Server` and `src/AirGuard.Client`.
- **CORS or API calls failing:** confirm the API is running on port 5100 and `wwwroot/appsettings.client.json` points to `http://localhost:5100`.

For Docker-based instructions, jump to `docs/DOCKER.md`.

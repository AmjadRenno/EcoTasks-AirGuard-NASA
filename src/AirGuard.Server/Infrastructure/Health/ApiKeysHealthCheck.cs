using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AirGuard.Server.Infrastructure.Health;

public class ApiKeysHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;

    public ApiKeysHealthCheck(IConfiguration config)
    {
        _config = config;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        string? openWeather = _config["OpenWeather:ApiKey"]
            ?? Environment.GetEnvironmentVariable("OpenWeather__ApiKey")
            ?? Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");

        string? airNow = _config["AirNow:ApiKey"]
            ?? Environment.GetEnvironmentVariable("AirNow__ApiKey")
            ?? Environment.GetEnvironmentVariable("AIRNOW_API_KEY");

        var data = new Dictionary<string, object?>
        {
            ["openWeatherConfigured"] = !string.IsNullOrWhiteSpace(openWeather),
            ["airNowConfigured"] = !string.IsNullOrWhiteSpace(airNow)
        };

        if (string.IsNullOrWhiteSpace(openWeather) && string.IsNullOrWhiteSpace(airNow))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Both OpenWeather and AirNow API keys are missing", data: data));
        }
        if (string.IsNullOrWhiteSpace(openWeather) || string.IsNullOrWhiteSpace(airNow))
        {
            return Task.FromResult(HealthCheckResult.Degraded("One of the API keys is missing", data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy("API keys present", data: data));
    }
}

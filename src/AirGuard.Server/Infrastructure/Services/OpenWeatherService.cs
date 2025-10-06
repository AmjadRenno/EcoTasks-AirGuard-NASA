// src/AirGuard.API/Services/OpenWeatherService.cs
using System.Text.Json;

namespace AirGuard.Server.Infrastructure.Services;

public record WeatherNow(double TempC, double Humidity, double WindMs);

public interface IWeatherService
{
    Task<WeatherNow> GetNowAsync(double lat, double lon, CancellationToken ct);
}

public sealed class OpenWeatherService : IWeatherService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public OpenWeatherService(IHttpClientFactory factory, Microsoft.Extensions.Configuration.IConfiguration config)
    {
        _http = factory.CreateClient("owm");
        // Prefer hierarchical config (supports OpenWeather__ApiKey). Fallback to legacy env var if set.
        _apiKey = config["OpenWeather:ApiKey"]
                  ?? Environment.GetEnvironmentVariable("OpenWeather__ApiKey")
                  ?? Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY")
                  ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("OpenWeather API key not configured. Set OpenWeather__ApiKey environment variable.");
    }

    public async Task<WeatherNow> GetNowAsync(double lat, double lon, CancellationToken ct)
    {
        // Current weather endpoint
        var url = $"/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";

        using var doc = await _http.GetFromJsonAsync<JsonDocument>(url, ct)
                      ?? throw new InvalidOperationException("Empty response from OpenWeather.");

        var root = doc.RootElement;
        var main = root.GetProperty("main");
        var wind = root.TryGetProperty("wind", out var w) ? w : default;

        var temp = main.GetProperty("temp").GetDouble();
        var hum  = main.GetProperty("humidity").GetDouble();
        var windMs = wind.ValueKind == JsonValueKind.Object && wind.TryGetProperty("speed", out var spd)
                     ? spd.GetDouble() : 0d;

        return new WeatherNow(temp, hum, windMs);
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using AirGuard.Shared;

namespace AirGuard.Server.Infrastructure.Services;

public interface ITempoService
{
    Task<AirNowResult?> GetAirQualityAsync(double lat, double lon, CancellationToken ct = default);
}

public class TempoService : ITempoService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public TempoService(IHttpClientFactory factory, IConfiguration config)
    {
        _http = factory.CreateClient("airnow");
    _apiKey = config["AirNow:ApiKey"]
          ?? Environment.GetEnvironmentVariable("AirNow__ApiKey")
          ?? Environment.GetEnvironmentVariable("AIRNOW_API_KEY")
          ?? throw new InvalidOperationException("AirNow API Key is missing!");
    }

    public async Task<AirNowResult?> GetAirQualityAsync(double lat, double lon, CancellationToken ct = default)
    {
        try
        {
            // ✅ AirNow API endpoint for current observations
            var url = $"/aq/observation/latLong/current/?format=application/json&latitude={lat}&longitude={lon}&distance=25&API_KEY={_apiKey}";
            
            var data = await _http.GetFromJsonAsync<List<AirNowResult>>(url, ct);
            
            // Return the first PM2.5 or O3 measurement (prioritize PM2.5)
            return data?.FirstOrDefault(x => x.ParameterName == "PM2.5") 
                   ?? data?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AirNow fetch failed: {ex.Message}");
            return null;
        }
    }
}

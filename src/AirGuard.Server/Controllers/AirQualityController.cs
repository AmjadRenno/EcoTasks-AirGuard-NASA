using AirGuard.Shared;
using AirGuard.Server.Application;
using AirGuard.Server.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AirGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AirQualityController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IAqiService _aqiService;
    private readonly IMemoryCache _cache;

    public AirQualityController(IConfiguration config, IAqiService aqiService, IMemoryCache cache)
    {
        _config = config;
        _aqiService = aqiService;
        _cache = cache;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(
        [FromServices] IWeatherService weather,
        [FromServices] ITempoService tempo,
        [FromQuery] string location = "New York",
        CancellationToken ct = default)
    {
        // 1️⃣ Convert city name to coordinates dynamically via OpenWeather
        var geocodeUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(location)}&limit=1&appid={_config["OpenWeather:ApiKey"]}";
        using var http = new HttpClient();
        var geo = await http.GetFromJsonAsync<JsonElement[]>(geocodeUrl, ct);

        if (geo == null || geo.Length == 0)
            return BadRequest($"Could not find coordinates for {location}");

        double lat = geo[0].GetProperty("lat").GetDouble();
        double lon = geo[0].GetProperty("lon").GetDouble();

        // 2️⃣ Call real weather and air quality services
        var wx = await weather.GetNowAsync(lat, lon, ct);
        var airNowData = await tempo.GetAirQualityAsync(lat, lon, ct);

        double? aqi = airNowData?.AQI;
        string category = airNowData?.Category?.Name ?? "Unknown";

        var pm25 = aqi.HasValue ? _aqiService.ConvertAqiToPm25(aqi.Value) : 18.5;
        var no2 = 32.4;
        var o3 = 40.1;
        var baseStatus = category != "Unknown" ? category : _aqiService.GetAqiCategory(aqi ?? _aqiService.ConvertPm25ToAqi(pm25));
        var status = baseStatus;
        if (wx.WindMs < 1.5 && baseStatus == "Good") status = "Moderate";

        var dto = new AirQualityDto(
            Location: location,
            Pm25: pm25,
            No2: no2,
            O3: o3,
            Status: status,
            TimestampUtc: DateTime.UtcNow,
            TempC: wx.TempC,
            WindMs: wx.WindMs,
            AQI: aqi,
            Category: category
        );

        return Ok(dto);
    }


    // GET api/airquality/forecast?location=New York
    [HttpGet("forecast")]
    public IActionResult GetForecast(
        [FromServices] IAirQualityForecastService forecastService,
        [FromQuery] string location = "New York")
    {
        var forecast = forecastService.GenerateForecast(18.0);
        return Ok(new AirQualityForecast(location, forecast));
    }

    [HttpGet("bulk")]
    public async Task<IActionResult> GetBulkCurrent(
        [FromServices] IWeatherService weather,
        [FromServices] ITempoService tempo,
        [FromQuery] string[] cities,
        CancellationToken ct = default)
    {
        var results = new List<AirQualityDto>();
        
        foreach (var city in cities.Take(20)) // Limit to 20 cities
        {
            try
            {
                var cacheKey = $"aqi_{city}";
                if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is AirQualityDto cachedResult)
                {
                    results.Add(cachedResult);
                    continue;
                }

                // Get coordinates dynamically
                var geocodeUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(city)}&limit=1&appid={_config["OpenWeather:ApiKey"]}";
                using var http = new HttpClient();
                var geo = await http.GetFromJsonAsync<JsonElement[]>(geocodeUrl, ct);

                if (geo == null || geo.Length == 0) continue;

                double lat = geo[0].GetProperty("lat").GetDouble();
                double lon = geo[0].GetProperty("lon").GetDouble();

                var wx = await weather.GetNowAsync(lat, lon, ct);
                var airNowData = await tempo.GetAirQualityAsync(lat, lon, ct);

                double? aqi = airNowData?.AQI;
                string category = airNowData?.Category?.Name ?? "Unknown";
                var pm25 = aqi.HasValue ? _aqiService.ConvertAqiToPm25(aqi.Value) : 18.5;
                var finalAqi = aqi ?? _aqiService.ConvertPm25ToAqi(pm25);

                var dto = new AirQualityDto(
                    Location: city,
                    Pm25: pm25,
                    No2: 32.4,
                    O3: 40.1,
                    Status: _aqiService.GetAqiCategory(finalAqi),
                    TimestampUtc: DateTime.UtcNow,
                    TempC: wx.TempC,
                    WindMs: wx.WindMs,
                    AQI: finalAqi,
                    Category: category
                );

                // Cache for 10 minutes
                _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(10));
                results.Add(dto);
            }
            catch
            {
                // Skip failed cities
            }
        }

        return Ok(results);
    }
    
    
}
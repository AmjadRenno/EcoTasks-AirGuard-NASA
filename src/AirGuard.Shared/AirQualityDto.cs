namespace AirGuard.Shared;

public record AirQualityDto(
    string Location,
    double Pm25,
    double No2,
    double O3,
    string Status,
    DateTime TimestampUtc,
    double? TempC = null,
    double? WindMs = null,
    double? AQI = null,
    string? Category = null
);

public record ForecastPoint(DateTime TimestampUtc, double Pm25);

public record AirQualityForecast(string Location, IEnumerable<ForecastPoint> Points);

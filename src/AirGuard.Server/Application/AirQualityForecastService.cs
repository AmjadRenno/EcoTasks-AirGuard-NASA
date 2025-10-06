using AirGuard.Shared;

namespace AirGuard.Server.Application;

public interface IAirQualityForecastService
{
    IEnumerable<ForecastPoint> GenerateForecast(double basePm25);
}

public class AirQualityForecastService : IAirQualityForecastService
{
    public IEnumerable<ForecastPoint> GenerateForecast(double basePm25)
    {
        var now = DateTime.UtcNow;

        return Enumerable.Range(1, 12).Select(i =>
        {
            var pm25 = basePm25 + Math.Sin(i / 2.0) * 4 + Random.Shared.NextDouble();
            return new ForecastPoint(now.AddHours(i), Math.Round(pm25, 1));
        });
    }
}

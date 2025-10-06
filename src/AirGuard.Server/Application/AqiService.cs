using AirGuard.Shared;

namespace AirGuard.Server.Application;

public interface IAqiService
{
    double ConvertAqiToPm25(double aqi);
    double ConvertPm25ToAqi(double pm25);
    string GetAqiCategory(double aqi);
    string GetAqiColor(double aqi);
    bool IsUnhealthyLevel(double aqi);
}

public class AqiService : IAqiService
{
    public double ConvertAqiToPm25(double aqi)
    {
        // Convert AQI to PM2.5 concentration (µg/m³) based on EPA breakpoints
        if (aqi <= 50) return aqi * 12.0 / 50.0;
        if (aqi <= 100) return 12.1 + (aqi - 51) * 23.4 / 49.0;
        if (aqi <= 150) return 35.5 + (aqi - 101) * 19.4 / 49.0;
        if (aqi <= 200) return 55.5 + (aqi - 151) * 94.4 / 49.0;
        if (aqi <= 300) return 150.5 + (aqi - 201) * 99.4 / 99.0;
        return 250.5 + (aqi - 301) * 149.4 / 99.0;
    }

    public double ConvertPm25ToAqi(double pm25)
    {
        // Convert PM2.5 concentration to AQI based on EPA breakpoints
        if (pm25 <= 12.0) return pm25 * 50.0 / 12.0;
        if (pm25 <= 35.4) return 51 + (pm25 - 12.1) * 49.0 / 23.3;
        if (pm25 <= 55.4) return 101 + (pm25 - 35.5) * 49.0 / 19.9;
        if (pm25 <= 150.4) return 151 + (pm25 - 55.5) * 49.0 / 94.9;
        if (pm25 <= 250.4) return 201 + (pm25 - 150.5) * 99.0 / 99.9;
        return 301 + (pm25 - 250.5) * 99.0 / 149.4;
    }

    public string GetAqiCategory(double aqi)
    {
        return aqi switch
        {
            <= 50 => "Good",
            <= 100 => "Moderate",
            <= 150 => "Unhealthy for Sensitive Groups",
            <= 200 => "Unhealthy",
            <= 300 => "Very Unhealthy",
            _ => "Hazardous"
        };
    }

    public string GetAqiColor(double aqi)
    {
        return aqi switch
        {
            <= 50 => "#00E400",    // Green
            <= 100 => "#FFFF00",   // Yellow
            <= 150 => "#FF7E00",   // Orange
            <= 200 => "#FF0000",   // Red
            <= 300 => "#8F3F97",   // Purple
            _ => "#7E0023"         // Maroon
        };
    }

    public bool IsUnhealthyLevel(double aqi)
    {
        return aqi >= 101; // Unhealthy for Sensitive Groups and above
    }
}
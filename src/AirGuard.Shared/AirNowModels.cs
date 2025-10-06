namespace AirGuard.Shared;

public record AirNowCategory(
    int Number,
    string Name
);

public record AirNowResult(
    string? ReportingArea,
    string ParameterName,
    double AQI,
    AirNowCategory Category
);
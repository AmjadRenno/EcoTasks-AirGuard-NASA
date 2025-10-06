using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AirGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ILogger<ReportsController> logger)
    {
        _logger = logger;
        
        // QuestPDF license configuration (Community License for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [HttpGet("weekly-trend")]
    public IActionResult GetWeeklyTrend()
    {
        _logger.LogInformation("Fetching weekly AQI trend data");

        try
        {
            // Generate data for the last 7 days
            var trendData = new
            {
                Labels = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" },
                Datasets = new[]
                {
                    new
                    {
                        Label = "New York, NY",
                        Data = new[] { 45, 52, 48, 65, 70, 58, 50 },
                        BorderColor = "rgb(75, 192, 192)",
                        BackgroundColor = "rgba(75, 192, 192, 0.2)",
                        Tension = 0.4
                    },
                    new
                    {
                        Label = "Los Angeles, CA",
                        Data = new[] { 68, 75, 82, 78, 85, 80, 72 },
                        BorderColor = "rgb(255, 159, 64)",
                        BackgroundColor = "rgba(255, 159, 64, 0.2)",
                        Tension = 0.4
                    },
                    new
                    {
                        Label = "Toronto, ON",
                        Data = new[] { 42, 48, 45, 55, 52, 49, 46 },
                        BorderColor = "rgb(54, 162, 235)",
                        BackgroundColor = "rgba(54, 162, 235, 0.2)",
                        Tension = 0.4
                    },
                    new
                    {
                        Label = "Mexico City, MX",
                        Data = new[] { 85, 92, 88, 95, 90, 88, 85 },
                        BorderColor = "rgb(255, 99, 132)",
                        BackgroundColor = "rgba(255, 99, 132, 0.2)",
                        Tension = 0.4
                    }
                }
            };

            return Ok(trendData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weekly trend data");
            return StatusCode(500, new { error = "Failed to fetch trend data" });
        }
    }

    [HttpGet("monthly-pdf")]
    public IActionResult GenerateMonthlyReport()
    {
        _logger.LogInformation("Generating monthly air quality report PDF");

        try
        {
            var reportData = new[]
            {
                new { City = "New York, NY", AvgAqi = 62, BestDay = "Sep 12", WorstDay = "Sep 23", Category = "Moderate" },
                new { City = "Los Angeles, CA", AvgAqi = 78, BestDay = "Sep 4", WorstDay = "Sep 19", Category = "Moderate" },
                new { City = "Toronto, ON", AvgAqi = 54, BestDay = "Sep 8", WorstDay = "Sep 21", Category = "Moderate" },
                new { City = "Mexico City, MX", AvgAqi = 85, BestDay = "Sep 6", WorstDay = "Sep 27", Category = "Moderate" },
                new { City = "Houston, TX", AvgAqi = 68, BestDay = "Sep 3", WorstDay = "Sep 18", Category = "Moderate" },
                new { City = "Chicago, IL", AvgAqi = 56, BestDay = "Sep 11", WorstDay = "Sep 22", Category = "Moderate" }
            };

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text("EcoTasks AirGuard").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                        column.Item().AlignCenter().Text("Monthly Air Quality Report").FontSize(16).SemiBold();
                        column.Item().AlignCenter().Text($"Generated: {DateTime.UtcNow:MMMM dd, yyyy HH:mm} UTC").FontSize(10).FontColor(Colors.Grey.Darken1);
                        column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    // Content
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        // Introduction
                        column.Item().PaddingBottom(15).Text(text =>
                        {
                            text.Span("TEMPO Satellite Coverage - North America").FontSize(14).SemiBold();
                            text.Span("\nThis report provides a comprehensive overview of air quality metrics across major cities monitored by NASA's TEMPO satellite and EPA ground stations.").FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                        // Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("City").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Avg AQI").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Category").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Best Day").FontColor(Colors.White).SemiBold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Worst Day").FontColor(Colors.White).SemiBold();
                            });

                            // Data rows
                            foreach (var (item, index) in reportData.Select((x, i) => (x, i)))
                            {
                                var bgColor = index % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                                
                                table.Cell().Background(bgColor).Padding(8).Text(item.City);
                                table.Cell().Background(bgColor).Padding(8).AlignCenter().Text(item.AvgAqi.ToString());
                                table.Cell().Background(bgColor).Padding(8).AlignCenter().Text(item.Category).FontColor(GetAqiColor(item.AvgAqi));
                                table.Cell().Background(bgColor).Padding(8).AlignCenter().Text(item.BestDay);
                                table.Cell().Background(bgColor).Padding(8).AlignCenter().Text(item.WorstDay).FontColor(Colors.Red.Darken1);
                            }
                        });

                        // AQI Legend
                        column.Item().PaddingTop(20).Column(col =>
                        {
                            col.Item().Text("AQI Categories & Health Implications:").FontSize(12).SemiBold();
                            col.Item().PaddingTop(5).Text("• 0-50 (Good): Air quality is satisfactory").FontColor(Colors.Green.Darken1);
                            col.Item().Text("• 51-100 (Moderate): Acceptable for most").FontColor(Colors.Yellow.Darken2);
                            col.Item().Text("• 101-150 (Unhealthy for Sensitive Groups): Members of sensitive groups may experience health effects").FontColor(Colors.Orange.Darken1);
                            col.Item().Text("• 151-200 (Unhealthy): Everyone may begin to experience health effects").FontColor(Colors.Red.Darken1);
                            col.Item().Text("• 201-300 (Very Unhealthy): Health alert").FontColor(Colors.Purple.Darken1);
                            col.Item().Text("• 301+ (Hazardous): Health warning of emergency conditions").FontColor(Colors.Brown.Darken2);
                        });

                        // Data Sources
                        column.Item().PaddingTop(15).Column(col =>
                        {
                            col.Item().Text("Data Sources:").FontSize(11).SemiBold();
                            col.Item().Text("• NASA TEMPO Satellite - Tropospheric Emissions Monitoring").FontSize(9);
                            col.Item().Text("• EPA AirNow API - Real-time air quality data").FontSize(9);
                            col.Item().Text("• OpenWeatherMap - Weather and pollution metrics").FontSize(9);
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("EcoTasks AirGuard © 2025 | NASA Space Apps Challenge").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"AirGuard_Monthly_Report_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly report PDF");
            return StatusCode(500, new { error = "Failed to generate report" });
        }
    }

    private static string GetAqiColor(int aqi)
    {
        return aqi switch
        {
            <= 50 => Colors.Green.Darken1,
            <= 100 => Colors.Yellow.Darken2,
            <= 150 => Colors.Orange.Darken1,
            <= 200 => Colors.Red.Darken1,
            <= 300 => Colors.Purple.Darken1,
            _ => Colors.Brown.Darken2
        };
    }
}

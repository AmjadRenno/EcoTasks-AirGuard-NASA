using AirGuard.Server.Application;
using AirGuard.Server.Infrastructure.Services;
using AirGuard.Server.Infrastructure.Health;

var builder = WebApplication.CreateBuilder(args);

// ⚠️ SECURITY NOTE: For production, use User Secrets, Azure Key Vault, or Environment Variables
// Never commit real API keys to version control
// See: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets

// Services configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS: Allow Blazor frontend
const string CorsPolicyName = "FrontendOnly";

// Read allowed origins from configuration (supports Docker environment variables)
var allowedOrigins = builder.Configuration["CORS:AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries)
    ?? new[] { "http://localhost:5188", "https://localhost:5188" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// HTTP Clients
builder.Services.AddHttpClient("owm", c => 
    c.BaseAddress = new Uri("https://api.openweathermap.org"));

builder.Services.AddHttpClient("airnow", c => 
    c.BaseAddress = new Uri("https://www.airnowapi.org"));

// Register services
builder.Services.AddScoped<IWeatherService, OpenWeatherService>();
builder.Services.AddScoped<ITempoService, TempoService>();
builder.Services.AddScoped<IAirQualityForecastService, AirQualityForecastService>();
builder.Services.AddScoped<IAqiService, AqiService>();
builder.Services.AddMemoryCache();

// Health checks (with API key presence)
builder.Services.AddHealthChecks()
    .AddCheck<ApiKeysHealthCheck>("api_keys");

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect root to Swagger UI in development (helpful when opening 5100 directly)
app.MapGet("/", ctx =>
{
    if (app.Environment.IsDevelopment())
    {
        ctx.Response.Redirect("/swagger");
        return Task.CompletedTask;
    }
    ctx.Response.ContentType = "text/plain";
    return ctx.Response.WriteAsync("EcoTasks AirGuard API - see /swagger or /health");
});


// Security headers and HSTS (apply HTTPS only outside Development)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Add security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.TryAdd("X-Frame-Options", "SAMEORIGIN");
    await next();
});

var requestLogger = app.Logger;
app.Use(async (context, next) =>
{
    requestLogger.LogInformation("Handling {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    requestLogger.LogInformation("Completed {Method} {Path} with {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
});

app.UseCors(CorsPolicyName);

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Fallback: if someone hits unknown root path segments (like "/index.html" expectation) redirect to swagger in dev
if (app.Environment.IsDevelopment())
{
    app.MapFallback(async ctx =>
    {
        if (ctx.Request.Path == "/")
        {
            ctx.Response.Redirect("/swagger");
            return;
        }
        ctx.Response.StatusCode = 404;
        await ctx.Response.WriteAsync("Not Found - see /swagger");
    });
}

app.Run();

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/AirGuard.Shared/AirGuard.Shared.csproj", "src/AirGuard.Shared/"]
COPY ["src/AirGuard.Server/AirGuard.Server.csproj", "src/AirGuard.Server/"]

# Restore dependencies
RUN dotnet restore "src/AirGuard.Server/AirGuard.Server.csproj"

# Copy all source files
COPY src/ src/

# Build and publish
WORKDIR /src/src/AirGuard.Server
RUN dotnet publish "AirGuard.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install only required packages (curl for container-level health/debug) then clean
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080

# Create non-root user (UID 1001) and group to drop privileges
RUN addgroup --system appgroup \
	&& adduser --system --ingroup appgroup --no-create-home --disabled-password appuser

# Copy published files and ensure ownership by non-root user
COPY --chown=appuser:appgroup --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
	ASPNETCORE_ENVIRONMENT=Development \
	DOTNET_EnableDiagnostics=0

# Switch to least-privileged user
USER appuser

# Entry point
ENTRYPOINT ["dotnet", "AirGuard.Server.dll"]

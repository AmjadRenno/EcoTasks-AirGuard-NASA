# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/AirGuard.Shared/AirGuard.Shared.csproj", "src/AirGuard.Shared/"]
COPY ["src/AirGuard.Client/AirGuard.Client.csproj", "src/AirGuard.Client/"]

# Restore dependencies
RUN dotnet restore "src/AirGuard.Client/AirGuard.Client.csproj"

# Copy all source files
COPY src/ src/

# Build and publish
WORKDIR /src/src/AirGuard.Client
RUN dotnet publish "AirGuard.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage with Nginx
FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

# Copy published Blazor WASM files
COPY --from=build /app/publish/wwwroot .

# Copy nginx configuration
COPY src/AirGuard.Client/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

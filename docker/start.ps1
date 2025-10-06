# EcoTasks AirGuard - Docker Start Script for Windows

Write-Host "🚀 Starting EcoTasks AirGuard..." -ForegroundColor Green
Write-Host ""

# Check if Docker is running
try {
    docker info | Out-Null
    Write-Host "✅ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: Docker is not running!" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again." -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Navigate to docker directory
Set-Location $PSScriptRoot

# Check if .env exists, if not copy from .env.example
if (-not (Test-Path ".env")) {
    Write-Host "📝 Creating .env file from .env.example..." -ForegroundColor Cyan
    Copy-Item ".env.example" ".env"
    Write-Host "⚠️  Please update .env with your API keys if needed" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "🔨 Building and starting containers..." -ForegroundColor Cyan
Write-Host ""
Write-Host "Services will be available at:" -ForegroundColor Cyan
Write-Host "  • Frontend: http://localhost:5188" -ForegroundColor White
Write-Host "  • Backend API: http://localhost:5100" -ForegroundColor White
Write-Host "  • Swagger: http://localhost:5100/swagger" -ForegroundColor White
Write-Host ""

# Build and start containers
docker compose up --build

Write-Host ""
Write-Host "✅ Containers stopped" -ForegroundColor Green

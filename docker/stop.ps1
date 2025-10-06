# EcoTasks AirGuard - Docker Stop Script

Write-Host "🛑 Stopping EcoTasks AirGuard containers..." -ForegroundColor Yellow
Write-Host ""

Set-Location $PSScriptRoot

docker compose down

Write-Host ""
Write-Host "✅ All containers stopped and removed" -ForegroundColor Green

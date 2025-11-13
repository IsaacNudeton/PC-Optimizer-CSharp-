# PC Optimizer Development Startup Script
# Starts API and React dev server in background

Write-Host "🚀 Starting PC Optimizer Development Environment..." -ForegroundColor Cyan

# Kill any existing instances
Write-Host "Cleaning up existing processes..." -ForegroundColor Yellow
Get-Process -Name "PCOptimizer-API" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {$_.MainModule.FileName -like "*PCOptimizer*"} | Stop-Process -Force

# Start API in background
Write-Host "Starting API server (port 5211)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\PCOptimizer-API'; & 'C:\Program Files\dotnet\dotnet.exe' run --no-build" -WindowStyle Minimized

Start-Sleep -Seconds 3

# Start Vite dev server in background
Write-Host "Starting React dev server (port 5173)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\PCOptimizer-Frontend'; npm run dev" -WindowStyle Minimized

Start-Sleep -Seconds 3

Write-Host ""
Write-Host "✅ Development environment started!" -ForegroundColor Green
Write-Host ""
Write-Host "Services running:" -ForegroundColor Cyan
Write-Host "  - API:   http://localhost:5211" -ForegroundColor White
Write-Host "  - React: http://localhost:5173" -ForegroundColor White
Write-Host ""
Write-Host "To launch Electron: cd PCOptimizer-Frontend; npm run electron" -ForegroundColor Yellow
Write-Host "To stop all: Press Ctrl+C in each minimized window" -ForegroundColor Yellow
Write-Host ""

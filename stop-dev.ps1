# Stop all PC Optimizer development processes

Write-Host "🛑 Stopping PC Optimizer development environment..." -ForegroundColor Red

# Kill API
Get-Process -Name "PCOptimizer-API" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Stopping API (PID $($_.Id))..." -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force
}

# Kill node processes (Vite)
Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object {
    $_.MainModule.FileName -like "*PC-Optimizer*"
} | ForEach-Object {
    Write-Host "Stopping Vite/Node (PID $($_.Id))..." -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force
}

# Kill any dotnet processes related to PC Optimizer
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {
    $_.Path -like "*PC-Optimizer*"
} | ForEach-Object {
    Write-Host "Stopping .NET process (PID $($_.Id))..." -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force
}

Write-Host ""
Write-Host "✅ All development processes stopped!" -ForegroundColor Green

@echo off
echo ========================================
echo   Creating PC Optimizer C# Project
echo ========================================
echo.

:: Navigate to project directory
cd /d C:\Users\isaac\PC-Optimizer-CSharp

:: Check if dotnet is available
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found in PATH!
    echo.
    echo Please restart your terminal or run this from a NEW PowerShell window.
    echo.
    pause
    exit /b 1
)

echo .NET SDK version:
dotnet --version
echo.

:: Create WPF project
echo Creating WPF project...
dotnet new wpf -n PCOptimizer -f net9.0

:: Navigate to project
cd PCOptimizer

echo.
echo Installing packages...
echo.

:: Add required packages
dotnet add package ModernWpfUI --version 0.9.6
dotnet add package LiveCharts.Wpf --version 0.9.7
dotnet add package CommunityToolkit.Mvvm --version 8.3.2
dotnet add package System.Management --version 9.0.0

echo.
echo ========================================
echo   Project created successfully!
echo ========================================
echo.
echo Location: C:\Users\isaac\PC-Optimizer-CSharp\PCOptimizer
echo.
echo To run the app:
echo   cd PCOptimizer
echo   dotnet run
echo.
echo To open in Visual Studio:
echo   Double-click PCOptimizer.sln
echo.
pause

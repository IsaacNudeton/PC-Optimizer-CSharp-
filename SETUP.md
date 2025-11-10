# PC Optimizer - C# Edition Setup Guide

## Prerequisites

1. **Install .NET 8.0 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Choose "Windows x64" installer
   - Restart your terminal after installation

2. **Install Visual Studio 2022** (Recommended) OR **VS Code**
   - Visual Studio 2022 Community (Free): https://visualstudio.microsoft.com/
   - During install, select ".NET desktop development" workload

   OR

   - VS Code: https://code.visualstudio.com/
   - Install C# extension from Microsoft

## Project Creation

```bash
# Navigate to project directory
cd C:\Users\isaac\PC-Optimizer-CSharp

# Create WPF application
dotnet new wpf -n PCOptimizer

# Add required NuGet packages
cd PCOptimizer
dotnet add package ModernWpfUI
dotnet add package LiveCharts.Wpf
dotnet add package CommunityToolkit.Mvvm
dotnet add package System.Management

# Run the app
dotnet run
```

## Project Structure

```
PCOptimizer/
├── App.xaml                    # Application entry point
├── App.xaml.cs
├── MainWindow.xaml             # Main UI window
├── MainWindow.xaml.cs
├── Models/
│   ├── PerformanceMetrics.cs  # Data models
│   ├── OptimizationProfile.cs
│   └── SystemInfo.cs
├── ViewModels/
│   ├── MainViewModel.cs       # MVVM pattern
│   ├── DashboardViewModel.cs
│   └── MonitorViewModel.cs
├── Views/
│   ├── DashboardView.xaml
│   ├── MonitorView.xaml
│   └── OptimizationsView.xaml
├── Services/
│   ├── PerformanceMonitor.cs  # Core optimization logic
│   ├── SystemOptimizer.cs
│   └── RegistryManager.cs
└── Resources/
    └── Styles/
        └── ModernDark.xaml    # UI styling

```

## Key Libraries

- **ModernWpfUI**: Modern Fluent Design UI controls
- **LiveCharts.Wpf**: Beautiful real-time charts for monitoring
- **CommunityToolkit.Mvvm**: MVVM helpers (INotifyPropertyChanged, etc)
- **System.Management**: WMI/System access (replaces PowerShell CIM)

## Migration Path

All your PowerShell functions translate directly:

| PowerShell | C# Equivalent |
|------------|---------------|
| `Get-CimInstance Win32_Processor` | `new ManagementObjectSearcher("SELECT * FROM Win32_Processor")` |
| `Get-Process` | `Process.GetProcesses()` |
| `Set-ItemProperty` | `Registry.SetValue(...)` |
| `PerformanceCounter` | `new PerformanceCounter(...)` |

## Next Steps

1. Install .NET SDK
2. Run `dotnet new wpf -n PCOptimizer`
3. Design UI in Figma (use prompt provided)
4. Start migrating functions to C# Services

# PC Optimizer AI Agent Instructions

## Project Overview

This is a **hybrid Universal AI PC Optimizer** with three distinct codebases that communicate via REST API:

1. **PCOptimizer** (C# WPF) - Desktop app with AI orchestration engine
2. **PCOptimizer-API** (ASP.NET Core) - REST API backend referencing the WPF services
3. **PCOptimizer-Frontend** (React + Electron + Vite) - Alternative cross-platform UI

The core innovation is an **AI Agent Orchestrator** that learns user workflows, detects activities (gaming, streaming, development), and automatically configures the entire Windows system.

## Architecture: Multi-AI Agent System

### Core AI Components (`PCOptimizer/Services/AI/`)

- **`AIAgentOrchestrator.cs`** - Brain of the system. Coordinates multiple task-specific agents, resolves resource conflicts between competing agents, and orchestrates coordinated optimization plans
- **`ITaskAgent` interface** - Contract for all agents: `Reason()` (recommend actions), `ExecuteAction()` (apply optimizations), `Learn()` (improve from feedback), `GetResourceRequirements()` (declare needs)
- **Task-Specific Agents** in `Services/AI/Agents/`:
  - `GameOptimizationAgent` - Gaming optimization (input latency, FPS, GPU)
  - `StreamingAgent` - Streaming optimization (bitrate, encoding, quality)
  - _(More agents are designed but not yet implemented)_
- **`AutomationRecipeDatabase.cs`** - 13+ pre-configured workflow recipes (competitive gaming, streaming, game dev, video editing, compound workflows)
- **`UniversalConfigurator.cs`** - Applies recipes to the actual system (registry, services, resource allocation)

### Key Workflow

```
User Activity (e.g., launches Valorant + OBS)
  → BehaviorMonitor detects processes
  → Orchestrator creates GameOptimizationAgent + StreamingAgent
  → Agents Reason() and recommend conflicting actions
  → Orchestrator resolves conflicts (Gaming: 80% GPU, Streaming: 20% GPU)
  → Executes coordinated plan
  → Learns from feedback
```

### Learning Loop

Agents track success rates and confidence scores. After each action: measure system improvement → compare to expected → update knowledge base → next time: better recommendations with higher confidence.

## Technology Stack

### C# WPF Desktop (Primary UI)

- **Framework**: .NET 9.0 (Windows-only)
- **UI**: WPF with ModernWpfUI (Fluent Design)
- **Patterns**: MVVM (CommunityToolkit.Mvvm), INotifyPropertyChanged
- **System Access**: `System.Management` for WMI/registry, `LibreHardwareMonitorLib` for sensors
- **Charts**: LiveCharts.Wpf for performance graphs
- **ML**: Microsoft.ML for anomaly detection
- **Build**: Use `RUN-PROJECT.cmd` or `dotnet run` in `PCOptimizer/`

### React + Electron Frontend (Alternative)

- **Framework**: React 18 + TypeScript, Vite dev server (port 5173)
- **UI**: Material-UI (MUI), Radix UI primitives, Tailwind CSS v4 (using CSS variables)
- **State**: Zustand for global state, TanStack Query for API caching
- **API**: Axios client with base URL `http://localhost:5211/api`
- **Routing**: React Router v6
- **Dev**: Run `npm run dev` in `PCOptimizer-Frontend/` (starts Vite), `npm run electron-dev` for full Electron
- **Important**: Frontend uses `@/` path alias for `./src/`

### ASP.NET Core API

- **Framework**: .NET 9.0 Web API
- **Port**: 5211 (HTTP), with CORS enabled for Vite dev server
- **Pattern**: Controllers reference singleton services from WPF project
- **Services**: Registered as singletons (`PerformanceMonitor`, `OptimizerService`, `BehaviorMonitor`, etc.)
- **Build**: `dotnet run` in `PCOptimizer-API/`

## File Pathing Rules

**CRITICAL**: This project has strict path conventions:

- **Windows-only**: All paths use backslashes `\` (PowerShell environment)
- **Absolute paths required**: Use `C:\Users\isaac\PC-Optimizer-CSharp\...` for all file operations
- **Frontend alias**: Use `@/` for imports in React (e.g., `import { ... } from '@/api'`)
- **C# Resources**: Use forward slashes in URI resources (e.g., `/CosmicUI/Themes/VoidTheme.xaml`)

## Theme System: CosmicUI

A custom WPF theme library with profile-based auto-switching:

### Three Core Themes

1. **VoidTheme** (Default/Balanced) - Pure blacks with Hawking Radiation blue-white glow
2. **GamingTheme** (Gaming) - Aggressive red/black with crimson accents
3. **WorkTheme** (Work/Eco) - Light ergonomic with soft blues, reduced blue light

### Optional Accent Overlays

Users can apply color overlays to any theme: `PinkAccent`, `PurpleAccent`, `BlueAccent` (in `CosmicUI/Themes/Overlays/`)

### Shared Brush Keys

All themes use consistent keys: `VoidBlackBrush`, `StarlightBrush`, `HawkingRadiationBrush`, `EventHorizonBrush`, etc. Custom controls like `AccretionDiskGauge` use these brushes.

### Dynamic Switching

Use `ThemeManager.SwitchTheme(profile, accentOverlay)` to change themes at runtime. Profiles auto-switch based on detected workflow (Gaming → GamingTheme, Work → WorkTheme).

## Common Development Tasks

### Running the Full Stack

```powershell
# Terminal 1: Start API
cd C:\Users\isaac\PC-Optimizer-CSharp\PCOptimizer-API
dotnet run

# Terminal 2: Start Frontend
cd C:\Users\isaac\PC-Optimizer-CSharp\PCOptimizer-Frontend
npm run dev

# Terminal 3 (optional): Run WPF Desktop
cd C:\Users\isaac\PC-Optimizer-CSharp\PCOptimizer
dotnet run
```

### Adding a New AI Agent

1. Create class in `PCOptimizer/Services/AI/Agents/` extending `BaseTaskAgent`
2. Implement `Reason()` with detection logic and recommendations
3. Implement `ExecuteAction()` to apply optimizations
4. Implement `Learn()` to update knowledge from feedback
5. Register in `AIAgentOrchestrator.DetectAndCreateAgentsFor()`

Example pattern:
```csharp
public class NewAgent : BaseTaskAgent {
    public override async Task<AgentRecommendation> Reason(string scenario, Dictionary<string, object> context) {
        // Detect conditions, analyze context, return recommendation
    }
}
```

### API Integration

Frontend calls like:
```typescript
import { dashboardService } from '@/api';
const metrics = await dashboardService.getMetrics(); // GET /api/dashboard/metrics
```

Map to API controllers:
```csharp
[Route("api/dashboard")]
public class DashboardController : ControllerBase {
    private readonly PerformanceMonitor _monitor;
    [HttpGet("metrics")] public IActionResult GetMetrics() { ... }
}
```

### MVVM Pattern in WPF

- ViewModels in `PCOptimizer/ViewModels/` implement `INotifyPropertyChanged`
- Use `CommunityToolkit.Mvvm` attributes: `[ObservableProperty]`, `[RelayCommand]`
- Views in `PCOptimizer/Views/` bind to ViewModel properties via `{Binding PropertyName}`
- Example: `MainViewModel` coordinates all subviews

## Project-Specific Conventions

### Service Singletons

Most services are singletons shared across the entire app:
- `PerformanceMonitor` - Real-time CPU/GPU/RAM monitoring
- `BehaviorMonitor` - User activity detection (processes, windows)
- `OptimizerService` - Applies system optimizations
- `ProfileService` - Manages optimization profiles (Gaming, Work, Balanced)
- `GameDetectionService` - Detects specific games (Valorant, CS2, etc.)

### Optimization Workflow

1. `BehaviorMonitor.GetLastSnapshot()` → detect running processes
2. `AIAgentOrchestrator.DetectAndCreateAgentsFor()` → spawn agents
3. Agents `Reason()` → return recommendations
4. Orchestrator `ResolveAgentConflicts()` → merge plans
5. `ExecuteCoordinatedPlan()` → apply changes
6. `Learn()` from feedback → improve future decisions

### Data Flow

```
User Activity → BehaviorMonitor → AIAgentOrchestrator → Agents (Reason) 
→ Conflict Resolution → UniversalConfigurator (Apply) → System Changes
→ PerformanceMonitor (Measure) → AgentFeedback → Learn()
```

## Testing & Debugging

- **WPF**: Run with debugger in Visual Studio or Rider
- **API**: Check Swagger UI at `http://localhost:5211/swagger` (if enabled)
- **Frontend**: Use React DevTools, check Network tab for API calls
- **Agent Learning**: Check `ConversationLogger` logs for agent decisions and reasoning

## Architectural Notes

- **Why three codebases?** WPF for deep Windows integration, Electron for cross-platform potential, API for loose coupling
- **Why agent conflicts?** Multiple agents (Gaming + Streaming) compete for resources (both want GPU). Orchestrator prioritizes based on user intent.
- **Why recipes?** Pre-configured workflows speed up detection. Agents learn to refine recipes over time.
- **Why singleton services?** Maintain consistent state across app (e.g., one `PerformanceMonitor` for all views)

## External Documentation

- **AI Architecture**: See `AI_SYSTEM_ARCHITECTURE.md` for in-depth agent design
- **CosmicUI**: See `PCOptimizer/CosmicUI/Documentation/README.md` for theme system
- **Setup**: See `SETUP.md` and `QUICK-START.md` for installation steps

# Universal AI System Orchestrator Architecture

## Vision: One App, Any PC, Infinite Workflows

This is a **Universal AI System** that learns **how to automate, optimize, and configure any PC** - from laptops to gaming rigs to workstations - for any workflow.

---

## Core Philosophy

The system doesn't just apply profiles. It:
- **Learns** from every optimization action and its outcomes
- **Reasons** about what the user is doing and what the system needs
- **Automates** entire workflows by detecting process combinations
- **Optimizes** resource allocation intelligently
- **Configures** the entire system automatically

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│         USER ACTIVITY DETECTION (BehaviorMonitor)          │
│  Detects: running processes, active window, browser activity │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────┐
│         AI AGENT ORCHESTRATOR (Main Controller)             │
│  • Detects workflow from activity                           │
│  • Creates task-specific agents                             │
│  • Coordinates resource allocation                          │
│  • Resolves agent conflicts                                 │
└────────────────────────┬────────────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        ↓                ↓                ↓
   ┌─────────────┐  ┌─────────────┐  ┌──────────────┐
   │   Gaming    │  │  Streaming  │  │ Development  │
   │    Agent    │  │    Agent    │  │    Agent     │
   │             │  │             │  │              │
   │ • Reasons   │  │ • Reasons   │  │ • Reasons    │
   │ • Learns    │  │ • Learns    │  │ • Learns     │
   │ • Executes  │  │ • Executes  │  │ • Executes   │
   └──────┬──────┘  └──────┬──────┘  └──────┬───────┘
          │                │               │
          └────────────────┼───────────────┘
                           │
                           ↓
        ┌──────────────────────────────────┐
        │  AUTOMATION RECIPE DATABASE      │
        │  13+ Workflows:                  │
        │  • Competitive Gaming            │
        │  • Open World Gaming             │
        │  • Streaming (Twitch/YouTube)    │
        │  • Game Development              │
        │  • Video Editing                 │
        │  • Graphics Rendering            │
        │  • Office Work                   │
        │  • Compound workflows            │
        └──────────────┬───────────────────┘
                       │
                       ↓
        ┌──────────────────────────────────┐
        │   UNIVERSAL CONFIGURATOR         │
        │  • Applies recipes               │
        │  • Modifies registry             │
        │  • Controls services             │
        │  • Allocates resources           │
        │  • Launches apps                 │
        └──────────────┬───────────────────┘
                       │
                       ↓
        ┌──────────────────────────────────┐
        │     SYSTEM CHANGES               │
        │  • Registry modifications        │
        │  • Service enable/disable        │
        │  • Resource allocation           │
        │  • App launching                 │
        └──────────────────────────────────┘
```

---

## Component Details

### 1. ITaskAgent Interface & BaseTaskAgent Class

**Location**: `Services/AI/Core/`

Defines the contract for all AI agents:

```csharp
public interface ITaskAgent
{
    Task<AgentRecommendation> Reason(string scenario, Dictionary<string, object> context);
    Task<AgentActionResult> ExecuteAction(string actionName, Dictionary<string, object> parameters);
    Task Learn(string scenario, AgentFeedback feedback);
    Task<Dictionary<string, double>> GetCurrentMetrics();
    AgentResourceRequirements GetResourceRequirements();
}
```

**Key Methods**:
- **Reason()** - Pure logic. Analyzes system state and recommends actions without executing
- **ExecuteAction()** - Performs the optimization
- **Learn()** - Updates internal knowledge base from outcomes
- **GetResourceRequirements()** - Tells orchestrator what resources it needs

---

### 2. Task-Specific Agents

#### GameOptimizationAgent

**Location**: `Services/AI/Agents/GameOptimizationAgent.cs`

Specializes in gaming optimization:

```csharp
public class GameOptimizationAgent : BaseTaskAgent
{
    // Detects game type (Valorant, CS2, GTA, etc.)
    // Adapts strategy based on workload:

    // Competitive Shooters: Prioritize input latency
    if (IsCompetitiveShooter(game))
    {
        Recommend: DisableVSync, BoostTimerResolution, MaxPollingRate
        ExpectedImprovement: 35% latency reduction
    }

    // Open World: Prioritize FPS
    if (IsOpenWorld(game))
    {
        Recommend: BoostGPU, OptimizeVRAM, ClearMemory
        ExpectedImprovement: 40% FPS improvement
    }
}
```

#### StreamingAgent

**Location**: `Services/AI/Agents/StreamingAgent.cs`

Handles streaming optimization:

```csharp
public class StreamingAgent : BaseTaskAgent
{
    // Detects platform (Twitch, YouTube)
    // Balances stream quality with system stability
    // Auto-adjusts bitrate, resolution, latency
    // Prevents frame drops
}
```

---

### 3. AIAgentOrchestrator

**Location**: `Services/AI/AIAgentOrchestrator.cs`

The "brain" of the system:

```csharp
public async Task RunOrchestration(CancellationToken cancellationToken)
{
    while (running)
    {
        // 1. Get system state
        var systemState = await GetSystemSnapshot();

        // 2. Detect active workflow
        var activity = await GetUserActivity();

        // 3. Create agents for detected tasks
        await DetectAndCreateAgentsFor(activity);

        // 4. Get recommendations from all agents
        var recommendations = await GetAgentRecommendations();

        // 5. CRITICAL: Resolve conflicts between agents
        var finalPlan = ResolveAgentConflicts(recommendations);

        // 6. Execute coordinated plan
        await ExecuteCoordinatedPlan(finalPlan);

        // 7. Learn from feedback
        await LearnFromLastActions();

        await Task.Delay(5000);  // Check every 5 seconds
    }
}
```

**Conflict Resolution Example**:
```
Scenario: User gaming + streaming simultaneously
Gaming Agent: "I need 95% GPU"
Streaming Agent: "I need 30% GPU for encoding"
Conflict: 125% > 100%

Orchestrator Decision:
- Gaming is critical (user playing) → 80% GPU
- Streaming is secondary → 20% GPU
- Maintain both at acceptable quality
```

---

### 4. AutomationRecipeDatabase

**Location**: `Services/AI/AutomationRecipeDatabase.cs`

13+ automation recipes covering:

#### Gaming Recipes
- `CompetitiveGaming` - Valorant, CS2, OW2 optimization
- `OpenWorldGaming` - GTA, Red Dead, Cyberpunk

#### Streaming Recipes
- `TwitchStreaming` - 1080p60, < 5 sec latency
- `YouTubeStreaming` - High bitrate, quality-first

#### Development Recipes
- `GameDevelopment` - UE5, Visual Studio, build optimization
- `WebDevelopment` - VS Code, Node.js, fast reload

#### Content Creation Recipes
- `VideoEditing` - Premiere, DaVinci, 4K editing
- `GraphicsRendering` - Blender, 3DS Max, GPU rendering

#### Compound Workflows
- `GameDevStreaming` - Develop + Stream simultaneously
- `GamingStreaming` - Play + Stream simultaneously

#### Productivity
- `OfficeWork` - Excel, Word, Outlook optimization
- `Programming` - Professional development environment

Each recipe contains:
```csharp
public class AutomationRecipe
{
    public List<string> ProcessTriggers;        // What detects this workflow
    public List<string> OptimizationActions;    // What to do
    public Dictionary<string, double> ResourceAllocation;  // GPU, CPU, RAM percentages
    public Dictionary<string, string> RegistryChanges;     // Registry modifications
    public Dictionary<string, bool> ServiceStates;         // Services to disable
    public List<string> CompanionApps;         // Apps to launch
    public string ExpectedOutcome;              // What user will experience
}
```

---

### 5. UniversalConfigurator

**Location**: `Services/AI/UniversalConfigurator.cs`

Applies recipes to the actual system:

```csharp
public async Task<ConfigurationResult> DetectAndConfigureWorkflow(
    List<string> runningProcesses,
    SystemSnapshot systemState)
{
    // 1. Detect which recipe matches
    var matchedRecipes = _db.FindRecipesForProcesses(runningProcesses);

    // 2. Apply the best match
    var bestRecipe = matchedRecipes.OrderByDescending(r => r.Specificity).First();

    // 3. Execute recipe:
    //    - Modify registry
    //    - Enable/disable services
    //    - Allocate resources
    //    - Launch apps

    return result;
}
```

---

## How It Works: Real Example

### Scenario 1: User opens Valorant

```
[BehaviorMonitor] Detects: Valorant.exe running

[Orchestrator] Receives activity
               → Calls DetectAndCreateAgentsFor()
               → GameOptimizationAgent created and initialized

[GameOptimizationAgent] Reasons:
                       "Competitive shooter detected"
                       "Priority: Input latency > FPS"
                       "Recommend: DisableVSync, BoostTimerResolution, MaxPollingRate"
                       "Confidence: 95%"
                       "AutoApply: true"

[Orchestrator] Receives recommendation
              → No conflicts (only one agent)
              → Executes: DisableVSync, BoostTimerResolution, MaxPollingRate

[System] Changes applied:
        - VSync disabled
        - Timer resolution: 1ms → 0.5ms
        - USB polling: 125Hz → 8kHz

[Result] User experiences:
        - Input latency: -50%
        - Frame consistency: improved
        - 240+ FPS stable

[Feedback Loop] After 1 minute:
               GameOptimizationAgent.Learn("Valorant input latency optimization")
               → Success rate updated
               → Confidence increased
               → Next Valorant session: even faster recommendations
```

### Scenario 2: User opens game + OBS (streaming)

```
[Activity] Valorant.exe + OBS64.exe detected

[Orchestrator] Creates two agents:
              - GameOptimizationAgent
              - StreamingAgent

[GameOptimizationAgent] Recommends:
                       "Allocate 95% GPU to game"
                       "Confidence: 90%"

[StreamingAgent] Recommends:
                "Allocate 30% GPU to encoding"
                "Allocate 80% network"
                "Confidence: 85%"

[CONFLICT] GPU: 95% + 30% = 125% (exceeds 100%)

[Orchestrator Resolution]
   Gaming is primary (user playing)
   Streaming is secondary

   Decision:
   - Game: 80% GPU (slight quality reduction acceptable)
   - Streaming: 20% GPU (can use CPU encoding as fallback)
   - Network: 85% priority

   Execute combined plan:
   - Boost GPU clocks to 80%
   - Set CPU encoder for stream
   - Optimize network stack

[Result]
   - Game: 140+ FPS (down from 240, but still excellent)
   - Stream: 1080p60, 7.5Mbps, < 4sec latency
   - Both work well simultaneously
```

---

## How Agents Learn

### Learning Flow

```
Action Executed
    ↓
System Change Applied
    ↓
Wait 10-30 seconds
    ↓
Measure Results (FPS, Latency, CPU, GPU, Temps)
    ↓
Compare: Expected vs Actual
    ↓
AgentFeedback Created:
  - FeedbackType: Success / PartialSuccess / Failure
  - MeasuredImprovement: actual % gained
  - UserFeedback: "feels better" / "no difference" / "worse"
    ↓
Agent.Learn(scenario, feedback)
    ↓
Update Knowledge Base:
  - Success rates
  - Learned patterns
  - Confidence scores
    ↓
NEXT TIME: Better recommendations (higher confidence, auto-apply)
```

### Example Learning

```
Day 1: User plays Valorant
  GameAgent: "Recommend DisableVSync, confidence 60%"
  Measured: Latency improved 35%
  Feedback: "feels good"
  → Success rate: 100%, confidence: 70%

Day 2: User plays Valorant again
  GameAgent: "Recommend DisableVSync, confidence 75%, AutoApply: true"
  Measured: Latency improved 38%
  Feedback: "even better"
  → Success rate: 100%, confidence: 85%

Day 3: User plays on similar hardware
  GameAgent: "Recommend DisableVSync + BoostTimerResolution, confidence 92%, AutoApply: true"
  Measured: Latency improved 42%
  Feedback: "feels amazing"
  → This optimization profile saved for future use
```

---

## Integration Points

### How to integrate with existing services:

```csharp
// In Program.cs or startup
services.AddSingleton<BehaviorMonitor>();
services.AddSingleton<PerformanceMonitor>();
services.AddSingleton<AIAgentOrchestrator>();
services.AddSingleton<UniversalConfigurator>();

// In API controller
[ApiController]
public class AIController
{
    private AIAgentOrchestrator _orchestrator;

    [HttpPost("activate-workflow")]
    public async Task<IActionResult> ActivateWorkflow(string workflow)
    {
        var result = await _configurator.ApplyRecipeByName(workflow);
        return Ok(result);
    }

    [HttpGet("detect-workflow")]
    public async Task<IActionResult> DetectWorkflow()
    {
        var activity = _behavior.GetLastSnapshot();
        var processes = activity.RunningProcesses.Select(p => p.ProcessName).ToList();
        var result = await _configurator.DetectAndConfigureWorkflow(processes, systemState);
        return Ok(result);
    }
}

// In background service
[HostedService]
public class AIOrchestrationService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _orchestrator.RunOrchestration(stoppingToken);
    }
}
```

---

## Key Design Principles

1. **Learning Never Stops**: Every action feeds back into improving future recommendations
2. **Reasoning is Transparent**: Agents explain WHY they recommend something
3. **Conflicts are Resolved Intelligently**: Orchestrator prioritizes based on user intent
4. **Everything is Reversible**: Recipes can be reverted to previous state
5. **Scales from Laptop to Workstation**: Same architecture, different recipes
6. **Learns Your Preferences**: After 10 uses of a workflow, agent knows what you like
7. **Zero Manual Configuration**: Everything is automated and learned

---

## Future Extensions

1. **More Agents**: Content creator agent, database agent, VM optimization agent
2. **ML Enhancement**: Use patterns database to predict optimal settings before user needs them
3. **Cross-PC Learning**: Sync learned optimizations across multiple PCs
4. **Hardware Generalization**: Learn that RTX 4090 optimizations work similarly on RTX 4080
5. **Predictive**: Anticipate user needs based on calendar and history
6. **Community**: Share successful recipes across users

---

## The Universal Brand

This system embodies **Universal PC Optimization**:
- One app
- Works on ANY PC
- Handles ANY workflow
- Learns from YOUR usage
- Adapts to YOUR hardware
- Explains its decisions
- Gets smarter every day

The AI doesn't just optimize. It learns how to think like a PC optimization expert and applies that expertise to your specific machine and workflow.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PCOptimizer.Services.AI.Agents;
using PCOptimizer.Services.AI.Core;

namespace PCOptimizer.Services.AI
{
    /// <summary>
    /// The core AI System Orchestrator
    /// Manages multiple AI agents, coordinates resource allocation, resolves conflicts
    /// This is the "Universal AI Brain" that automates, optimizes, and configures the entire system
    /// </summary>
    public class AIAgentOrchestrator
    {
        private List<ITaskAgent> _activeAgents = new();
        private SystemSnapshot _currentSystemState = new();
        private PerformanceMonitor _performanceMonitor;
        private BehaviorMonitor _behaviorMonitor;
        private CancellationTokenSource _orchestrationCts = new();
        private Dictionary<string, AgentActionResult> _lastActionResults = new();

        public AIAgentOrchestrator(PerformanceMonitor performanceMonitor, BehaviorMonitor behaviorMonitor)
        {
            _performanceMonitor = performanceMonitor;
            _behaviorMonitor = behaviorMonitor;
            Console.WriteLine("[Orchestrator] AI System Orchestrator initialized");
        }

        /// <summary>
        /// Detect user's current task/workflow and create appropriate agents
        /// This is the core automation engine
        /// </summary>
        public async Task<List<ITaskAgent>> DetectAndCreateAgentsFor(ActivitySnapshot activity)
        {
            var newAgents = new List<ITaskAgent>();

            Console.WriteLine($"[Orchestrator] Detecting task from active processes...");
            Console.WriteLine($"[Orchestrator] Active Window: {activity.ActiveWindow?.ProcessName}");

            // Detect gaming
            if (IsGameRunning(activity))
            {
                if (!AgentExists("Gaming"))
                {
                    var gameAgent = new GameOptimizationAgent();
                    await gameAgent.Initialize(_currentSystemState);
                    newAgents.Add(gameAgent);
                    _activeAgents.Add(gameAgent);
                    Console.WriteLine("[Orchestrator] Created GameOptimizationAgent");
                }
            }

            // Detect streaming
            if (IsStreamingSetup(activity))
            {
                if (!AgentExists("Streaming"))
                {
                    var streamAgent = new StreamingAgent();
                    await streamAgent.Initialize(_currentSystemState);
                    newAgents.Add(streamAgent);
                    _activeAgents.Add(streamAgent);
                    Console.WriteLine("[Orchestrator] Created StreamingAgent");
                }
            }

            // Detect development
            if (IsDevelopmentWorkload(activity))
            {
                if (!AgentExists("Development"))
                {
                    var devAgent = new DevelopmentAgent();
                    await devAgent.Initialize(_currentSystemState);
                    newAgents.Add(devAgent);
                    _activeAgents.Add(devAgent);
                    Console.WriteLine("[Orchestrator] Created DevelopmentAgent");
                }
            }

            // Detect content creation
            if (IsContentCreation(activity))
            {
                if (!AgentExists("ContentCreation"))
                {
                    var contentAgent = new ContentCreationAgent();
                    await contentAgent.Initialize(_currentSystemState);
                    newAgents.Add(contentAgent);
                    _activeAgents.Add(contentAgent);
                    Console.WriteLine("[Orchestrator] Created ContentCreationAgent");
                }
            }

            return newAgents;
        }

        /// <summary>
        /// Run the orchestrator loop
        /// Continuously monitors system state and coordinates agents
        /// </summary>
        public async Task RunOrchestration(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("[Orchestrator] Starting orchestration loop...");

            while (!cancellationToken.IsCancellationRequested && !_orchestrationCts.Token.IsCancellationRequested)
            {
                try
                {
                    // 1. Get current system state
                    _currentSystemState = await GetSystemSnapshot();

                    // 2. Get current activity
                    var activity = _behaviorMonitor?.CaptureSnapshot();

                    if (activity != null)
                    {
                        // 3. Create agents for detected tasks
                        await DetectAndCreateAgentsFor(activity);

                        // 4. Get recommendations from all agents
                        var recommendations = await GetAgentRecommendations();

                        // 5. Resolve conflicts between agents
                        var finalPlan = ResolveAgentConflicts(recommendations);

                        // 6. Execute coordinated plan
                        if (finalPlan.Count > 0)
                        {
                            await ExecuteCoordinatedPlan(finalPlan);
                        }
                    }

                    // 7. Learn from feedback
                    await LearnFromLastActions();

                    // Sleep before next iteration
                    await Task.Delay(5000, cancellationToken);  // Check every 5 seconds
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Orchestrator] Error in orchestration loop: {ex.Message}");
                    await Task.Delay(1000);
                }
            }

            Console.WriteLine("[Orchestrator] Shutting down orchestration...");
            await ShutdownAllAgents();
        }

        /// <summary>
        /// Get reasoning from all active agents
        /// </summary>
        private async Task<List<AgentRecommendation>> GetAgentRecommendations()
        {
            var recommendations = new List<AgentRecommendation>();

            foreach (var agent in _activeAgents)
            {
                try
                {
                    var context = new Dictionary<string, object>
                    {
                        { "systemState", _currentSystemState },
                        { "agentCount", _activeAgents.Count },
                        { "cpuUsage", _currentSystemState.CurrentCPUUsage },
                        { "gpuUsage", _currentSystemState.CurrentGPUUsage },
                        { "ramUsage", _currentSystemState.CurrentRAMUsage }
                    };

                    var rec = await agent.Reason($"{agent.AgentType}_optimization", context);
                    if (rec != null)
                    {
                        recommendations.Add(rec);
                        Console.WriteLine($"[{agent.AgentType}] Recommendation: {rec.Title} (confidence: {rec.Confidence:F2})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Orchestrator] Error getting recommendation from {agent.AgentType}: {ex.Message}");
                }
            }

            return recommendations;
        }

        /// <summary>
        /// Core conflict resolution engine
        /// When agents compete for resources, orchestrator decides priority
        /// </summary>
        private Dictionary<string, List<string>> ResolveAgentConflicts(List<AgentRecommendation> recommendations)
        {
            var executionPlan = new Dictionary<string, List<string>>();

            // Group recommendations by agent
            var agentRecs = new Dictionary<string, AgentRecommendation>();
            foreach (var rec in recommendations)
            {
                // Find which agent made this recommendation (simplified for now)
                var agentType = rec.Title.Split(' ')[rec.Title.Split(' ').Length - 1];
                if (!agentRecs.ContainsKey(agentType) || rec.Confidence > agentRecs[agentType].Confidence)
                {
                    agentRecs[agentType] = rec;
                }
            }

            // Resolve conflicts
            var gpuRequests = new List<(string Agent, double Priority)>();
            var cpuRequests = new List<(string Agent, double Priority)>();

            foreach (var agent in _activeAgents)
            {
                var req = agent.GetResourceRequirements();
                if (req.GPUPercentage > 0)
                    gpuRequests.Add((agent.AgentType, req.Priority));
                if (req.CPUPercentage > 0)
                    cpuRequests.Add((agent.AgentType, req.Priority));
            }

            // GPU allocation: sort by priority, allocate sequentially
            var totalGPU = 0.0;
            foreach (var (agent, priority) in gpuRequests.OrderByDescending(x => x.Priority))
            {
                if (totalGPU < 100)
                {
                    var agentAllocation = Math.Min(30, 100 - totalGPU);  // Cap at 30% per agent
                    totalGPU += agentAllocation;
                    Console.WriteLine($"[Orchestrator] Allocating {agentAllocation}% GPU to {agent}");
                }
            }

            // If there are conflicts, adjust priorities
            if (gpuRequests.Count > 1 && totalGPU > 100)
            {
                Console.WriteLine("[Orchestrator] GPU oversubscription detected, reducing secondary agents");
                // Reduce non-critical agents
                foreach (var (agent, priority) in gpuRequests.OrderBy(x => x.Priority))
                {
                    if (agent != "Gaming")  // Gaming is critical
                    {
                        Console.WriteLine($"[Orchestrator] Reducing {agent} GPU allocation");
                    }
                }
            }

            // Build execution plan from recommendations
            foreach (var (agentType, rec) in agentRecs)
            {
                if (rec.AutoApply || rec.Confidence > 0.8)
                {
                    executionPlan[agentType] = rec.ActionsToTake;
                }
            }

            return executionPlan;
        }

        /// <summary>
        /// Execute the coordinated optimization plan
        /// </summary>
        private async Task ExecuteCoordinatedPlan(Dictionary<string, List<string>> plan)
        {
            foreach (var (agentType, actions) in plan)
            {
                var agent = _activeAgents.FirstOrDefault(a => a.AgentType == agentType);
                if (agent == null) continue;

                foreach (var action in actions)
                {
                    var result = await agent.ExecuteAction(action, new());
                    _lastActionResults[action] = result;

                    if (result.Success)
                    {
                        Console.WriteLine($"[{agentType}] ✓ {action}: {result.Improvement:F1}% improvement");
                    }
                    else
                    {
                        Console.WriteLine($"[{agentType}] ✗ {action} failed: {result.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Learn from last executed actions
        /// Update agent knowledge based on actual outcomes
        /// </summary>
        private async Task LearnFromLastActions()
        {
            foreach (var (agent, result) in _lastActionResults)
            {
                var agentObj = _activeAgents.FirstOrDefault(a => a.AgentType.Contains(agent));
                if (agentObj != null)
                {
                    var feedback = new AgentFeedback
                    {
                        FeedbackType = result.Success ? "Success" : "Failure",
                        MeasuredImprovement = result.Improvement,
                        Description = result.Message
                    };

                    await agentObj.Learn(agent, feedback);
                }
            }
        }

        /// <summary>
        /// Get system snapshot for agent context
        /// </summary>
        private async Task<SystemSnapshot> GetSystemSnapshot()
        {
            return SystemSnapshot.FromMonitors(_performanceMonitor, _behaviorMonitor);
        }

        /// <summary>
        /// Shutdown all agents gracefully
        /// </summary>
        private async Task ShutdownAllAgents()
        {
            foreach (var agent in _activeAgents)
            {
                await agent.Shutdown();
            }
            _activeAgents.Clear();
        }

        // Detection helpers
        private bool IsGameRunning(ActivitySnapshot activity) =>
            activity?.RunningProcesses?.Any(p =>
                p.ProcessName.ContainsIgnoreCase("Valorant") ||
                p.ProcessName.ContainsIgnoreCase("CS2") ||
                p.ProcessName.ContainsIgnoreCase("GTA") ||
                p.ProcessName.ContainsIgnoreCase("Unreal") ||
                p.ProcessName.ContainsIgnoreCase("Steam") ||
                p.ProcessName.ContainsIgnoreCase("Epic")) ?? false;

        private bool IsStreamingSetup(ActivitySnapshot activity) =>
            activity?.RunningProcesses?.Any(p =>
                p.ProcessName.ContainsIgnoreCase("OBS") ||
                p.ProcessName.ContainsIgnoreCase("Streamlabs") ||
                p.ProcessName.ContainsIgnoreCase("Twitch")) ?? false;

        private bool IsDevelopmentWorkload(ActivitySnapshot activity) =>
            activity?.RunningProcesses?.Any(p =>
                p.ProcessName.ContainsIgnoreCase("Visual Studio") ||
                p.ProcessName.ContainsIgnoreCase("VS Code") ||
                p.ProcessName.ContainsIgnoreCase("Rider")) ?? false;

        private bool IsContentCreation(ActivitySnapshot activity) =>
            activity?.RunningProcesses?.Any(p =>
                p.ProcessName.ContainsIgnoreCase("Premiere") ||
                p.ProcessName.ContainsIgnoreCase("DaVinci") ||
                p.ProcessName.ContainsIgnoreCase("Blender")) ?? false;

        private bool AgentExists(string agentType) =>
            _activeAgents.Any(a => a.AgentType == agentType);
    }

    internal static class StringExtensions2
    {
        internal static bool ContainsIgnoreCase(this string str, string value) =>
            str?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}

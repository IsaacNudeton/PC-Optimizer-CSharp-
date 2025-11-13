using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCOptimizer.Services.AI.Core
{
    /// <summary>
    /// Defines the interface for all AI agents
    /// Agents are task-specific optimization and automation entities
    /// </summary>
    public interface ITaskAgent
    {
        string AgentId { get; }
        string AgentName { get; }
        string AgentType { get; }  // "Gaming", "Streaming", "Development", "ContentCreation", etc.
        AgentState CurrentState { get; }
        double ConfidenceScore { get; }

        /// <summary>
        /// Initialize the agent with system context
        /// </summary>
        Task Initialize(SystemSnapshot systemContext);

        /// <summary>
        /// Reason about current state and return recommendation
        /// The "brain" of the agent - pure reasoning without execution
        /// </summary>
        Task<AgentRecommendation> Reason(string scenario, Dictionary<string, object> context);

        /// <summary>
        /// Execute an optimization action
        /// </summary>
        Task<AgentActionResult> ExecuteAction(string actionName, Dictionary<string, object> parameters);

        /// <summary>
        /// Learn from feedback - improves future recommendations
        /// </summary>
        Task Learn(string scenario, AgentFeedback feedback);

        /// <summary>
        /// Get current metrics this agent is monitoring
        /// </summary>
        Task<Dictionary<string, double>> GetCurrentMetrics();

        /// <summary>
        /// Adjust priority relative to other agents (0.0 = lowest, 1.0 = highest)
        /// </summary>
        Task SetResourcePriority(double priority);

        /// <summary>
        /// Get recommendations for resource allocation
        /// </summary>
        AgentResourceRequirements GetResourceRequirements();

        /// <summary>
        /// Shutdown the agent gracefully
        /// </summary>
        Task Shutdown();
    }

    public enum AgentState
    {
        Uninitialized,
        Ready,
        Active,
        Optimizing,
        Monitoring,
        Error,
        Paused,
        Shutdown
    }

    public class AgentRecommendation
    {
        public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;  // Explain WHY we recommend this

        public List<string> ActionsToTake { get; set; } = new();
        public Dictionary<string, object> ActionParameters { get; set; } = new();

        public double Confidence { get; set; }  // 0.0 to 1.0
        public double ExpectedImprovement { get; set; }  // % improvement expected
        public string OptimizationMetric { get; set; } = string.Empty;  // What are we optimizing? FPS, Latency, etc.

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool AutoApply { get; set; } = false;  // Should this be auto-applied?
    }

    public class AgentActionResult
    {
        public bool Success { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public double Improvement { get; set; }  // Actual improvement achieved
        public Dictionary<string, object> Metrics { get; set; } = new();
        public DateTime ExecutedAt { get; set; } = DateTime.Now;
    }

    public class AgentFeedback
    {
        public string FeedbackType { get; set; } = string.Empty;  // "Success", "PartialSuccess", "Failure", "UserRejected"
        public string Description { get; set; } = string.Empty;
        public double MeasuredImprovement { get; set; }  // Actual improvement vs expected
        public string UserFeedback { get; set; } = string.Empty;  // "feels better", "no difference", "worse"
        public DateTime FeedbackTime { get; set; } = DateTime.Now;
    }

    public class AgentResourceRequirements
    {
        public string AgentType { get; set; } = string.Empty;
        public double CPUPercentage { get; set; }  // % of CPU this agent needs
        public double GPUPercentage { get; set; }  // % of GPU this agent needs
        public double RAMPercentage { get; set; }  // % of RAM this agent needs
        public double NetworkPercentage { get; set; }  // % of network bandwidth
        public double StorageIOPercentage { get; set; }  // % of storage I/O
        public double Priority { get; set; } = 0.5;  // 0.0-1.0, default medium
        public bool RequiresElevation { get; set; } = false;  // Needs admin rights
        public List<string> ConflictsWith { get; set; } = new();  // Other agents this conflicts with
    }

    public class AgentKnowledge
    {
        public string Topic { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;  // "GameOptimization", "NetworkTuning", etc.
        public Dictionary<string, double> SuccessRates { get; set; } = new();  // "Action" -> success rate
        public Dictionary<string, double> UserPreferences { get; set; } = new();  // What this user likes
        public List<LearnedPattern> Patterns { get; set; } = new();  // Patterns learned from past experience
    }

    public class LearnedPattern
    {
        public string Condition { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public double SuccessRate { get; set; }
        public int ObservedTimes { get; set; }
        public DateTime FirstObserved { get; set; }
        public DateTime LastObserved { get; set; }
    }
}

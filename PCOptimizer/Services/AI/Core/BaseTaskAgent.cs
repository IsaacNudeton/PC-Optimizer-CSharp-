using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PCOptimizer.Services.AI.Core
{
    /// <summary>
    /// Abstract base class for all AI agents
    /// Provides common functionality: learning, reasoning, state management
    /// </summary>
    public abstract class BaseTaskAgent : ITaskAgent
    {
        public string AgentId { get; protected set; }
        public string AgentName { get; protected set; }
        public string AgentType { get; protected set; }
        public AgentState CurrentState { get; protected set; } = AgentState.Uninitialized;
        public double ConfidenceScore { get; protected set; } = 0.5;

        protected SystemSnapshot _systemContext = new();
        protected AgentKnowledge _knowledge = new();
        protected List<AgentActionResult> _actionHistory = new();
        protected Dictionary<string, double> _currentMetrics = new();
        protected AgentResourceRequirements _resourceRequirements = new();
        protected double _resourcePriority = 0.5;

        protected BaseTaskAgent()
        {
            AgentId = Guid.NewGuid().ToString();
            Console.WriteLine($"[Agent] Created: {AgentType} ({AgentId})");
        }

        public virtual async Task Initialize(SystemSnapshot systemContext)
        {
            _systemContext = systemContext;
            CurrentState = AgentState.Ready;
            Console.WriteLine($"[{AgentType}] Initialized with system context: {systemContext.CPUModel} + {systemContext.GPUModel}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// The core reasoning engine - pure logic without side effects
        /// This is where the AI "thinks"
        /// </summary>
        public abstract Task<AgentRecommendation> Reason(string scenario, Dictionary<string, object> context);

        /// <summary>
        /// Execute an action and track results for learning
        /// </summary>
        public async Task<AgentActionResult> ExecuteAction(string actionName, Dictionary<string, object> parameters)
        {
            try
            {
                CurrentState = AgentState.Optimizing;
                Console.WriteLine($"[{AgentType}] Executing: {actionName}");

                var result = new AgentActionResult
                {
                    ActionName = actionName,
                    Success = false
                };

                // Call the derived class's implementation
                result = await ExecuteActionInternal(actionName, parameters);

                _actionHistory.Add(result);
                CurrentState = AgentState.Active;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{AgentType}] Error executing action: {ex.Message}");
                CurrentState = AgentState.Error;
                return new AgentActionResult { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// Override this in derived classes to implement specific actions
        /// </summary>
        protected abstract Task<AgentActionResult> ExecuteActionInternal(string actionName, Dictionary<string, object> parameters);

        /// <summary>
        /// Learn from feedback - update internal knowledge base
        /// This is how the agent improves over time
        /// </summary>
        public virtual async Task Learn(string scenario, AgentFeedback feedback)
        {
            Console.WriteLine($"[{AgentType}] Learning: {scenario} -> {feedback.FeedbackType}");

            // Update success rates
            if (_knowledge.SuccessRates.ContainsKey(scenario))
            {
                // Weighted average: give more weight to recent feedback
                var oldRate = _knowledge.SuccessRates[scenario];
                var isSuccess = feedback.FeedbackType == "Success" || feedback.FeedbackType == "PartialSuccess" ? 1.0 : 0.0;
                _knowledge.SuccessRates[scenario] = (oldRate * 0.7) + (isSuccess * 0.3);
            }
            else
            {
                var isSuccess = feedback.FeedbackType == "Success" || feedback.FeedbackType == "PartialSuccess" ? 1.0 : 0.0;
                _knowledge.SuccessRates[scenario] = isSuccess;
            }

            // Update confidence score based on overall success rate
            if (_knowledge.SuccessRates.Any())
            {
                ConfidenceScore = _knowledge.SuccessRates.Values.Average();
            }

            // Store pattern if learning rate was good
            if (feedback.MeasuredImprovement > feedback.MeasuredImprovement * 0.8)
            {
                var existingPattern = _knowledge.Patterns.FirstOrDefault(p => p.Condition == scenario);
                if (existingPattern != null)
                {
                    existingPattern.SuccessRate = (existingPattern.SuccessRate * 0.8) + (feedback.MeasuredImprovement * 0.2);
                    existingPattern.ObservedTimes++;
                    existingPattern.LastObserved = DateTime.Now;
                }
                else
                {
                    _knowledge.Patterns.Add(new LearnedPattern
                    {
                        Condition = scenario,
                        SuccessRate = feedback.MeasuredImprovement,
                        ObservedTimes = 1,
                        FirstObserved = DateTime.Now,
                        LastObserved = DateTime.Now
                    });
                }
            }

            Console.WriteLine($"[{AgentType}] Confidence updated to: {ConfidenceScore:F2}");
            await Task.CompletedTask;
        }

        public async Task<Dictionary<string, double>> GetCurrentMetrics()
        {
            return await Task.FromResult(_currentMetrics);
        }

        public async Task SetResourcePriority(double priority)
        {
            _resourcePriority = Math.Clamp(priority, 0.0, 1.0);
            _resourceRequirements.Priority = _resourcePriority;
            Console.WriteLine($"[{AgentType}] Priority set to: {_resourcePriority:F2}");
            await Task.CompletedTask;
        }

        public AgentResourceRequirements GetResourceRequirements()
        {
            _resourceRequirements.Priority = _resourcePriority;
            return _resourceRequirements;
        }

        public virtual async Task Shutdown()
        {
            CurrentState = AgentState.Shutdown;
            Console.WriteLine($"[{AgentType}] Shutting down...");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Helper: Get current bottleneck in the system
        /// Used by agents to make intelligent decisions
        /// </summary>
        protected string DetectBottleneck(SystemSnapshot system)
        {
            // Simple heuristic: what's the main constraint?
            if (system.CurrentGPUTemp > 85 || system.GPUVRAM > system.GPUVRAM * 0.9)
                return "GPU";
            if (system.CurrentCPUTemp > 85 || system.CPUCores < 8)
                return "CPU";
            if (system.TotalRAM < 16)
                return "RAM";
            if (system.StorageType == "HDD")
                return "Storage";
            return "Balanced";
        }

        /// <summary>
        /// Helper: Calculate expected improvement for an action
        /// Based on learned patterns and system state
        /// </summary>
        protected double CalculateExpectedImprovement(string action)
        {
            if (_knowledge.SuccessRates.ContainsKey(action))
            {
                return _knowledge.SuccessRates[action] * 100;  // Convert to percentage
            }
            return 20.0;  // Default expectation
        }
    }
}

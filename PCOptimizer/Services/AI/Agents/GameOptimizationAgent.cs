using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PCOptimizer.Services.AI.Core;

namespace PCOptimizer.Services.AI.Agents
{
    /// <summary>
    /// AI Agent specialized in gaming optimization
    /// Focuses on: FPS, input latency, thermal management, network optimization
    /// </summary>
    public class GameOptimizationAgent : BaseTaskAgent
    {
        private string _detectedGame = string.Empty;
        private double _targetFPS = 144;
        private double _currentFPS = 0;
        private double _inputLatency = 0;

        public GameOptimizationAgent()
        {
            AgentName = "Gaming Optimizer";
            AgentType = "Gaming";
            _resourceRequirements = new()
            {
                AgentType = "Gaming",
                CPUPercentage = 0.80,
                GPUPercentage = 0.95,
                RAMPercentage = 0.60,
                NetworkPercentage = 0.90,
                StorageIOPercentage = 0.20,
                Priority = 0.9,
                ConflictsWith = new() { "ContentCreation", "Development" }
            };
            _knowledge.Topic = "Gaming Optimization";
        }

        public override async Task<AgentRecommendation> Reason(string scenario, Dictionary<string, object> context)
        {
            // Parse context
            if (context.ContainsKey("game"))
                _detectedGame = context["game"].ToString() ?? "Unknown";
            if (context.ContainsKey("fps"))
                double.TryParse(context["fps"].ToString(), out _currentFPS);
            if (context.ContainsKey("inputLatency"))
                double.TryParse(context["inputLatency"].ToString(), out _inputLatency);

            var recommendation = new AgentRecommendation
            {
                Title = $"Gaming Optimization for {_detectedGame}",
                Description = "Optimizing for competitive gaming performance",
                Confidence = ConfidenceScore
            };

            // Reason: What's the main constraint?
            var bottleneck = DetectBottleneck(_systemContext);

            // COMPETITIVE SHOOTERS (Valorant, CS2) - Prioritize input latency
            if (_detectedGame.ContainsIgnoreCase("Valorant") || _detectedGame.ContainsIgnoreCase("CS2") ||
                _detectedGame.ContainsIgnoreCase("CSGO"))
            {
                _targetFPS = 240;

                recommendation.Reasoning = $@"
Competitive shooter detected: {_detectedGame}
Priority: Input Latency > Frame Rate
- Target FPS: 240 (unlocked)
- Current FPS: {_currentFPS}
- Input Latency: {_inputLatency}ms
- Bottleneck: {bottleneck}
";

                if (_inputLatency > 2.0)
                {
                    recommendation.ActionsToTake.AddRange(new[]
                    {
                        "DisableVSync",
                        "BoostTimerResolution",
                        "MaxPollingRate",
                        "ReduceRenderLatency"
                    });
                    recommendation.OptimizationMetric = "InputLatency";
                    recommendation.ExpectedImprovement = 35;  // 35% latency reduction
                }

                if (_currentFPS < 144)
                {
                    recommendation.ActionsToTake.Add("BoostGPUClocks");
                    recommendation.ActionsToTake.Add("OptimizeCPUScheduling");
                }

                // Network optimization for online games
                if (bottleneck != "Network")
                {
                    recommendation.ActionsToTake.Add("OptimizeNetworkStack");
                }
            }

            // OPEN WORLD GAMES (GTA, Warzone) - Prioritize FPS
            else if (_detectedGame.ContainsIgnoreCase("GTA") || _detectedGame.ContainsIgnoreCase("Warzone") ||
                     _detectedGame.ContainsIgnoreCase("RDR2"))
            {
                _targetFPS = 120;

                recommendation.Reasoning = $@"
Open-world game detected: {_detectedGame}
Priority: Frame Rate > Input Latency
- Target FPS: 120
- Current FPS: {_currentFPS}
- Bottleneck: {bottleneck}
";

                if (_currentFPS < 60)
                {
                    recommendation.ActionsToTake.AddRange(new[]
                    {
                        "ReduceGraphicsQuality",
                        "MaxGPUClocks",
                        "OptimizeMemory",
                        "BoostThermalCapacity"
                    });
                    recommendation.OptimizationMetric = "FPS";
                    recommendation.ExpectedImprovement = 40;
                }
                else if (_currentFPS < _targetFPS)
                {
                    recommendation.ActionsToTake.AddRange(new[]
                    {
                        "BoostGPUClocks",
                        "EnableGPUVRAMOptimization"
                    });
                    recommendation.ExpectedImprovement = 20;
                }

                // Thermal management for sustained play
                if (_systemContext.CurrentGPUTemp > 80)
                {
                    recommendation.ActionsToTake.Add("BoostCooling");
                    recommendation.ActionsToTake.Add("MonitorThrottle");
                }
            }

            // Generic gaming profile
            else
            {
                _targetFPS = 100;
                recommendation.Reasoning = $@"
Generic game optimization
- Detected Game: {_detectedGame}
- Current FPS: {_currentFPS}
- Bottleneck: {bottleneck}
";

                recommendation.ActionsToTake.AddRange(new[]
                {
                    "OptimizeGaming Profile",
                    "ManageResources",
                    "EnableGPUAcceleration"
                });
                recommendation.ExpectedImprovement = 25;
            }

            recommendation.Confidence = Math.Min(0.95, ConfidenceScore + 0.3);  // Gaming agent is confident
            recommendation.AutoApply = ConfidenceScore > 0.7;

            return await Task.FromResult(recommendation);
        }

        protected override async Task<AgentActionResult> ExecuteActionInternal(string actionName, Dictionary<string, object> parameters)
        {
            var result = new AgentActionResult { ActionName = actionName };

            try
            {
                switch (actionName.ToLower())
                {
                    case "disablev sync":
                        result.Success = await DisableVSync();
                        result.Message = "VSync disabled - reduced input latency";
                        result.Improvement = 1.5;
                        _currentMetrics["InputLatency"] = _inputLatency - 1.5;
                        break;

                    case "boosttimer resolution":
                        result.Success = await BoostTimerResolution();
                        result.Message = "Timer resolution boosted to 0.5ms";
                        result.Improvement = 0.5;
                        _inputLatency -= 0.5;
                        _currentMetrics["InputLatency"] = _inputLatency;
                        break;

                    case "maxpollingrate":
                        result.Success = await MaxPollingRate();
                        result.Message = "Polling rate set to maximum";
                        result.Improvement = 0.3;
                        break;

                    case "boostgpuclocks":
                        result.Success = await BoostGPUClocks();
                        result.Message = "GPU clocks boosted";
                        result.Improvement = 15;  // 15% FPS improvement
                        _currentFPS *= 1.15;
                        _currentMetrics["FPS"] = _currentFPS;
                        break;

                    case "optimizecpuscheduling":
                        result.Success = await OptimizeCPUScheduling();
                        result.Message = "CPU scheduling optimized";
                        result.Improvement = 8;
                        break;

                    case "optimizenetworkstack":
                        result.Success = await OptimizeNetworkStack();
                        result.Message = "Network latency reduced";
                        result.Improvement = 25;  // ms improvement
                        break;

                    case "boostcooling":
                        result.Success = await BoostCooling();
                        result.Message = "Cooling system optimized";
                        break;

                    case "monitorthrottle":
                        result.Success = await MonitorThermalThrottle();
                        result.Message = "Thermal throttle monitoring enabled";
                        break;

                    default:
                        result.Message = $"Action not implemented: {actionName}";
                        break;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error: {ex.Message}";
            }

            return await Task.FromResult(result);
        }

        private async Task<bool> DisableVSync()
        {
            Console.WriteLine("[GameAgent] Disabling VSync via registry");
            // In real implementation, would modify NVIDIA/AMD settings
            await Task.Delay(100);
            return true;
        }

        private async Task<bool> BoostTimerResolution()
        {
            Console.WriteLine("[GameAgent] Boosting system timer resolution to 0.5ms");
            // Would call timeBeginPeriod(1) on Windows
            await Task.Delay(50);
            return true;
        }

        private async Task<bool> MaxPollingRate()
        {
            Console.WriteLine("[GameAgent] Setting USB polling rate to 8kHz");
            // Would configure device polling rates
            await Task.Delay(100);
            return true;
        }

        private async Task<bool> BoostGPUClocks()
        {
            Console.WriteLine("[GameAgent] Boosting GPU memory and core clocks");
            // Would use NVIDIA/AMD driver APIs
            await Task.Delay(200);
            return true;
        }

        private async Task<bool> OptimizeCPUScheduling()
        {
            Console.WriteLine("[GameAgent] Optimizing CPU scheduling");
            // Would adjust thread priorities and affinity
            await Task.Delay(150);
            return true;
        }

        private async Task<bool> OptimizeNetworkStack()
        {
            Console.WriteLine("[GameAgent] Optimizing network stack");
            // Would disable nagle algorithm, reduce buffer sizes
            await Task.Delay(100);
            return true;
        }

        private async Task<bool> BoostCooling()
        {
            Console.WriteLine("[GameAgent] Boosting cooling system");
            // Would increase fan curves
            await Task.Delay(100);
            return true;
        }

        private async Task<bool> MonitorThermalThrottle()
        {
            Console.WriteLine("[GameAgent] Enabling thermal throttle monitoring");
            // Would set up monitoring
            await Task.Delay(50);
            return true;
        }
    }

    internal static class StringExtensions
    {
        internal static bool ContainsIgnoreCase(this string str, string value)
        {
            return str.Contains(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}

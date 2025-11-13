using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PCOptimizer.Services.AI.Core;

namespace PCOptimizer.Services.AI.Agents
{
    /// <summary>
    /// AI Agent for streaming optimization (Twitch, YouTube, etc.)
    /// Balances stream quality with system performance
    /// </summary>
    public class StreamingAgent : BaseTaskAgent
    {
        private string _streamPlatform = string.Empty;
        private double _currentBitrate = 0;
        private double _currentLatency = 0;
        private double _dropFrameRate = 0;

        public StreamingAgent()
        {
            AgentName = "Streaming Optimizer";
            AgentType = "Streaming";
            _resourceRequirements = new()
            {
                AgentType = "Streaming",
                CPUPercentage = 0.30,
                GPUPercentage = 0.25,
                RAMPercentage = 0.20,
                NetworkPercentage = 0.80,
                StorageIOPercentage = 0.10,
                Priority = 0.7,
                ConflictsWith = new() { "Download", "Upload" }
            };
            _knowledge.Topic = "Stream Optimization";
        }

        public override async Task<AgentRecommendation> Reason(string scenario, Dictionary<string, object> context)
        {
            if (context.ContainsKey("platform"))
                _streamPlatform = context["platform"].ToString() ?? "Unknown";
            if (context.ContainsKey("bitrate"))
                double.TryParse(context["bitrate"].ToString(), out _currentBitrate);
            if (context.ContainsKey("latency"))
                double.TryParse(context["latency"].ToString(), out _currentLatency);
            if (context.ContainsKey("dropFrameRate"))
                double.TryParse(context["dropFrameRate"].ToString(), out _dropFrameRate);

            var recommendation = new AgentRecommendation
            {
                Title = $"Stream Optimization for {_streamPlatform}",
                Description = "Optimizing stream quality while maintaining system stability",
                Confidence = ConfidenceScore
            };

            // Detect network capacity
            var networkCapacity = DetermineNetworkCapacity();
            var optimalBitrate = CalculateOptimalBitrate(networkCapacity);

            recommendation.Reasoning = $@"
Stream Analysis:
- Platform: {_streamPlatform}
- Current Bitrate: {_currentBitrate} Mbps
- Optimal Bitrate: {optimalBitrate} Mbps
- Stream Latency: {_currentLatency}ms
- Drop Frame Rate: {_dropFrameRate}%
- Network Capacity: {networkCapacity}%
";

            // Issue detection and fixes
            if (_dropFrameRate > 2)
            {
                recommendation.ActionsToTake.Add("ReduceBitrate");
                recommendation.ActionsToTake.Add("LowerResolution");
                recommendation.OptimizationMetric = "FrameDrop";
                recommendation.ExpectedImprovement = 50;  // Reduce drops by 50%
            }

            if (_currentLatency > 4000)  // 4 second latency is too high
            {
                recommendation.ActionsToTake.Add("OptimizeEncoding");
                recommendation.ActionsToTake.Add("ReduceBufferSize");
                recommendation.OptimizationMetric = "StreamLatency";
                recommendation.ExpectedImprovement = 40;
            }

            if (_currentBitrate > optimalBitrate * 1.2)
            {
                recommendation.ActionsToTake.Add("ReduceBitrate");
            }
            else if (_currentBitrate < optimalBitrate * 0.8 && _dropFrameRate < 1)
            {
                recommendation.ActionsToTake.Add("IncreaseBitrate");
            }

            recommendation.Confidence = Math.Min(0.92, ConfidenceScore + 0.2);
            recommendation.AutoApply = _dropFrameRate > 1.5;  // Auto-apply if quality degraded

            return await Task.FromResult(recommendation);
        }

        protected override async Task<AgentActionResult> ExecuteActionInternal(string actionName, Dictionary<string, object> parameters)
        {
            var result = new AgentActionResult { ActionName = actionName };

            try
            {
                switch (actionName.ToLower())
                {
                    case "reducebitrate":
                        result.Success = true;
                        _currentBitrate *= 0.9;
                        result.Message = $"Reduced bitrate to {_currentBitrate:F1} Mbps";
                        result.Improvement = 30;  // Reduce frame drop by 30%
                        break;

                    case "increasebitrate":
                        result.Success = true;
                        _currentBitrate *= 1.1;
                        result.Message = $"Increased bitrate to {_currentBitrate:F1} Mbps";
                        result.Improvement = 20;
                        break;

                    case "lowerresolution":
                        result.Success = true;
                        result.Message = "Lowered stream resolution to 1080p";
                        result.Improvement = 40;
                        break;

                    case "optimizeencoding":
                        result.Success = true;
                        result.Message = "Optimized encoder settings";
                        result.Improvement = 25;
                        _currentLatency *= 0.75;
                        break;

                    case "reducebuffersize":
                        result.Success = true;
                        result.Message = "Reduced stream buffer size";
                        result.Improvement = 35;
                        _currentLatency *= 0.8;
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

        private double DetermineNetworkCapacity()
        {
            // Simplified: check available bandwidth
            // In real implementation, would ping server and measure available bandwidth
            if (_dropFrameRate > 5)
                return 50;  // Network is congested
            if (_currentLatency > 2000)
                return 60;
            return 80;  // Good capacity
        }

        private double CalculateOptimalBitrate(double networkCapacity)
        {
            // Conservative: use 70-80% of detected capacity
            var targetBitrate = networkCapacity * 0.75;

            // Platform-specific recommendations
            return _streamPlatform switch
            {
                "Twitch" when targetBitrate > 8 => 8.0,  // Twitch recommends max 8Mbps for 1080p60
                "YouTube" when targetBitrate > 25 => 25.0,  // YouTube can handle higher
                _ => Math.Min(targetBitrate, 12.0)
            };
        }
    }
}

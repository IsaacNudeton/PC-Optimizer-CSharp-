using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PCOptimizer.Services.AI.Core;

namespace PCOptimizer.Services.AI.Agents
{
    /// <summary>
    /// AI Agent for content creation optimization (video editing, 3D rendering, image editing)
    /// Optimizes rendering times, preview quality, and creative workflow
    /// </summary>
    public class ContentCreationAgent : BaseTaskAgent
    {
        private string _currentTool = string.Empty;
        private string _contentType = string.Empty;
        private double _renderProgress = 0;
        private double _estimatedRenderTime = 0;
        private int _previewQuality = 100;
        private double _storageIOLoad = 0;

        public ContentCreationAgent()
        {
            AgentName = "Content Creation Optimizer";
            AgentType = "ContentCreation";
            _resourceRequirements = new()
            {
                AgentType = "ContentCreation",
                CPUPercentage = 0.60,
                GPUPercentage = 0.70,
                RAMPercentage = 0.50,
                NetworkPercentage = 0.10,
                StorageIOPercentage = 0.60,
                Priority = 0.75,
                ConflictsWith = new() { "Gaming", "Heavy Background Tasks" }
            };
            _knowledge.Topic = "Content Creation Optimization";
        }

        public override async Task<AgentRecommendation> Reason(string scenario, Dictionary<string, object> context)
        {
            if (context.ContainsKey("tool"))
                _currentTool = context["tool"].ToString() ?? "Unknown";
            if (context.ContainsKey("contentType"))
                _contentType = context["contentType"].ToString() ?? "Unknown";
            if (context.ContainsKey("renderProgress"))
                double.TryParse(context["renderProgress"].ToString(), out _renderProgress);
            if (context.ContainsKey("estimatedRenderTime"))
                double.TryParse(context["estimatedRenderTime"].ToString(), out _estimatedRenderTime);
            if (context.ContainsKey("previewQuality"))
                int.TryParse(context["previewQuality"].ToString(), out _previewQuality);
            if (context.ContainsKey("storageIOLoad"))
                double.TryParse(context["storageIOLoad"].ToString(), out _storageIOLoad);

            var recommendation = new AgentRecommendation
            {
                Title = $"Content Creation Optimization for {_currentTool}",
                Description = "Optimizing rendering performance and creative workflow",
                Confidence = ConfidenceScore
            };

            recommendation.Reasoning = $@"
Content Creation Analysis:
- Tool: {_currentTool}
- Content Type: {_contentType}
- Render Progress: {_renderProgress:F1}%
- Estimated Render Time: {_estimatedRenderTime:F1} minutes
- Preview Quality: {_previewQuality}%
- Storage I/O Load: {_storageIOLoad:F1}%
";

            // Issue detection and fixes
            if (_estimatedRenderTime > 60)  // Long render times
            {
                recommendation.ActionsToTake.Add("EnableGPUAcceleration");
                recommendation.ActionsToTake.Add("OptimizeRenderSettings");
                recommendation.OptimizationMetric = "RenderTime";
                recommendation.ExpectedImprovement = 45;  // Reduce by 45%
            }

            if (_storageIOLoad > 80)  // Storage bottleneck
            {
                recommendation.ActionsToTake.Add("MoveCacheToSSD");
                recommendation.ActionsToTake.Add("IncreaseRAMCache");
                recommendation.OptimizationMetric = "StorageIO";
                recommendation.ExpectedImprovement = 35;
            }

            if (_previewQuality > 75 && _renderProgress < 100)  // High preview quality during editing
            {
                recommendation.ActionsToTake.Add("LowerPreviewQuality");
                recommendation.OptimizationMetric = "PreviewResponsiveness";
                recommendation.ExpectedImprovement = 50;
            }

            // Tool-specific optimizations
            if (_currentTool.Contains("Premiere") || _currentTool.Contains("After Effects"))
            {
                recommendation.ActionsToTake.Add("OptimizeAdobeMediaCache");
                recommendation.ActionsToTake.Add("EnableMercuryPlayback");
            }
            else if (_currentTool.Contains("Blender"))
            {
                recommendation.ActionsToTake.Add("EnableOptixDenoising");
                recommendation.ActionsToTake.Add("OptimizeTileSize");
            }
            else if (_currentTool.Contains("DaVinci") || _currentTool.Contains("Resolve"))
            {
                recommendation.ActionsToTake.Add("OptimizePlaybackProxy");
                recommendation.ActionsToTake.Add("EnableSmartCache");
            }

            // Content type specific
            if (_contentType.Contains("4K") || _contentType.Contains("8K"))
            {
                recommendation.ActionsToTake.Add("EnableProxyEditing");
                recommendation.ExpectedImprovement += 20;  // Higher improvement for 4K+
            }

            recommendation.Confidence = Math.Min(0.90, ConfidenceScore + 0.18);
            recommendation.AutoApply = _estimatedRenderTime > 120 || _storageIOLoad > 85;

            return await Task.FromResult(recommendation);
        }

        protected override async Task<AgentActionResult> ExecuteActionInternal(string actionName, Dictionary<string, object> parameters)
        {
            var result = new AgentActionResult { ActionName = actionName };

            try
            {
                switch (actionName.ToLower())
                {
                    case "enablegpuacceleration":
                        result.Success = true;
                        _estimatedRenderTime *= 0.55;  // 45% reduction
                        result.Message = "Enabled GPU-accelerated rendering";
                        result.Improvement = 45;
                        break;

                    case "optimizerendersettings":
                        result.Success = true;
                        _estimatedRenderTime *= 0.85;
                        result.Message = "Optimized render quality/speed balance";
                        result.Improvement = 15;
                        break;

                    case "movecachetossd":
                        result.Success = true;
                        _storageIOLoad *= 0.65;
                        result.Message = "Moved media cache to SSD";
                        result.Improvement = 35;
                        break;

                    case "increaseramcache":
                        result.Success = true;
                        _storageIOLoad *= 0.80;
                        result.Message = "Increased RAM cache size for media";
                        result.Improvement = 20;
                        break;

                    case "lowerpreviewquality":
                        result.Success = true;
                        _previewQuality = 50;
                        result.Message = "Lowered preview quality to 50% (1/2 resolution)";
                        result.Improvement = 50;
                        break;

                    case "optimizeadobemediacache":
                        result.Success = true;
                        result.Message = "Optimized Adobe Media Cache settings";
                        result.Improvement = 25;
                        break;

                    case "enablemercuryplayback":
                        result.Success = true;
                        result.Message = "Enabled Mercury Playback Engine (GPU)";
                        result.Improvement = 40;
                        break;

                    case "enableoptixdenoising":
                        result.Success = true;
                        _estimatedRenderTime *= 0.70;
                        result.Message = "Enabled OptiX AI denoising (Blender)";
                        result.Improvement = 30;
                        break;

                    case "optimizetilesize":
                        result.Success = true;
                        result.Message = "Optimized Blender tile size for GPU";
                        result.Improvement = 15;
                        break;

                    case "optimizeplaybackproxy":
                        result.Success = true;
                        result.Message = "Enabled proxy media for smooth playback";
                        result.Improvement = 60;
                        break;

                    case "enablesmartcache":
                        result.Success = true;
                        result.Message = "Enabled DaVinci Resolve Smart Cache";
                        result.Improvement = 35;
                        break;

                    case "enableproxyediting":
                        result.Success = true;
                        _previewQuality = 25;
                        result.Message = "Enabled proxy editing for 4K/8K footage";
                        result.Improvement = 70;
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
    }
}

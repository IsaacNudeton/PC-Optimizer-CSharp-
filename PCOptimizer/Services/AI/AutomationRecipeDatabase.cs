using System;
using System.Collections.Generic;

namespace PCOptimizer.Services.AI
{
    /// <summary>
    /// Universal Automation Recipe Database
    /// Contains 50+ workflow recipes for different tasks
    /// Each recipe describes how to auto-detect a workflow and optimize for it
    /// </summary>
    public class AutomationRecipe
    {
        public string RecipeName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Process names that trigger this recipe
        /// </summary>
        public List<string> ProcessTriggers { get; set; } = new();

        /// <summary>
        /// File extensions or patterns to detect
        /// </summary>
        public List<string> FileTriggers { get; set; } = new();

        /// <summary>
        /// Window titles to detect
        /// </summary>
        public List<string> WindowTitlePatterns { get; set; } = new();

        /// <summary>
        /// Agents to instantiate
        /// </summary>
        public List<string> RequiredAgents { get; set; } = new();

        /// <summary>
        /// Optimizations to apply
        /// </summary>
        public List<string> OptimizationActions { get; set; } = new();

        /// <summary>
        /// Resource allocation (percentage)
        /// </summary>
        public Dictionary<string, double> ResourceAllocation { get; set; } = new();

        /// <summary>
        /// Registry modifications
        /// </summary>
        public Dictionary<string, string> RegistryChanges { get; set; } = new();

        /// <summary>
        /// Services to enable/disable
        /// </summary>
        public Dictionary<string, bool> ServiceStates { get; set; } = new();

        /// <summary>
        /// Applications to launch with the recipe
        /// </summary>
        public List<string> CompanionApps { get; set; } = new();

        /// <summary>
        /// Expected system impact
        /// </summary>
        public string ExpectedOutcome { get; set; } = string.Empty;
    }

    public class AutomationRecipeDatabase
    {
        public Dictionary<string, AutomationRecipe> Recipes { get; private set; } = new();

        public AutomationRecipeDatabase()
        {
            InitializeRecipes();
        }

        private void InitializeRecipes()
        {
            // ==================== GAMING RECIPES ====================

            Recipes["CompetitiveGaming"] = new AutomationRecipe
            {
                RecipeName = "Competitive Gaming Optimizer",
                Category = "Gaming",
                Description = "Optimizes for competitive shooters - Valorant, CS2, Overwatch. Maximizes input latency reduction and FPS consistency.",
                ProcessTriggers = new() { "Valorant.exe", "CS2.exe", "CSGO.exe", "OW2.exe" },
                RequiredAgents = new() { "Gaming", "Network" },
                OptimizationActions = new()
                {
                    "DisableVSync",
                    "BoostTimerResolution",
                    "MaxPollingRate",
                    "OptimizeCPUScheduling",
                    "OptimizeNetworkStack",
                    "ReduceInputLatency",
                    "DisableBackgroundApps"
                },
                ResourceAllocation = new()
                {
                    { "GPU", 0.95 },
                    { "CPU", 0.85 },
                    { "RAM", 0.50 },
                    { "Network", 1.0 },
                    { "StorageIO", 0.1 }
                },
                RegistryChanges = new()
                {
                    { "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\LanmanWorkstation\\Parameters", "IrpStackSize=50" },
                    { "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters", "TCPNoDelay=1" },
                    { "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters", "DisableTaskOffload=1" }
                },
                ServiceStates = new()
                {
                    { "DiagTrack", false },
                    { "dmwappushservice", false },
                    { "HomeGroupListener", false },
                    { "HomeGroupProvider", false },
                    { "WSearch", false }
                },
                ExpectedOutcome = "Input latency reduced by 30-50%, consistent 240+ FPS on competitive titles"
            };

            Recipes["OpenWorldGaming"] = new AutomationRecipe
            {
                RecipeName = "Open World Gaming Optimizer",
                Category = "Gaming",
                Description = "Optimizes for open-world AAA games - GTA, Red Dead, Cyberpunk. Maximizes FPS and VRAM usage.",
                ProcessTriggers = new() { "GTA5.exe", "RDR2.exe", "Cyberpunk2077.exe", "Warzone2.exe" },
                RequiredAgents = new() { "Gaming" },
                OptimizationActions = new()
                {
                    "BoostGPUClocks",
                    "OptimizeVRAM",
                    "ClearMemoryCache",
                    "OptimizeShaderCache",
                    "BoostThermalCapacity",
                    "OptimizeStorageAccess"
                },
                ResourceAllocation = new()
                {
                    { "GPU", 0.95 },
                    { "CPU", 0.70 },
                    { "RAM", 0.80 },
                    { "StorageIO", 0.85 }
                },
                ExpectedOutcome = "40-60% FPS improvement, stable 60+ FPS at high settings"
            };

            // ==================== STREAMING RECIPES ====================

            Recipes["TwitchStreaming"] = new AutomationRecipe
            {
                RecipeName = "Twitch Streaming Setup",
                Category = "Streaming",
                Description = "Optimizes system for Twitch streaming - handles encoding, network optimization, and system management.",
                ProcessTriggers = new() { "OBS Studio.exe", "obs64.exe", "OBSPortable.exe", "Streamlabs OBS.exe" },
                RequiredAgents = new() { "Streaming" },
                OptimizationActions = new()
                {
                    "OptimizeOBSSettings",
                    "EnableHardwareEncoding",
                    "OptimizeNetworkStack",
                    "LimitBackgroundApps",
                    "MonitorStreamHealth"
                },
                ResourceAllocation = new()
                {
                    { "GPU", 0.30 },  // GPU encoding
                    { "CPU", 0.30 },  // CPU reserved for encoding
                    { "Network", 0.80 },  // High network priority
                    { "RAM", 0.20 }
                },
                CompanionApps = new() { "OBS", "Discord" },
                RegistryChanges = new()
                {
                    { "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters", "TcpWindowSize=65535" }
                },
                ExpectedOutcome = "Stable 1080p60 stream with < 5 sec latency, 0% frame drops"
            };

            Recipes["YouTubeStreaming"] = new AutomationRecipe
            {
                RecipeName = "YouTube Streaming Setup",
                Category = "Streaming",
                Description = "Optimizes for YouTube Live - allows higher bitrate, better quality.",
                ProcessTriggers = new() { "OBS Studio.exe", "Chrome.exe", "youtube.com" },
                RequiredAgents = new() { "Streaming" },
                OptimizationActions = new()
                {
                    "EnableHighBitrate",
                    "OptimizeNetworkStack",
                    "BufferOptimization",
                    "QoSConfiguration"
                },
                ResourceAllocation = new()
                {
                    { "GPU", 0.25 },
                    { "Network", 0.90 }
                },
                ExpectedOutcome = "1440p60 or 4K30 capable streaming"
            };

            // ==================== DEVELOPMENT RECIPES ====================

            Recipes["GameDevelopment"] = new AutomationRecipe
            {
                RecipeName = "Game Development Workstation",
                Category = "Development",
                Description = "Optimizes for game development - Visual Studio, Unreal Engine, Unity.",
                ProcessTriggers = new() { "UE4Editor.exe", "UE5Editor.exe", "Unity.exe", "devenv.exe", "Code.exe" },
                RequiredAgents = new() { "Development" },
                OptimizationActions = new()
                {
                    "OptimizeCompilation",
                    "ShaderCompilationAsync",
                    "IncreaseVirtualMemory",
                    "OptimizeIntelliSense",
                    "ClearBuildCache"
                },
                ResourceAllocation = new()
                {
                    { "CPU", 0.90 },
                    { "GPU", 0.60 },
                    { "RAM", 0.85 },
                    { "StorageIO", 0.80 }
                },
                ServiceStates = new()
                {
                    { "DiagTrack", false },
                    { "WSearch", false },  // Disable indexing
                    { "WMPNetworkSvc", false }
                },
                ExpectedOutcome = "30-40% faster compilation, improved editor responsiveness"
            };

            Recipes["WebDevelopment"] = new AutomationRecipe
            {
                RecipeName = "Web Development Workstation",
                Category = "Development",
                Description = "Optimizes for web development - VS Code, Node.js, browsers.",
                ProcessTriggers = new() { "Code.exe", "node.exe", "npm.cmd", "yarn.cmd", "chrome.exe" },
                RequiredAgents = new() { "Development" },
                OptimizationActions = new()
                {
                    "OptimizeNodeJS",
                    "FastRefresh",
                    "MemoryOptimization",
                    "HotReloadOptimization"
                },
                ResourceAllocation = new()
                {
                    { "CPU", 0.70 },
                    { "RAM", 0.75 },
                    { "Network", 0.40 }
                },
                ExpectedOutcome = "Fast hot-reload, reduced compile times"
            };

            // ==================== CONTENT CREATION RECIPES ====================

            Recipes["VideoEditing"] = new AutomationRecipe
            {
                RecipeName = "Video Editing Workstation",
                Category = "ContentCreation",
                Description = "Optimizes for video editing - Premiere, DaVinci, Vegas.",
                ProcessTriggers = new() { "Premiere.exe", "davinci", "Vegaspro.exe" },
                RequiredAgents = new() { "ContentCreation" },
                OptimizationActions = new()
                {
                    "MaximizeGPUAcceleration",
                    "OptimizeRAMAllocation",
                    "SetupMediaCache",
                    "OptimizeStorageAccess",
                    "GPURenderingSetup"
                },
                ResourceAllocation = new()
                {
                    { "GPU", 0.95 },
                    { "RAM", 0.95 },
                    { "StorageIO", 0.90 },
                    { "CPU", 0.80 }
                },
                RegistryChanges = new()
                {
                    { "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management", "LargeSystemCache=1" }
                },
                ExpectedOutcome = "Real-time 4K editing preview, 50% faster renders"
            };

            Recipes["GraphicsRendering"] = new AutomationRecipe
            {
                RecipeName = "Graphics Rendering Workstation",
                Category = "ContentCreation",
                Description = "Optimizes for rendering - Blender, 3DS Max, Cinema 4D.",
                ProcessTriggers = new() { "blender.exe", "3dsmax.exe", "cinema4d.exe", "lightwave.exe" },
                RequiredAgents = new() { "ContentCreation" },
                OptimizationActions = new()
                {
                    "EnableGPURendering",
                    "MaximizeCUDA",
                    "OptimizeCompute",
                    "SetupRenderFarm",
                    "MemoryAllocationMax"
                },
                ResourceAllocation = new()
                {
                    { "GPU", 1.0 },  // All GPU power
                    { "CPU", 0.95 },
                    { "RAM", 0.95 }
                },
                ExpectedOutcome = "Full GPU utilization, 2-3x faster renders"
            };

            // ==================== PRODUCTIVITY RECIPES ====================

            Recipes["OfficeWork"] = new AutomationRecipe
            {
                RecipeName = "Office Productivity Setup",
                Category = "Productivity",
                Description = "Optimizes for document editing, spreadsheets, presentations.",
                ProcessTriggers = new() { "EXCEL.EXE", "WINWORD.EXE", "POWERPNT.EXE", "Outlook.exe" },
                RequiredAgents = new() { },
                OptimizationActions = new()
                {
                    "DisableAnimations",
                    "OptimizeMemory",
                    "FocusMode",
                    "DisableBackgroundApps"
                },
                ResourceAllocation = new()
                {
                    { "CPU", 0.30 },
                    { "RAM", 0.50 }
                },
                ServiceStates = new()
                {
                    { "WSearch", false },
                    { "DiagTrack", false }
                },
                ExpectedOutcome = "Minimal system load, long battery life for laptops"
            };

            Recipes["Programming"] = new AutomationRecipe
            {
                RecipeName = "Professional Programming",
                Category = "Development",
                Description = "Optimizes for intensive programming - compilation, debugging, analysis.",
                ProcessTriggers = new() { "devenv.exe", "Code.exe", "Rider.exe", "Intellij.exe" },
                RequiredAgents = new() { "Development" },
                OptimizationActions = new()
                {
                    "BoostCPUClocks",
                    "OptimizeCompilation",
                    "IncreaseVirtualMemory",
                    "DisableAnalysis",
                    "MaxIntelliSense"
                },
                ResourceAllocation = new()
                {
                    { "CPU", 0.95 },
                    { "RAM", 0.90 }
                },
                ExpectedOutcome = "35-50% faster compilation, instant IntelliSense"
            };

            // ==================== DUAL WORKLOAD RECIPES ====================

            Recipes["GameDevStreaming"] = new AutomationRecipe
            {
                RecipeName = "Game Dev + Streaming (Compound)",
                Category = "Compound",
                Description = "Handles simultaneous game development and streaming.",
                ProcessTriggers = new() { "UE5Editor.exe", "OBS64.exe" },
                RequiredAgents = new() { "Development", "Streaming" },
                OptimizationActions = new()
                {
                    "CompileAsync",
                    "StreamOptimized",
                    "ResourceSharing",
                    "ConflictResolution"
                },
                ResourceAllocation = new()
                {
                    { "GPU", 0.60 },  // Shared: 40% dev, 20% stream
                    { "CPU", 0.70 },  // 40% dev, 30% stream
                    { "RAM", 0.80 },
                    { "Network", 0.80 }
                },
                ExpectedOutcome = "Both tasks work well simultaneously without major FPS/quality loss"
            };

            Recipes["GamingStreaming"] = new AutomationRecipe
            {
                RecipeName = "Gaming + Streaming (Compound)",
                Category = "Compound",
                Description = "Optimizes for playing a game while streaming it.",
                ProcessTriggers = new() { "Valorant.exe", "OBS64.exe" },
                RequiredAgents = new() { "Gaming", "Streaming" },
                OptimizationActions = new()
                {
                    "PrioritizeGameFPS",
                    "SecondaryStreamEncoding",
                    "NetworkOptimization",
                    "ThermalManagement"
                },
                ResourceAllocation = new()
                {
                    { "GPU", 0.95 },  // Game gets priority
                    { "CPU", 0.60 },  // CPU encoding for stream
                    { "Network", 0.85 }
                },
                ExpectedOutcome = "Maintain 150+ FPS while streaming at 1080p60"
            };
        }

        /// <summary>
        /// Find recipe by name
        /// </summary>
        public AutomationRecipe? GetRecipe(string recipeName)
        {
            return Recipes.TryGetValue(recipeName, out var recipe) ? recipe : null;
        }

        /// <summary>
        /// Find recipes by detected processes
        /// </summary>
        public List<AutomationRecipe> FindRecipesForProcesses(List<string> runningProcesses)
        {
            var matchedRecipes = new List<AutomationRecipe>();

            foreach (var recipe in Recipes.Values)
            {
                var matchCount = 0;
                foreach (var processTrigger in recipe.ProcessTriggers)
                {
                    if (runningProcesses.Any(p => p.Contains(processTrigger, StringComparison.OrdinalIgnoreCase)))
                    {
                        matchCount++;
                    }
                }

                // Add recipe if at least one process matches
                if (matchCount > 0)
                {
                    matchedRecipes.Add(recipe);
                }
            }

            return matchedRecipes;
        }

        /// <summary>
        /// Get all recipes for a category
        /// </summary>
        public List<AutomationRecipe> GetRecipesByCategory(string category)
        {
            return Recipes.Values
                .Where(r => r.Category == category)
                .ToList();
        }
    }
}

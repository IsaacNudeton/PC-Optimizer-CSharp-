using System;
using System.Collections.Generic;

namespace PCOptimizer.Services
{
    /// <summary>
    /// Calculates optimization weights based on PC specs and game requirements
    /// Uses rule-based heuristics - NO machine learning, deterministic outputs only
    /// </summary>
    public class OptimizationWeights
    {
        public double CPULatency { get; set; }           // 0.0 (don't optimize) to 1.0 (aggressive)
        public double InputLag { get; set; }              // Mouse/keyboard polling optimization
        public double GPUBoost { get; set; }              // GPU clock speeds, shader cache
        public double RAMAllocation { get; set; }         // Memory optimization intensity
        public double ThermalManagement { get; set; }     // Thermal throttle prevention
        public double DiskIO { get; set; }                // Storage optimization
        public double PowerManagement { get; set; }       // Power state control
        public double NetworkOptimization { get; set; }   // Network QoS

        public override string ToString()
        {
            return $"CPULatency:{CPULatency:F2} | InputLag:{InputLag:F2} | GPU:{GPUBoost:F2} | RAM:{RAMAllocation:F2} | Thermal:{ThermalManagement:F2} | Disk:{DiskIO:F2} | Power:{PowerManagement:F2} | Network:{NetworkOptimization:F2}";
        }
    }

    /// <summary>
    /// Game profile requirements (base weights before PC adjustment)
    /// </summary>
    public class GameOptimizationProfile
    {
        public string GameName { get; set; } = string.Empty;
        public double CPULatencyBase { get; set; }
        public double InputLagBase { get; set; }
        public double GPUBoostBase { get; set; }
        public double RAMAllocationBase { get; set; }
        public double NetworkOptimizationBase { get; set; }
        public string Category { get; set; } = string.Empty;  // "CompetitiveShooter", "OpenWorld", "Rendering", etc.
    }

    public class OptimizationWeightCalculator
    {
        private readonly Dictionary<string, GameOptimizationProfile> _gameProfiles;

        public OptimizationWeightCalculator()
        {
            _gameProfiles = InitializeGameProfiles();
        }

        /// <summary>
        /// Initialize game-specific optimization profiles
        /// </summary>
        private Dictionary<string, GameOptimizationProfile> InitializeGameProfiles()
        {
            return new Dictionary<string, GameOptimizationProfile>
            {
                // ==================== COMPETITIVE SHOOTERS ====================
                ["Valorant"] = new GameOptimizationProfile
                {
                    GameName = "Valorant",
                    Category = "CompetitiveShooter",
                    CPULatencyBase = 0.95,        // Critical: instant response
                    InputLagBase = 0.98,          // Critical: mouse/keyboard latency
                    GPUBoostBase = 0.55,          // Low: CPU bottleneck, 240+ FPS
                    RAMAllocationBase = 0.15,     // Light: ~3-4GB
                    NetworkOptimizationBase = 0.90 // Important: ping matters
                },

                ["CS2"] = new GameOptimizationProfile
                {
                    GameName = "CS:GO 2",
                    Category = "CompetitiveShooter",
                    CPULatencyBase = 0.95,
                    InputLagBase = 0.97,
                    GPUBoostBase = 0.50,
                    RAMAllocationBase = 0.15,
                    NetworkOptimizationBase = 0.92
                },

                ["CSGO"] = new GameOptimizationProfile
                {
                    GameName = "CS:GO",
                    Category = "CompetitiveShooter",
                    CPULatencyBase = 0.93,
                    InputLagBase = 0.96,
                    GPUBoostBase = 0.45,
                    RAMAllocationBase = 0.12,
                    NetworkOptimizationBase = 0.90
                },

                ["OverWatch2"] = new GameOptimizationProfile
                {
                    GameName = "Overwatch 2",
                    Category = "CompetitiveShooter",
                    CPULatencyBase = 0.85,
                    InputLagBase = 0.90,
                    GPUBoostBase = 0.70,
                    RAMAllocationBase = 0.25,
                    NetworkOptimizationBase = 0.85
                },

                ["Apex"] = new GameOptimizationProfile
                {
                    GameName = "Apex Legends",
                    Category = "CompetitiveShooter",
                    CPULatencyBase = 0.80,
                    InputLagBase = 0.85,
                    GPUBoostBase = 0.75,
                    RAMAllocationBase = 0.30,
                    NetworkOptimizationBase = 0.80
                },

                ["Fortnite"] = new GameOptimizationProfile
                {
                    GameName = "Fortnite",
                    Category = "CompetitiveShooter",
                    CPULatencyBase = 0.75,
                    InputLagBase = 0.80,
                    GPUBoostBase = 0.80,
                    RAMAllocationBase = 0.35,
                    NetworkOptimizationBase = 0.78
                },

                // ==================== OPEN WORLD / AAA ====================
                ["GTAV"] = new GameOptimizationProfile
                {
                    GameName = "GTA V",
                    Category = "OpenWorld",
                    CPULatencyBase = 0.60,        // Moderate: GPU is bottleneck
                    InputLagBase = 0.50,          // Low: not latency-critical
                    GPUBoostBase = 0.88,          // High: needs GPU power
                    RAMAllocationBase = 0.60,     // Moderate-High: textures
                    NetworkOptimizationBase = 0.40
                },

                ["Warzone2"] = new GameOptimizationProfile
                {
                    GameName = "Warzone 2",
                    Category = "OpenWorld",
                    CPULatencyBase = 0.72,
                    InputLagBase = 0.70,
                    GPUBoostBase = 0.82,
                    RAMAllocationBase = 0.55,
                    NetworkOptimizationBase = 0.85
                },

                // ==================== MMOs ====================
                ["GW2"] = new GameOptimizationProfile
                {
                    GameName = "Guild Wars 2",
                    Category = "MMO",
                    CPULatencyBase = 0.50,
                    InputLagBase = 0.45,
                    GPUBoostBase = 0.70,
                    RAMAllocationBase = 0.50,
                    NetworkOptimizationBase = 0.75
                },

                ["FFXIV"] = new GameOptimizationProfile
                {
                    GameName = "Final Fantasy XIV",
                    Category = "MMO",
                    CPULatencyBase = 0.50,
                    InputLagBase = 0.45,
                    GPUBoostBase = 0.65,
                    RAMAllocationBase = 0.40,
                    NetworkOptimizationBase = 0.70
                },

                ["WoW"] = new GameOptimizationProfile
                {
                    GameName = "World of Warcraft",
                    Category = "MMO",
                    CPULatencyBase = 0.55,
                    InputLagBase = 0.50,
                    GPUBoostBase = 0.68,
                    RAMAllocationBase = 0.45,
                    NetworkOptimizationBase = 0.72
                },

                // ==================== CONTENT CREATION ====================
                ["Blender"] = new GameOptimizationProfile
                {
                    GameName = "Blender",
                    Category = "Rendering",
                    CPULatencyBase = 0.10,        // Irrelevant
                    InputLagBase = 0.05,          // Irrelevant
                    GPUBoostBase = 0.95,          // Maximum
                    RAMAllocationBase = 0.95,     // Maximum
                    NetworkOptimizationBase = 0.00
                },

                ["Premiere"] = new GameOptimizationProfile
                {
                    GameName = "Adobe Premiere",
                    Category = "VideoEditing",
                    CPULatencyBase = 0.20,
                    InputLagBase = 0.10,
                    GPUBoostBase = 0.90,
                    RAMAllocationBase = 0.90,
                    NetworkOptimizationBase = 0.00
                },

                ["DaVinci"] = new GameOptimizationProfile
                {
                    GameName = "DaVinci Resolve",
                    Category = "VideoEditing",
                    CPULatencyBase = 0.20,
                    InputLagBase = 0.10,
                    GPUBoostBase = 0.92,
                    RAMAllocationBase = 0.92,
                    NetworkOptimizationBase = 0.00
                },

                // ==================== DEVELOPMENT ====================
                ["VisualStudio"] = new GameOptimizationProfile
                {
                    GameName = "Visual Studio",
                    Category = "Development",
                    CPULatencyBase = 0.70,
                    InputLagBase = 0.60,
                    GPUBoostBase = 0.30,
                    RAMAllocationBase = 0.80,
                    NetworkOptimizationBase = 0.40
                }
            };
        }

        /// <summary>
        /// Calculate optimization weights for a game based on PC specs
        /// </summary>
        public OptimizationWeights CalculateWeights(string gameName, PCSpecs pc)
        {
            // Get game profile
            if (!_gameProfiles.TryGetValue(gameName, out var gameProfile))
            {
                System.Console.WriteLine($"[OptimizationWeightCalculator] Game '{gameName}' not found, using balanced defaults");
                gameProfile = GetBalancedProfile();
            }

            // Start with game profile base values
            var weights = new OptimizationWeights();

            // ============ PC-BASED ADJUSTMENTS ============

            // CPU Latency (architectural, not thermal)
            weights.CPULatency = gameProfile.CPULatencyBase;

            // Input Lag stays from profile
            weights.InputLag = gameProfile.InputLagBase;

            // GPU Boost Adjustment (thermals)
            double gpuThermalAdjustment = 1.0;
            if (pc.CurrentGPUTemp > 85)
                gpuThermalAdjustment = 0.65;   // Hot: reduce GPU boost
            else if (pc.CurrentGPUTemp > 75)
                gpuThermalAdjustment = 0.80;
            else if (pc.CurrentGPUTemp < 60)
                gpuThermalAdjustment = 1.1;    // Cold: safe to boost more

            weights.GPUBoost = Clamp(gameProfile.GPUBoostBase * gpuThermalAdjustment, 0.0, 1.0);

            // RAM Adjustment
            double ramAdjustment = 1.0;
            if (pc.TotalRAM < 16)
                ramAdjustment = 0.85;  // Weak system: be more aggressive on RAM
            else if (pc.TotalRAM < 32)
                ramAdjustment = 0.95;
            else if (pc.TotalRAM > 64)
                ramAdjustment = 1.05;  // Plenty of RAM: less aggressive needed

            weights.RAMAllocation = Clamp(gameProfile.RAMAllocationBase * ramAdjustment, 0.0, 1.0);

            // Thermal Management (higher if hot)
            weights.ThermalManagement = pc.CurrentGPUTemp > 75 ? 0.85 : 0.45;

            // Disk I/O (if HDD, be more aggressive)
            weights.DiskIO = pc.StorageType.ToLower() == "hdd" ? 0.85 : 0.30;

            // Power Management (if on battery, reduce optimization intensity)
            weights.PowerManagement = pc.OnBattery ? 0.70 : 0.40;

            // Network stays from profile
            weights.NetworkOptimization = gameProfile.NetworkOptimizationBase;

            return weights;
        }

        /// <summary>
        /// Get balanced profile for unknown games
        /// </summary>
        private GameOptimizationProfile GetBalancedProfile()
        {
            return new GameOptimizationProfile
            {
                GameName = "Balanced",
                Category = "Generic",
                CPULatencyBase = 0.60,
                InputLagBase = 0.55,
                GPUBoostBase = 0.70,
                RAMAllocationBase = 0.50,
                NetworkOptimizationBase = 0.50
            };
        }

        /// <summary>
        /// Clamp value between min and max
        /// </summary>
        private double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }

    /// <summary>
    /// PC specs snapshot for weight calculation
    /// </summary>
    public class PCSpecs
    {
        public string CPUModel { get; set; } = string.Empty;
        public int CPUCores { get; set; }
        public double TotalRAM { get; set; }         // GB
        public string GPUModel { get; set; } = string.Empty;
        public double GPUVRAM { get; set; }          // GB
        public double CurrentGPUTemp { get; set; }   // Celsius
        public double CurrentCPUTemp { get; set; }   // Celsius
        public string StorageType { get; set; } = string.Empty;      // "SSD" or "HDD"
        public bool OnBattery { get; set; }
    }
}

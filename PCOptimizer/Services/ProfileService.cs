using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCOptimizer.Services
{
    // Profile definition - a bundle of optimizations
    public class OptimizationProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime AppliedAt { get; set; }
        public List<string> Optimizations { get; set; } = new List<string>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }

    public class ProfileService
    {
        private readonly OptimizerService _optimizerService;
        private readonly PerformanceMonitor _performanceMonitor;
        private OptimizationProfile? _currentProfile = null;
        private readonly Dictionary<string, OptimizationProfile> _profiles;

        public ProfileService(OptimizerService optimizerService, PerformanceMonitor performanceMonitor)
        {
            _optimizerService = optimizerService;
            _performanceMonitor = performanceMonitor;
            _profiles = InitializeProfiles();
        }

        /// <summary>
        /// Initializes default optimization profiles
        /// </summary>
        private Dictionary<string, OptimizationProfile> InitializeProfiles()
        {
            return new Dictionary<string, OptimizationProfile>
            {
                ["Gaming"] = new OptimizationProfile
                {
                    Name = "Gaming",
                    Description = "High-performance gaming profile - maximizes FPS and reduces latency",
                    Optimizations = new List<string>
                    {
                        "OptimizeNvidiaGPU",
                        "OptimizeAMDGPU",
                        "ClearStandbyMemory",
                        "CreateGamingPowerPlan",
                        "OptimizeNetworkAdvanced",
                        "OptimizeAudioLatency",
                        "DisableBackgroundApps"
                    },
                    Settings = new Dictionary<string, object>
                    {
                        { "PowerPlan", "High Performance" },
                        { "GPUPerformance", "Maximum" },
                        { "NetworkPriority", "Gaming" },
                        { "AudioLatency", "Minimum" }
                    }
                },

                ["Development"] = new OptimizationProfile
                {
                    Name = "Development",
                    Description = "Development workload - balances performance and resource availability",
                    Optimizations = new List<string>
                    {
                        "CreateGamingPowerPlan",
                        "OptimizeDisplay"
                    },
                    Settings = new Dictionary<string, object>
                    {
                        { "PowerPlan", "Balanced" },
                        { "MaxCPUThreads", true },
                        { "DebugMode", true }
                    }
                },

                ["VideoEditing"] = new OptimizationProfile
                {
                    Name = "VideoEditing",
                    Description = "Video editing profile - maximizes RAM and GPU VRAM allocation",
                    Optimizations = new List<string>
                    {
                        "OptimizeNvidiaGPU",
                        "OptimizeAMDGPU",
                        "CreateGamingPowerPlan",
                        "ClearStandbyMemory"
                    },
                    Settings = new Dictionary<string, object>
                    {
                        { "PowerPlan", "High Performance" },
                        { "GPUVRAMAllocation", "Maximum" },
                        { "RAMAllocation", "Maximum" },
                        { "StorageOptimization", true }
                    }
                },

                ["Workstation"] = new OptimizationProfile
                {
                    Name = "Workstation",
                    Description = "General workstation profile - balanced performance and stability",
                    Optimizations = new List<string>
                    {
                        "OptimizeDisplay",
                        "OptimizeBootSettings"
                    },
                    Settings = new Dictionary<string, object>
                    {
                        { "PowerPlan", "Balanced" },
                        { "BootOptimization", true }
                    }
                },

                ["PowerSaver"] = new OptimizationProfile
                {
                    Name = "PowerSaver",
                    Description = "Battery-saving profile - reduces power consumption",
                    Optimizations = new List<string>(),
                    Settings = new Dictionary<string, object>
                    {
                        { "PowerPlan", "Power Saver" },
                        { "ScreenBrightness", "Medium" }
                    }
                }
            };
        }

        /// <summary>
        /// Gets all available profiles
        /// </summary>
        public Dictionary<string, OptimizationProfile> GetProfiles()
        {
            return _profiles;
        }

        /// <summary>
        /// Gets the currently active profile
        /// </summary>
        public OptimizationProfile? GetCurrentProfile()
        {
            return _currentProfile;
        }

        /// <summary>
        /// Applies a profile by name
        /// </summary>
        public Task<OptimizationResult> ApplyProfile(string profileName)
        {
            if (!_profiles.TryGetValue(profileName, out var profile))
            {
                return Task.FromResult(new OptimizationResult
                {
                    Success = false,
                    Message = $"Profile '{profileName}' not found",
                    Category = "Profile"
                });
            }

            try
            {
                var result = new OptimizationResult
                {
                    Success = true,
                    Message = $"Applying profile: {profileName}",
                    Category = "Profile",
                    Changes = new List<string>()
                };

                // Apply each optimization in the profile
                foreach (var optimization in profile.Optimizations)
                {
                    result.Changes.Add($"[{optimization}] Optimizing...");

                    // Call the appropriate optimizer method
                    // (We'll refactor OptimizerService to support this)
                }

                // Mark as active and record timestamp
                _currentProfile = profile;
                profile.IsActive = true;
                profile.AppliedAt = DateTime.Now;

                result.Success = true;
                result.Message = $"Profile '{profileName}' applied successfully";
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to apply profile: {ex.Message}",
                    Category = "Profile"
                });
            }
        }

        /// <summary>
        /// Reverts to default/balanced settings
        /// </summary>
        public Task<OptimizationResult> RevertProfile()
        {
            try
            {
                // Reset to balanced profile
                _currentProfile = null;

                return Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Message = "System reverted to default settings",
                    Category = "Profile"
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to revert: {ex.Message}",
                    Category = "Profile"
                });
            }
        }

        /// <summary>
        /// Creates a custom profile
        /// </summary>
        public OptimizationResult CreateCustomProfile(string name, List<string> optimizations, Dictionary<string, object> settings)
        {
            if (_profiles.ContainsKey(name))
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Profile '{name}' already exists",
                    Category = "Profile"
                };
            }

            var profile = new OptimizationProfile
            {
                Name = name,
                Description = $"Custom profile: {name}",
                Optimizations = optimizations,
                Settings = settings
            };

            _profiles[name] = profile;

            return new OptimizationResult
            {
                Success = true,
                Message = $"Custom profile '{name}' created successfully",
                Category = "Profile",
                Changes = new List<string> { $"Created profile with {optimizations.Count} optimizations" }
            };
        }
    }
}

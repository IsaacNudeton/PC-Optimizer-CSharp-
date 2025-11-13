using System;
using System.Collections.Generic;

namespace PCOptimizer.Services.AI.Core
{
    /// <summary>
    /// Represents a snapshot of the current system state for AI agent decision making
    /// </summary>
    public class SystemSnapshot
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Performance Metrics
        public double CpuUsage { get; set; }
        public double GpuUsage { get; set; }
        public double RamUsage { get; set; }
        public double RamAvailable { get; set; }
        public double DiskUsage { get; set; }
        public double NetworkUsage { get; set; }
        
        // Thermal
        public double CpuTemp { get; set; }
        public double GpuTemp { get; set; }
        
        // Activity Context
        public string CurrentActivity { get; set; } = "Unknown";
        public string ActiveProcess { get; set; } = "";
        public string ActiveWindow { get; set; } = "";
        public List<string> RunningProcesses { get; set; } = new();
        
        // User Behavior
        public int KeyboardActivityLevel { get; set; } // 0-100
        public int MouseActivityLevel { get; set; } // 0-100
        public bool IsUserActive { get; set; }
        
        // System State
        public string CurrentProfile { get; set; } = "Balanced";
        public List<string> ActiveOptimizations { get; set; } = new();
        public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
        
        // Gaming Specific
        public int? CurrentFps { get; set; }
        public double? FrameTime { get; set; }
        public double? InputLatency { get; set; }
        
        public SystemSnapshot()
        {
        }
        
        // Additional hardware info (optional)
        public string? CPUModel { get; set; }
        public string? GPUModel { get; set; }
        public int? CPUCores { get; set; }
        public double? GPUVRAM { get; set; }
        public double? TotalRAM { get; set; }
        public string? StorageType { get; set; }
        
        // Legacy property names for backwards compatibility
        public double CurrentCPUUsage => CpuUsage;
        public double CurrentGPUUsage => GpuUsage;
        public double CurrentRAMUsage => RamUsage;
        public double CurrentCPUTemp => CpuTemp;
        public double CurrentGPUTemp => GpuTemp;
        
        /// <summary>
        /// Create snapshot from current system state
        /// </summary>
        public static SystemSnapshot FromMonitors(PerformanceMonitor perfMonitor, BehaviorMonitor behaviorMonitor)
        {
            var snapshot = new SystemSnapshot();
            
            // Get performance metrics
            if (perfMonitor != null)
            {
                var metrics = perfMonitor.GetMetrics();
                if (metrics != null)
                {
                    snapshot.CpuUsage = metrics.CpuUsage;
                    snapshot.RamUsage = metrics.RamPercent;
                    snapshot.RamAvailable = metrics.RamTotalGB - metrics.RamUsedGB;
                    snapshot.GpuUsage = metrics.GpuUsage;
                    snapshot.CpuTemp = metrics.CpuTemp ?? 0;
                    snapshot.GpuTemp = metrics.GpuTemp ?? 0;
                }
            }
            
            // Get behavior context
            if (behaviorMonitor != null)
            {
                var behaviorSnapshot = behaviorMonitor.CaptureSnapshot();
                snapshot.CurrentActivity = behaviorSnapshot.Category;
                snapshot.ActiveProcess = behaviorSnapshot.ActiveWindow?.ProcessName ?? "None";
                snapshot.RunningProcesses = behaviorSnapshot.RunningProcesses.Select(p => p.ProcessName).ToList();
                // User is active if there's an active window and processes are running
                snapshot.IsUserActive = behaviorSnapshot.ActiveWindow != null && behaviorSnapshot.RunningProcesses.Any();
            }
            
            return snapshot;
        }
    }
}

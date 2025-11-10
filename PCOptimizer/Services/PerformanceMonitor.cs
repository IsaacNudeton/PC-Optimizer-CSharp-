using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Collections.Generic;
using LibreHardwareMonitor.Hardware;

namespace PCOptimizer.Services
{
    public class PerformanceMetrics
    {
        public float CpuUsage { get; set; }
        public float GpuUsage { get; set; }
        public double RamUsedGB { get; set; }
        public double RamTotalGB { get; set; }
        public double RamPercent { get; set; }
        public int? CpuTemp { get; set; }
        public int? GpuTemp { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public enum MonitoringMode
    {
        Active,      // Foreground: Poll every 2 seconds, full hardware monitoring
        Background,  // Background: Poll every 10 seconds, essential only
        Paused       // Paused: No polling at all (zero CPU usage)
    }

    public class PerformanceMonitor : IDisposable
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramAvailCounter;
        private double _totalRamGB;
        private Computer? _computer;
        private bool _hardwareMonitoringEnabled = true;

        // Adaptive monitoring
        private MonitoringMode _currentMode = MonitoringMode.Active;
        private PerformanceMetrics? _cachedMetrics;
        private DateTime _lastUpdate = DateTime.MinValue;

        // Circular buffer for metrics history (memory efficient - limits to 100 entries)
        private Queue<PerformanceMetrics> _metricsHistory = new Queue<PerformanceMetrics>();
        private const int MAX_HISTORY_SIZE = 100;

        // ML-powered anomaly detection
        private AnomalyDetectionService? _anomalyDetector;
        private bool _anomalyDetectionEnabled = false;
        private List<AnomalyResult> _recentAnomalies = new List<AnomalyResult>();
        private const int MAX_ANOMALY_HISTORY = 50;

        public MonitoringMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;

                // Cleanup hardware monitor when switching to paused mode
                if (value == MonitoringMode.Paused && _computer != null)
                {
                    _computer.Close();
                    _computer = null;
                    _hardwareMonitoringEnabled = false;
                }
                // Re-initialize when returning to active monitoring
                else if (value != MonitoringMode.Paused && _computer == null)
                {
                    InitializeHardwareMonitor();
                }
            }
        }

        public PerformanceMonitor()
        {
            // Initialize CPU counter
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cpuCounter.NextValue(); // First call returns 0

            // Initialize RAM counter
            _ramAvailCounter = new PerformanceCounter("Memory", "Available MBytes");

            // Get total RAM using WMI (cached - doesn't change)
            _totalRamGB = GetTotalRAM();

            // Initialize hardware monitor
            InitializeHardwareMonitor();
        }

        private void InitializeHardwareMonitor()
        {
            try
            {
                // Initialize LibreHardwareMonitor for GPU and temps
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = false,
                    IsMotherboardEnabled = false,
                    IsControllerEnabled = false,
                    IsNetworkEnabled = false,
                    IsStorageEnabled = false
                };
                _computer.Open();
                _computer.Accept(new UpdateVisitor());
                _hardwareMonitoringEnabled = true;
            }
            catch
            {
                _hardwareMonitoringEnabled = false;
            }
        }

        private double GetTotalRAM()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    ulong totalKB = (ulong)obj["TotalVisibleMemorySize"];
                    return Math.Round(totalKB / 1024.0 / 1024.0, 2); // Convert KB to GB
                }
            }
            catch { }
            return 16.0; // Fallback
        }

        public PerformanceMetrics GetMetrics()
        {
            // ADAPTIVE MONITORING: Return cached metrics if called too frequently
            var timeSinceLastUpdate = DateTime.Now - _lastUpdate;
            var minUpdateInterval = _currentMode switch
            {
                MonitoringMode.Active => TimeSpan.FromSeconds(1),      // Active: Allow updates every 1 second
                MonitoringMode.Background => TimeSpan.FromSeconds(5),  // Background: Min 5 seconds between updates
                MonitoringMode.Paused => TimeSpan.FromHours(1),        // Paused: Return cached forever (no updates)
                _ => TimeSpan.FromSeconds(2)
            };

            if (_cachedMetrics != null && timeSinceLastUpdate < minUpdateInterval)
            {
                // Return cached metrics - ZERO CPU/GPU overhead
                return _cachedMetrics;
            }

            // Paused mode: Don't do ANY work
            if (_currentMode == MonitoringMode.Paused)
            {
                return _cachedMetrics ?? new PerformanceMetrics();
            }

            // Create new metrics object
            var metrics = new PerformanceMetrics();

            // ESSENTIAL METRICS: Always get CPU and RAM (lightweight)
            metrics.CpuUsage = (float)Math.Round(_cpuCounter.NextValue(), 1);

            double availMB = _ramAvailCounter.NextValue();
            double availGB = availMB / 1024.0;
            double usedGB = _totalRamGB - availGB;

            metrics.RamTotalGB = _totalRamGB;
            metrics.RamUsedGB = Math.Round(usedGB, 2);
            metrics.RamPercent = Math.Round((usedGB / _totalRamGB) * 100, 1);

            // HARDWARE MONITORING: Only in Active mode (expensive operations)
            if (_currentMode == MonitoringMode.Active && _hardwareMonitoringEnabled && _computer != null)
            {
                try
                {
                    // Update hardware sensors
                    _computer.Accept(new UpdateVisitor());

                    // GPU Usage and Temperature
                    foreach (var hardware in _computer.Hardware)
                    {
                        if (hardware.HardwareType == HardwareType.GpuNvidia ||
                            hardware.HardwareType == HardwareType.GpuAmd ||
                            hardware.HardwareType == HardwareType.GpuIntel)
                        {
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Core"))
                                {
                                    metrics.GpuUsage = sensor.Value ?? 0;
                                }
                                if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("GPU Core"))
                                {
                                    metrics.GpuTemp = (int?)(sensor.Value ?? 0);
                                }
                            }
                        }

                        // CPU Temperature
                        if (hardware.HardwareType == HardwareType.Cpu)
                        {
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Temperature &&
                                    (sensor.Name.Contains("Package") || sensor.Name.Contains("Core Average")))
                                {
                                    metrics.CpuTemp = (int?)(sensor.Value ?? 0);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback: Use cached GPU/temp values if hardware monitoring fails
                    if (_cachedMetrics != null)
                    {
                        metrics.GpuUsage = _cachedMetrics.GpuUsage;
                        metrics.GpuTemp = _cachedMetrics.GpuTemp;
                        metrics.CpuTemp = _cachedMetrics.CpuTemp;
                    }
                }
            }
            else if (_cachedMetrics != null)
            {
                // Background mode: Reuse previous GPU/temp values (avoid expensive hardware queries)
                metrics.GpuUsage = _cachedMetrics.GpuUsage;
                metrics.GpuTemp = _cachedMetrics.GpuTemp;
                metrics.CpuTemp = _cachedMetrics.CpuTemp;
            }

            // Update timestamp
            metrics.Timestamp = DateTime.Now;

            // Cache metrics for next call
            _cachedMetrics = metrics;
            _lastUpdate = DateTime.Now;

            // Add to circular buffer (memory efficient - auto-removes old entries)
            AddToHistory(metrics);

            // ML ANOMALY DETECTION: Automatically detect anomalies if enabled
            if (_anomalyDetectionEnabled && _anomalyDetector != null)
            {
                try
                {
                    var detectedAnomalies = _anomalyDetector.DetectAnomalies(metrics);

                    if (detectedAnomalies.Count > 0)
                    {
                        _recentAnomalies.AddRange(detectedAnomalies);

                        // Maintain max anomaly history
                        while (_recentAnomalies.Count > MAX_ANOMALY_HISTORY)
                        {
                            _recentAnomalies.RemoveAt(0);
                        }
                    }
                }
                catch
                {
                    // Anomaly detection failure shouldn't crash monitoring
                }
            }

            return metrics;
        }

        private void AddToHistory(PerformanceMetrics metrics)
        {
            _metricsHistory.Enqueue(metrics);

            // Maintain max size - remove oldest entries
            while (_metricsHistory.Count > MAX_HISTORY_SIZE)
            {
                _metricsHistory.Dequeue();
            }
        }

        public IEnumerable<PerformanceMetrics> GetHistory()
        {
            return _metricsHistory.ToList();
        }

        public PerformanceMetrics? GetLastMetrics()
        {
            return _cachedMetrics;
        }

        // ANOMALY DETECTION METHODS

        public void EnableAnomalyDetection()
        {
            if (_anomalyDetector == null)
            {
                _anomalyDetector = new AnomalyDetectionService();
            }
            _anomalyDetectionEnabled = true;
        }

        public void DisableAnomalyDetection()
        {
            _anomalyDetectionEnabled = false;
        }

        public bool IsAnomalyDetectionEnabled()
        {
            return _anomalyDetectionEnabled;
        }

        public bool IsAnomalyDetectionReady()
        {
            return _anomalyDetector != null && _anomalyDetector.IsReady();
        }

        public List<AnomalyResult> GetRecentAnomalies()
        {
            return _recentAnomalies.ToList();
        }

        public List<AnomalyResult> GetNewAnomalies()
        {
            var newAnomalies = _recentAnomalies.ToList();
            _recentAnomalies.Clear();
            return newAnomalies;
        }

        public int GetAnomalyHistorySize()
        {
            return _anomalyDetector?.GetHistoryCount("cpu") ?? 0;
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _ramAvailCounter?.Dispose();
            _computer?.Close();
        }
    }

    // Helper class for updating hardware sensors
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (var subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}

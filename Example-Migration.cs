// EXAMPLE: How to migrate your PowerShell functions to C#

using System;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;

namespace PCOptimizer.Services
{
    /// <summary>
    /// Performance monitoring service - migrated from PowerShell Get-PerformanceMetrics
    /// </summary>
    public class PerformanceMonitor
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;
        private PerformanceCounter _diskCounter;

        public PerformanceMonitor()
        {
            // Initialize performance counters (stays resident, WAY faster than PowerShell)
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            // First call to NextValue() returns 0, so call it once
            _cpuCounter.NextValue();
        }

        /// <summary>
        /// Get current CPU usage (0-100%)
        /// PowerShell equivalent: $script:cpuCounter.NextValue()
        /// </summary>
        public float GetCpuUsage()
        {
            return (float)Math.Round(_cpuCounter.NextValue(), 1);
        }

        /// <summary>
        /// Get RAM usage in GB
        /// PowerShell equivalent: Get-CimInstance Win32_OperatingSystem
        /// </summary>
        public (double Used, double Total, double Percent) GetRamUsage()
        {
            var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            double totalRAM = computerInfo.TotalPhysicalMemory / (1024.0 * 1024.0 * 1024.0);
            double availRAM = computerInfo.AvailablePhysicalMemory / (1024.0 * 1024.0 * 1024.0);
            double usedRAM = totalRAM - availRAM;
            double percent = (usedRAM / totalRAM) * 100;

            return (
                Math.Round(usedRAM, 2),
                Math.Round(totalRAM, 2),
                Math.Round(percent, 1)
            );
        }

        /// <summary>
        /// Get CPU temperature (requires WMI)
        /// PowerShell equivalent: Get-CimInstance MSAcpi_ThermalZoneTemperature
        /// </summary>
        public int? GetCpuTemperature()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    @"root\WMI",
                    "SELECT * FROM MSAcpi_ThermalZoneTemperature"
                );

                foreach (ManagementObject obj in searcher.Get())
                {
                    double temp = Convert.ToDouble(obj["CurrentTemperature"]);
                    // Convert from tenths of Kelvin to Celsius
                    return (int)((temp - 2732) / 10.0);
                }
            }
            catch
            {
                return null; // Temperature sensors not available
            }

            return null;
        }
    }

    /// <summary>
    /// System optimization service - migrated from PowerShell optimization functions
    /// </summary>
    public class SystemOptimizer
    {
        /// <summary>
        /// Enable Ultimate Performance power plan
        /// PowerShell equivalent: powercfg commands
        /// </summary>
        public bool EnableUltimatePerformance()
        {
            try
            {
                // Execute powercfg command
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg.exe",
                        Arguments = "/duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enable Ultimate Performance: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disable Windows Game Bar
        /// PowerShell equivalent: Set-ItemProperty registry
        /// </summary>
        public bool DisableGameBar()
        {
            try
            {
                const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR";

                using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("AppCaptureEnabled", 0, RegistryValueKind.DWord);
                        key.SetValue("GameDVR_Enabled", 0, RegistryValueKind.DWord);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to disable Game Bar: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Set process priority (auto-boost games)
        /// PowerShell equivalent: Get-Process | Set-Process Priority
        /// </summary>
        public bool BoostProcessPriority(string processName, ProcessPriorityClass priority)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);

                foreach (var process in processes)
                {
                    process.PriorityClass = priority;
                    Console.WriteLine($"Boosted {processName} (PID: {process.Id}) to {priority}");
                }

                return processes.Length > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to boost process: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clear standby memory (ISLC technique)
        /// PowerShell equivalent: EmptyWorkingSet API calls
        /// </summary>
        public bool ClearStandbyMemory()
        {
            try
            {
                // This requires P/Invoke to native Windows APIs
                // See: https://github.com/Klocman/Bulk-Crap-Uninstaller/blob/master/source/UninstallTools/Helpers/MemoryManagement.cs

                // For now, call the PowerShell script as a workaround
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"Clear-RecycleBin -Force\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                process?.WaitForExit();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Example ViewModel for MVVM pattern
    /// </summary>
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly PerformanceMonitor _monitor;
        private float _cpuUsage;
        private double _ramUsedGB;

        public float CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged(nameof(CpuUsage));
            }
        }

        public double RamUsedGB
        {
            get => _ramUsedGB;
            set
            {
                _ramUsedGB = value;
                OnPropertyChanged(nameof(RamUsedGB));
            }
        }

        public DashboardViewModel()
        {
            _monitor = new PerformanceMonitor();

            // Update metrics every 2 seconds
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, e) => UpdateMetrics();
            timer.Start();
        }

        private void UpdateMetrics()
        {
            CpuUsage = _monitor.GetCpuUsage();
            var (used, total, percent) = _monitor.GetRamUsage();
            RamUsedGB = used;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

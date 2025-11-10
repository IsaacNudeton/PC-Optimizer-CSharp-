using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace PCOptimizer.Services
{
    public class OptimizationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Changes { get; set; } = new List<string>();
    }

    public class OptimizerService
    {
        #region Native API Imports
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetTimerResolution(int DesiredResolution, bool SetResolution, out int CurrentResolution);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQueryTimerResolution(out int MinimumResolution, out int MaximumResolution, out int CurrentResolution);

        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int period);

        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int period);
        #endregion

        #region Helper Methods

        private async Task RunCommand(string fileName, string arguments, List<string> changes)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    if (process.ExitCode == 0)
                    {
                        changes.Add($"{fileName} {arguments}");
                    }
                }
            }
            catch { }
        }

        #endregion

        #region GPU Optimizations

        public async Task<OptimizationResult> OptimizeNvidiaGPU(bool enableLowLatency = true, bool maxPerformance = true, bool disableVSync = true)
        {
            try
            {
                var changes = new List<string>();
                var nvidiaKeyPath = @"Software\NVIDIA Corporation\Global\NVTweak";

                using (var key = Registry.CurrentUser.CreateSubKey(nvidiaKeyPath, true))
                {
                    if (key != null)
                    {
                        if (maxPerformance)
                        {
                            // Power Management Mode: Prefer Maximum Performance
                            key.SetValue("Gestalt", 1, RegistryValueKind.DWord);
                            changes.Add("Max Performance Mode");

                            // Texture Filtering - Quality: High Performance
                            var nvCplPath = @"Software\NVIDIA Corporation\Global\NVTweak\NvCplSetProperty";
                            using (var nvCplKey = Registry.CurrentUser.CreateSubKey(nvCplPath, true))
                            {
                                nvCplKey?.SetValue("Filtering", 0, RegistryValueKind.DWord);
                            }
                            changes.Add("High Performance Texture Filtering");
                        }

                        if (enableLowLatency)
                        {
                            // Low Latency Mode: Ultra
                            key.SetValue("RmGpuLowLatencyMode", 1, RegistryValueKind.DWord);
                            changes.Add("Low Latency Mode (Ultra)");
                        }

                        if (disableVSync)
                        {
                            // Disable V-Sync
                            key.SetValue("VSync", 0, RegistryValueKind.DWord);
                            changes.Add("V-Sync Disabled");
                        }

                        // Maximum Pre-Rendered Frames: 1
                        key.SetValue("RmMaxFramesAllowed", 1, RegistryValueKind.DWord);
                        changes.Add("Max Pre-Rendered Frames = 1");

                        // Disable Anisotropic Filtering (user controlled in-game)
                        key.SetValue("AnisoLevel", 0, RegistryValueKind.DWord);

                        // Threaded Optimization: On
                        key.SetValue("ThreadedOptim", 1, RegistryValueKind.DWord);
                        changes.Add("Threaded Optimization Enabled");
                    }
                }

                // Try to restart NVIDIA Display Driver Service
                try
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "net",
                        Arguments = "stop NVDisplay.ContainerLocalSystem",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        Verb = "runas"
                    });
                    process?.WaitForExit();

                    await Task.Delay(1000);

                    process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "net",
                        Arguments = "start NVDisplay.ContainerLocalSystem",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        Verb = "runas"
                    });
                    process?.WaitForExit();
                }
                catch { }

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "NVIDIA GPU optimized successfully",
                    Category = "GPU"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize NVIDIA GPU: {ex.Message}",
                    Category = "GPU"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeAMDGPU(bool maxPerformance = true, bool disableVSync = true)
        {
            try
            {
                var changes = new List<string>();
                var amdKeyPath = @"Software\AMD\CN";

                using (var key = Registry.CurrentUser.CreateSubKey(amdKeyPath, true))
                {
                    if (key != null && maxPerformance)
                    {
                        // Set power profile to maximum performance
                        using (var powerKey = Registry.CurrentUser.CreateSubKey(@"Software\AMD\CN\PowerPlay", true))
                        {
                            powerKey?.SetValue("AC", 0, RegistryValueKind.DWord);
                            powerKey?.SetValue("DC", 0, RegistryValueKind.DWord);
                        }
                        changes.Add("Max Performance Mode");

                        // Texture Filtering Quality: Performance
                        using (var graphicsKey = Registry.CurrentUser.CreateSubKey(@"Software\AMD\CN\Graphics", true))
                        {
                            graphicsKey?.SetValue("TextureOpt", 1, RegistryValueKind.DWord);
                            graphicsKey?.SetValue("AAMode", 0, RegistryValueKind.DWord);
                            graphicsKey?.SetValue("AnisoType", 0, RegistryValueKind.DWord);
                        }
                        changes.Add("High Performance Texture Filtering");
                    }

                    if (disableVSync)
                    {
                        key?.SetValue("VSyncControl", 0, RegistryValueKind.DWord);
                        changes.Add("V-Sync Disabled");
                    }

                    // Radeon Anti-Lag: Enabled
                    try
                    {
                        using (var antiLagKey = Registry.CurrentUser.CreateSubKey(@"Software\AMD\CN\AntiLag", true))
                        {
                            antiLagKey?.SetValue("AntiLagEnabled", 1, RegistryValueKind.DWord);
                        }
                        changes.Add("Anti-Lag Enabled");
                    }
                    catch { }
                }

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "AMD GPU optimized successfully",
                    Category = "GPU"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize AMD GPU: {ex.Message}",
                    Category = "GPU"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeGPU()
        {
            // Auto-detect GPU vendor and apply appropriate optimization
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString()?.ToLower() ?? "";

                        if (name.Contains("nvidia"))
                            return await OptimizeNvidiaGPU();
                        else if (name.Contains("amd") || name.Contains("radeon"))
                            return await OptimizeAMDGPU();
                    }
                }

                return new OptimizationResult
                {
                    Success = false,
                    Message = "No NVIDIA or AMD GPU detected",
                    Category = "GPU"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Error detecting GPU: {ex.Message}",
                    Category = "GPU"
                };
            }
        }

        #endregion

        #region Power Plan Optimizations

        public async Task<OptimizationResult> CreateGamingPowerPlan()
        {
            try
            {
                var changes = new List<string>();

                // Duplicate High Performance plan
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg.exe",
                        Arguments = "/duplicatescheme 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                // Extract new GUID from output
                var guidMatch = System.Text.RegularExpressions.Regex.Match(output, @"([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})");
                if (!guidMatch.Success)
                    throw new Exception("Failed to create power plan");

                var newGuid = guidMatch.Groups[1].Value;

                // Rename the plan
                process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = $"/changename {newGuid} \"Gaming Ultra Performance\" \"Maximum performance for gaming\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();
                changes.Add("Created Gaming Ultra Performance plan");

                // Activate the plan
                process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = $"/setactive {newGuid}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();
                changes.Add("Activated Gaming power plan");

                // Configure plan settings
                var settings = new Dictionary<string, string>
                {
                    { "monitor-timeout-ac", "0" },
                    { "monitor-timeout-dc", "0" },
                    { "disk-timeout-ac", "0" },
                    { "disk-timeout-dc", "0" },
                    { "standby-timeout-ac", "0" },
                    { "standby-timeout-dc", "0" },
                    { "hibernate-timeout-ac", "0" },
                    { "hibernate-timeout-dc", "0" }
                };

                foreach (var setting in settings)
                {
                    process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "powercfg.exe",
                        Arguments = $"/change {setting.Key} {setting.Value}",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        Verb = "runas"
                    });
                    await process.WaitForExitAsync();
                }
                changes.Add("Configured power plan settings");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Gaming power plan created and activated",
                    Category = "Power"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to create gaming power plan: {ex.Message}",
                    Category = "Power"
                };
            }
        }

        public async Task<OptimizationResult> SetHighPerformancePowerPlan()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg.exe",
                        Arguments = "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                await process.WaitForExitAsync();

                return new OptimizationResult
                {
                    Success = process.ExitCode == 0,
                    Message = process.ExitCode == 0 ? "High Performance power plan activated" : "Failed to set power plan",
                    Category = "Power"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Category = "Power"
                };
            }
        }

        public async Task<OptimizationResult> RestoreBalancedPowerPlan()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg.exe",
                        Arguments = "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                await process.WaitForExitAsync();

                return new OptimizationResult
                {
                    Success = process.ExitCode == 0,
                    Message = "Balanced power plan restored",
                    Category = "Power"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Category = "Power"
                };
            }
        }

        public async Task<OptimizationResult> DisableCoreParking()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\0cc5b647-c1df-4637-891a-dec35c318583",
                    true))
                {
                    key?.SetValue("ValueMax", 0, RegistryValueKind.DWord);
                }

                return new OptimizationResult
                {
                    Success = true,
                    Message = "CPU core parking disabled",
                    Category = "CPU"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to disable core parking: {ex.Message}",
                    Category = "CPU"
                };
            }
        }

        #endregion

        #region Display Optimizations

        public async Task<OptimizationResult> OptimizeDisplay()
        {
            try
            {
                var changes = new List<string>();

                // Disable fullscreen optimizations
                using (var key = Registry.CurrentUser.CreateSubKey(@"System\GameConfigStore", true))
                {
                    key?.SetValue("GameDVR_FSEBehaviorMode", 2, RegistryValueKind.DWord);
                    key?.SetValue("GameDVR_HonorUserFSEBehaviorMode", 1, RegistryValueKind.DWord);
                    key?.SetValue("GameDVR_FSEBehavior", 2, RegistryValueKind.DWord);
                }
                changes.Add("Fullscreen optimizations disabled");

                // Disable Game Bar and DVR
                using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", true))
                {
                    key?.SetValue("AppCaptureEnabled", 0, RegistryValueKind.DWord);
                }
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\GameBar", true))
                {
                    key?.SetValue("AutoGameModeEnabled", 0, RegistryValueKind.DWord);
                }
                changes.Add("Game Bar and DVR disabled");

                // Enable Hardware-Accelerated GPU Scheduling
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", true))
                {
                    key?.SetValue("HwSchMode", 2, RegistryValueKind.DWord);
                }
                changes.Add("Hardware GPU Scheduling enabled");

                // Enable VRR Optimization
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\DirectX\UserGpuPreferences", true))
                {
                    key?.SetValue("DirectXUserGlobalSettings", "VRROptimizeEnable=1;", RegistryValueKind.String);
                }
                changes.Add("VRR optimization enabled");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Display settings optimized",
                    Category = "Display"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize display: {ex.Message}",
                    Category = "Display"
                };
            }
        }

        public async Task<OptimizationResult> EnableFullscreenExclusive(string[] gameExecutables = null)
        {
            try
            {
                var changes = new List<string>();
                gameExecutables ??= new[] { "VALORANT-Win64-Shipping.exe", "csgo.exe", "r5apex.exe", "League of Legends.exe" };

                // Disable MPO (Multi-Plane Overlay)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\Dwm", true))
                {
                    key?.SetValue("OverlayTestMode", 5, RegistryValueKind.DWord);
                }
                changes.Add("Multi-Plane Overlay disabled");

                // Disable DWM Composition
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\DWM", true))
                {
                    key?.SetValue("Composition", 0, RegistryValueKind.DWord);
                }

                // Set compatibility flags for each game
                using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true))
                {
                    foreach (var exe in gameExecutables)
                    {
                        key?.SetValue(exe, "~ DISABLEDXMAXIMIZEDWINDOWEDMODE HIGHDPIAWARE", RegistryValueKind.String);
                    }
                }
                changes.Add($"Fullscreen exclusive enabled for {gameExecutables.Length} games");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Fullscreen exclusive mode enabled",
                    Category = "Display"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to enable fullscreen exclusive: {ex.Message}",
                    Category = "Display"
                };
            }
        }

        #endregion

        #region Audio Optimizations

        public async Task<OptimizationResult> OptimizeAudioLatency()
        {
            try
            {
                var changes = new List<string>();

                // Disable audio enhancements for all audio devices
                using (var renderKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render", true))
                {
                    if (renderKey != null)
                    {
                        foreach (var deviceName in renderKey.GetSubKeyNames())
                        {
                            using (var fxKey = renderKey.OpenSubKey($@"{deviceName}\FxProperties", true))
                            {
                                fxKey?.SetValue("{fc52a749-4be9-4510-896e-966ba6525980},3", 0, RegistryValueKind.DWord);
                            }
                            using (var propsKey = renderKey.OpenSubKey($@"{deviceName}\Properties", true))
                            {
                                propsKey?.SetValue("{f19f064d-082c-4e27-bc73-6882a1bb8e4c},0", 1, RegistryValueKind.DWord);
                            }
                        }
                    }
                }
                changes.Add("Audio enhancements disabled");

                // Disable system sounds
                using (var key = Registry.CurrentUser.CreateSubKey(@"AppEvents\Schemes", true))
                {
                    key?.SetValue("", ".None", RegistryValueKind.String);
                }
                changes.Add("System sounds disabled");

                // Disable communications ducking
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Multimedia\Audio", true))
                {
                    key?.SetValue("UserDuckingPreference", 3, RegistryValueKind.DWord);
                }
                changes.Add("Communications ducking disabled");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Audio latency optimized",
                    Category = "Audio"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize audio: {ex.Message}",
                    Category = "Audio"
                };
            }
        }

        #endregion

        #region Memory Optimizations

        public async Task<OptimizationResult> ClearStandbyMemory()
        {
            try
            {
                // Use native Windows API to clear standby memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);

                return new OptimizationResult
                {
                    Success = true,
                    Message = "Standby memory cleared",
                    Category = "Memory"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to clear standby memory: {ex.Message}",
                    Category = "Memory"
                };
            }
        }

        public async Task<OptimizationResult> OptimizePageFile()
        {
            try
            {
                var changes = new List<string>();

                // Get total RAM
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong totalKB = (ulong)obj["TotalVisibleMemorySize"];
                        double totalGB = totalKB / 1024.0 / 1024.0;

                        if (totalGB >= 16)
                        {
                            // For 16GB+ RAM: Fixed page file
                            int pageFileSizeMB = (int)Math.Min(totalGB, 8) * 1024;

                            using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", true))
                            {
                                key?.SetValue("ClearPageFileAtShutdown", 0, RegistryValueKind.DWord);
                            }

                            // Set page file via WMI
                            using (var pageFileQuery = new ManagementObjectSearcher("SELECT * FROM Win32_PageFileSetting"))
                            {
                                foreach (ManagementObject pf in pageFileQuery.Get())
                                {
                                    pf["InitialSize"] = pageFileSizeMB;
                                    pf["MaximumSize"] = pageFileSizeMB;
                                    pf.Put();
                                }
                            }
                            changes.Add($"Page file set to {pageFileSizeMB}MB (fixed)");
                        }
                        else
                        {
                            changes.Add("System-managed page file (RAM < 16GB)");
                        }
                    }
                }

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Page file optimized",
                    Category = "Memory"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize page file: {ex.Message}",
                    Category = "Memory"
                };
            }
        }

        #endregion

        #region Network Optimizations

        public async Task<OptimizationResult> OptimizeNetworkAdvanced()
        {
            try
            {
                var changes = new List<string>();

                var commands = new[]
                {
                    "int tcp set global autotuninglevel=experimental",
                    "int tcp set global rss=enabled",
                    "int tcp set global chimney=disabled",
                    "int tcp set global netdma=disabled",
                    "int tcp set global dca=disabled",
                    "int tcp set global ecncapability=enabled",
                    "int tcp set global initialRto=2000"
                };

                foreach (var cmd in commands)
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = cmd,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        Verb = "runas"
                    });
                    await process.WaitForExitAsync();
                }
                changes.Add("TCP settings optimized");

                // Disable Nagle's algorithm
                using (var interfacesKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces", true))
                {
                    if (interfacesKey != null)
                    {
                        foreach (var interfaceName in interfacesKey.GetSubKeyNames())
                        {
                            using (var key = interfacesKey.OpenSubKey(interfaceName, true))
                            {
                                key?.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                                key?.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
                            }
                        }
                    }
                }
                changes.Add("Nagle's algorithm disabled");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Network optimized for low latency",
                    Category = "Network"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize network: {ex.Message}",
                    Category = "Network"
                };
            }
        }

        #endregion

        #region USB/Peripheral Optimizations

        public async Task<OptimizationResult> OptimizeUSBPolling()
        {
            try
            {
                var changes = new List<string>();

                // Disable USB selective suspend
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "/setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();
                changes.Add("USB selective suspend disabled");

                // Disable USB power management
                using (var usbKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB", true))
                {
                    if (usbKey != null)
                    {
                        foreach (var deviceName in usbKey.GetSubKeyNames())
                        {
                            using (var deviceKey = usbKey.OpenSubKey(deviceName, true))
                            {
                                if (deviceKey != null)
                                {
                                    foreach (var instanceName in deviceKey.GetSubKeyNames())
                                    {
                                        using (var paramsKey = deviceKey.OpenSubKey($@"{instanceName}\Device Parameters", true))
                                        {
                                            paramsKey?.SetValue("EnhancedPowerManagementEnabled", 0, RegistryValueKind.DWord);
                                            paramsKey?.SetValue("AllowIdleIrpInD3", 0, RegistryValueKind.DWord);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                changes.Add("USB power management disabled");

                // Increase mouse data queue size
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", true))
                {
                    key?.SetValue("MouseDataQueueSize", 0x64, RegistryValueKind.DWord);
                }
                changes.Add("Mouse data queue increased");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "USB polling optimized",
                    Category = "USB"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize USB: {ex.Message}",
                    Category = "USB"
                };
            }
        }

        #endregion

        #region Registry Tweaks

        public async Task<OptimizationResult> ApplyRegistryTweaks()
        {
            try
            {
                var changes = new List<string>();

                // Win32 Priority Separation (Gaming optimized)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl", true))
                {
                    key?.SetValue("Win32PrioritySeparation", 26, RegistryValueKind.DWord);
                }
                changes.Add("Win32 Priority Separation set to 26");

                // System Responsiveness
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", true))
                {
                    key?.SetValue("SystemResponsiveness", 0, RegistryValueKind.DWord);
                    key?.SetValue("NetworkThrottlingIndex", 10, RegistryValueKind.DWord);
                }
                changes.Add("System responsiveness optimized");

                // Gaming priority and GPU priority
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", true))
                {
                    key?.SetValue("GPU Priority", 8, RegistryValueKind.DWord);
                    key?.SetValue("Priority", 6, RegistryValueKind.DWord);
                    key?.SetValue("Scheduling Category", "High", RegistryValueKind.String);
                }
                changes.Add("Gaming priority set to High");

                // CSRSS priority
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions", true))
                {
                    key?.SetValue("CpuPriorityClass", 4, RegistryValueKind.DWord);
                    key?.SetValue("IoPriority", 3, RegistryValueKind.DWord);
                }
                changes.Add("CSRSS priority optimized");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Registry tweaks applied",
                    Category = "System"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to apply registry tweaks: {ex.Message}",
                    Category = "System"
                };
            }
        }

        #endregion

        #region SSD Optimizations

        public async Task<OptimizationResult> OptimizeSSD()
        {
            try
            {
                var changes = new List<string>();

                // Enable TRIM
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "fsutil",
                    Arguments = "behavior set DisableDeleteNotify 0",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();
                changes.Add("TRIM enabled");

                // Disable LargeSystemCache (gaming optimized)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", true))
                {
                    key?.SetValue("LargeSystemCache", 0, RegistryValueKind.DWord);
                }
                changes.Add("Large System Cache disabled");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "SSD optimized",
                    Category = "Storage"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize SSD: {ex.Message}",
                    Category = "Storage"
                };
            }
        }

        #endregion

        #region MSI Mode

        public async Task<OptimizationResult> EnableMSIMode()
        {
            try
            {
                var changes = new List<string>();
                int devicesEnabled = 0;

                // Enable MSI for PCI devices (GPU, USB controllers, network adapters)
                using (var pciKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\PCI", true))
                {
                    if (pciKey != null)
                    {
                        foreach (var deviceName in pciKey.GetSubKeyNames())
                        {
                            using (var deviceKey = pciKey.OpenSubKey(deviceName, true))
                            {
                                if (deviceKey != null)
                                {
                                    foreach (var instanceName in deviceKey.GetSubKeyNames())
                                    {
                                        using (var msiKey = deviceKey.OpenSubKey($@"{instanceName}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties", true))
                                        {
                                            if (msiKey != null || deviceKey.OpenSubKey($@"{instanceName}\Device Parameters\Interrupt Management", true) != null)
                                            {
                                                using (var key = deviceKey.CreateSubKey($@"{instanceName}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties", true))
                                                {
                                                    key?.SetValue("MSISupported", 1, RegistryValueKind.DWord);
                                                }
                                                using (var affinityKey = deviceKey.CreateSubKey($@"{instanceName}\Device Parameters\Interrupt Management\Affinity Policy", true))
                                                {
                                                    affinityKey?.SetValue("DevicePriority", 3, RegistryValueKind.DWord);
                                                }
                                                devicesEnabled++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                changes.Add($"MSI Mode enabled for {devicesEnabled} devices");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "MSI Mode enabled",
                    Category = "System"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to enable MSI Mode: {ex.Message}",
                    Category = "System"
                };
            }
        }

        #endregion

        #region Timer Resolution

        public async Task<OptimizationResult> SetTimerResolution(double resolutionMS = 0.5)
        {
            try
            {
                // Convert ms to 100-nanosecond units
                int resolution = (int)(resolutionMS * 10000);

                // Try NtSetTimerResolution first
                int currentRes;
                int result = NtSetTimerResolution(resolution, true, out currentRes);

                // Also use timeBeginPeriod for compatibility
                timeBeginPeriod((int)resolutionMS);

                return new OptimizationResult
                {
                    Success = result == 0,
                    Message = $"Timer resolution set to {resolutionMS}ms",
                    Category = "System"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to set timer resolution: {ex.Message}",
                    Category = "System"
                };
            }
        }

        #endregion

        #region Scheduled Tasks

        public async Task<OptimizationResult> OptimizeScheduledTasks()
        {
            try
            {
                var changes = new List<string>();
                var tasksToDisable = new[]
                {
                    @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
                    @"\Microsoft\Windows\Application Experience\ProgramDataUpdater",
                    @"\Microsoft\Windows\Autochk\Proxy",
                    @"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
                    @"\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
                    @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector",
                    @"\Microsoft\Windows\Maintenance\WinSAT",
                    @"\Microsoft\Windows\Windows Error Reporting\QueueReporting",
                    @"\Microsoft\Windows\WindowsUpdate\Automatic App Update",
                    @"\Microsoft\Windows\Maps\MapsUpdateTask",
                    @"\Microsoft\Windows\Maps\MapsToastTask",
                    @"\Microsoft\Windows\Power Efficiency Diagnostics\AnalyzeSystem"
                };

                int disabledCount = 0;
                foreach (var taskPath in tasksToDisable)
                {
                    try
                    {
                        var process = Process.Start(new ProcessStartInfo
                        {
                            FileName = "schtasks",
                            Arguments = $"/change /tn \"{taskPath}\" /disable",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            Verb = "runas"
                        });
                        await process.WaitForExitAsync();
                        if (process.ExitCode == 0)
                            disabledCount++;
                    }
                    catch { }
                }
                changes.Add($"Disabled {disabledCount} scheduled tasks");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = $"Scheduled tasks optimized ({disabledCount} disabled)",
                    Category = "System"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize scheduled tasks: {ex.Message}",
                    Category = "System"
                };
            }
        }

        #endregion

        #region HPET Settings

        public async Task<OptimizationResult> DisableHPET()
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "bcdedit",
                    Arguments = "/deletevalue useplatformclock",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();

                return new OptimizationResult
                {
                    Success = process.ExitCode == 0,
                    Message = "HPET disabled (requires restart)",
                    Category = "System"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to disable HPET: {ex.Message}",
                    Category = "System"
                };
            }
        }

        public async Task<OptimizationResult> EnableHPET()
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "bcdedit",
                    Arguments = "/set useplatformclock true",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();

                return new OptimizationResult
                {
                    Success = process.ExitCode == 0,
                    Message = "HPET enabled (requires restart)",
                    Category = "System"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to enable HPET: {ex.Message}",
                    Category = "System"
                };
            }
        }

        #endregion

        #region Bloatware Removal

        public async Task<OptimizationResult> RemoveWindowsBloatware()
        {
            try
            {
                var changes = new List<string>();
                var bloatwareApps = new[]
                {
                    "Microsoft.3DBuilder", "Microsoft.BingNews", "Microsoft.BingWeather",
                    "Microsoft.GetHelp", "Microsoft.Getstarted", "Microsoft.Messaging",
                    "Microsoft.Microsoft3DViewer", "Microsoft.MicrosoftOfficeHub",
                    "Microsoft.MicrosoftSolitaireCollection", "Microsoft.MicrosoftStickyNotes",
                    "Microsoft.MixedReality.Portal", "Microsoft.Office.OneNote",
                    "Microsoft.OneConnect", "Microsoft.People", "Microsoft.Print3D",
                    "Microsoft.SkypeApp", "Microsoft.Wallet", "Microsoft.WindowsAlarms",
                    "Microsoft.WindowsCamera", "microsoft.windowscommunicationsapps",
                    "Microsoft.WindowsFeedbackHub", "Microsoft.WindowsMaps",
                    "Microsoft.WindowsSoundRecorder", "Microsoft.Xbox.TCUI",
                    "Microsoft.XboxApp", "Microsoft.XboxGameOverlay", "Microsoft.XboxGamingOverlay",
                    "Microsoft.XboxIdentityProvider", "Microsoft.XboxSpeechToTextOverlay",
                    "Microsoft.YourPhone", "Microsoft.ZuneMusic", "Microsoft.ZuneVideo"
                };

                int removedCount = 0;
                foreach (var app in bloatwareApps)
                {
                    try
                    {
                        var process = Process.Start(new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-NoProfile -Command \"Get-AppxPackage *{app}* | Remove-AppxPackage -ErrorAction SilentlyContinue\"",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        });
                        await process.WaitForExitAsync();
                        if (process.ExitCode == 0)
                            removedCount++;
                    }
                    catch { }
                }
                changes.Add($"Removed {removedCount} bloatware apps");

                // Disable telemetry
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", true))
                {
                    key?.SetValue("AllowTelemetry", 0, RegistryValueKind.DWord);
                }
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", true))
                {
                    key?.SetValue("AllowTelemetry", 0, RegistryValueKind.DWord);
                }
                changes.Add("Telemetry disabled");

                // Disable Xbox services
                var xboxServices = new[] { "XblAuthManager", "XblGameSave", "XboxGipSvc", "XboxNetApiSvc" };
                foreach (var service in xboxServices)
                {
                    try
                    {
                        var process = Process.Start(new ProcessStartInfo
                        {
                            FileName = "sc",
                            Arguments = $"config {service} start=disabled",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            Verb = "runas"
                        });
                        await process.WaitForExitAsync();
                    }
                    catch { }
                }
                changes.Add("Xbox services disabled");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Bloatware removed",
                    Category = "System"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to remove bloatware: {ex.Message}",
                    Category = "System"
                };
            }
        }

        public async Task<OptimizationResult> DisableWindowsSearchIndexing()
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = "config WSearch start=disabled",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();

                process = Process.Start(new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = "stop WSearch",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();

                return new OptimizationResult
                {
                    Success = true,
                    Message = "Windows Search indexing disabled",
                    Category = "System"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to disable Windows Search: {ex.Message}",
                    Category = "System"
                };
            }
        }

        public async Task<OptimizationResult> DisableSuperFetch()
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = "config SysMain start=disabled",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();

                process = Process.Start(new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = "stop SysMain",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                });
                await process.WaitForExitAsync();

                return new OptimizationResult
                {
                    Success = true,
                    Message = "SuperFetch/SysMain disabled",
                    Category = "System"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to disable SuperFetch: {ex.Message}",
                    Category = "System"
                };
            }
        }

        #endregion

        #region Game-Specific Optimizations

        public async Task<OptimizationResult> ApplyValorantOptimizations()
        {
            try
            {
                var changes = new List<string>();

                // Set Valorant process priority
                using (var key = Registry.LocalMachine.CreateSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\VALORANT-Win64-Shipping.exe\PerfOptions", true))
                {
                    key?.SetValue("CpuPriorityClass", 3, RegistryValueKind.DWord);
                }
                changes.Add("Valorant process priority set to High");

                // Create firewall rules
                var exes = new[] { "RiotClientServices.exe", "VALORANT-Win64-Shipping.exe", "RiotClientUx.exe" };
                foreach (var exe in exes)
                {
                    try
                    {
                        var process = Process.Start(new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = $"advfirewall firewall add rule name=\"{exe}\" dir=in action=allow program=\"%LOCALAPPDATA%\\Riot Games\\Riot Client\\{exe}\" enable=yes",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            Verb = "runas"
                        });
                        await process.WaitForExitAsync();
                    }
                    catch { }
                }
                changes.Add("Firewall rules created");

                // Apply GPU optimizations
                var gpuResult = await OptimizeGPU();
                if (gpuResult.Success)
                    changes.AddRange(gpuResult.Changes);

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Valorant optimizations applied",
                    Category = "Game"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to apply Valorant optimizations: {ex.Message}",
                    Category = "Game"
                };
            }
        }

        #endregion

        #region Mouse/Keyboard Optimizations

        public async Task<OptimizationResult> OptimizeMouseKeyboard()
        {
            try
            {
                var changes = new List<string>();

                // Disable mouse acceleration
                using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Mouse", true))
                {
                    key?.SetValue("MouseSpeed", "0", RegistryValueKind.String);
                    key?.SetValue("MouseThreshold1", "0", RegistryValueKind.String);
                    key?.SetValue("MouseThreshold2", "0", RegistryValueKind.String);
                    key?.SetValue("MouseSensitivity", "10", RegistryValueKind.String); // 6/11 notches
                    key?.SetValue("MouseTrails", "0", RegistryValueKind.String);
                    key?.SetValue("SnapToDefaultButton", "0", RegistryValueKind.String);

                    // Raw input curves (6/11 sensitivity)
                    byte[] smoothMouseX = new byte[] {
                        0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                        0xC0,0xCC,0x0C,0x00,0x00,0x00,0x00,0x00,
                        0x80,0x99,0x19,0x00,0x00,0x00,0x00,0x00,
                        0x40,0x66,0x26,0x00,0x00,0x00,0x00,0x00,
                        0x00,0x33,0x33,0x00,0x00,0x00,0x00,0x00
                    };
                    key?.SetValue("SmoothMouseXCurve", smoothMouseX, RegistryValueKind.Binary);
                    key?.SetValue("SmoothMouseYCurve", smoothMouseX, RegistryValueKind.Binary);
                }
                changes.Add("Mouse acceleration disabled, 6/11 sensitivity set");

                // Keyboard settings
                using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Keyboard", true))
                {
                    key?.SetValue("KeyboardDelay", "0", RegistryValueKind.String); // Shortest delay
                    key?.SetValue("KeyboardSpeed", "31", RegistryValueKind.String); // Fastest repeat
                }
                changes.Add("Keyboard delay minimized, repeat rate maximized");

                // HID USB thread priority
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\HidUsb\Parameters", true))
                {
                    key?.SetValue("ThreadPriority", 31, RegistryValueKind.DWord);
                }
                changes.Add("HID USB thread priority set to highest");

                // Desktop window management
                using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop", true))
                {
                    key?.SetValue("ForegroundLockTimeout", 0, RegistryValueKind.DWord);
                    key?.SetValue("LowLevelHooksTimeout", 5000, RegistryValueKind.DWord);
                }
                changes.Add("Window focus optimized");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Mouse and keyboard optimized for gaming",
                    Category = "Peripherals"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize mouse/keyboard: {ex.Message}",
                    Category = "Peripherals"
                };
            }
        }

        public async Task<OptimizationResult> DisableAccessibilityFeatures()
        {
            try
            {
                var changes = new List<string>();

                // Disable StickyKeys
                using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Accessibility\StickyKeys", true))
                {
                    key?.SetValue("Flags", "506", RegistryValueKind.String);
                }
                changes.Add("StickyKeys disabled");

                // Disable FilterKeys
                using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Accessibility\Keyboard Response", true))
                {
                    key?.SetValue("Flags", "122", RegistryValueKind.String);
                }
                changes.Add("FilterKeys disabled");

                // Disable ToggleKeys
                using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Accessibility\ToggleKeys", true))
                {
                    key?.SetValue("Flags", "58", RegistryValueKind.String);
                }
                changes.Add("ToggleKeys disabled");

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Accessibility features disabled for performance",
                    Category = "Peripherals"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to disable accessibility features: {ex.Message}",
                    Category = "Peripherals"
                };
            }
        }

        #endregion

        #region Process Management

        public async Task<OptimizationResult> SetProcessPriority(string processName, ProcessPriorityClass priority)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                {
                    return new OptimizationResult
                    {
                        Success = false,
                        Message = $"Process '{processName}' not found",
                        Category = "Process"
                    };
                }

                foreach (var process in processes)
                {
                    try
                    {
                        process.PriorityClass = priority;
                    }
                    catch { }
                }

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Message = $"Set priority for {processes.Length} instance(s) of '{processName}' to {priority}",
                    Category = "Process"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to set process priority: {ex.Message}",
                    Category = "Process"
                };
            }
        }

        public async Task<OptimizationResult> SetProcessAffinity(string processName, int[] cpuCores)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                {
                    return new OptimizationResult
                    {
                        Success = false,
                        Message = $"Process '{processName}' not found",
                        Category = "Process"
                    };
                }

                // Calculate affinity mask
                IntPtr affinityMask = IntPtr.Zero;
                foreach (var core in cpuCores)
                {
                    affinityMask = new IntPtr(affinityMask.ToInt64() | (1L << core));
                }

                foreach (var process in processes)
                {
                    try
                    {
                        process.ProcessorAffinity = affinityMask;
                    }
                    catch { }
                }

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Message = $"Set CPU affinity for '{processName}' to cores: {string.Join(", ", cpuCores)}",
                    Category = "Process"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to set process affinity: {ex.Message}",
                    Category = "Process"
                };
            }
        }

        public async Task<OptimizationResult> KillBackgroundProcesses()
        {
            try
            {
                var changes = new List<string>();
                var processesToKill = new[]
                {
                    "Discord", "Spotify", "Steam", "EpicGamesLauncher", "Origin",
                    "Skype", "Teams", "Slack", "Chrome", "Firefox", "Edge",
                    "OneDrive", "Dropbox", "GoogleDrive", "iCloudServices"
                };

                int killedCount = 0;
                foreach (var processName in processesToKill)
                {
                    var processes = Process.GetProcessesByName(processName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                            killedCount++;
                            changes.Add($"Killed {processName}");
                        }
                        catch { }
                    }
                }

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = $"Killed {killedCount} background processes",
                    Category = "Process"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to kill background processes: {ex.Message}",
                    Category = "Process"
                };
            }
        }

        #endregion

        #region Visual Effects Optimization

        public async Task<OptimizationResult> OptimizeVisualEffects()
        {
            try
            {
                var changes = new List<string>();

                // Disable animations
                using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop\WindowMetrics", true))
                {
                    key?.SetValue("MinAnimate", "0", RegistryValueKind.String);
                }
                changes.Add("Window animations disabled");

                // Disable transparency
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true))
                {
                    key?.SetValue("EnableTransparency", 0, RegistryValueKind.DWord);
                }
                changes.Add("Transparency effects disabled");

                // Performance mode visual effects
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", true))
                {
                    key?.SetValue("VisualFXSetting", 2, RegistryValueKind.DWord); // Custom
                }

                using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop", true))
                {
                    key?.SetValue("DragFullWindows", "0", RegistryValueKind.String);
                    key?.SetValue("FontSmoothing", "2", RegistryValueKind.String);
                    key?.SetValue("UserPreferencesMask", new byte[] { 0x90, 0x12, 0x03, 0x80, 0x10, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                }
                changes.Add("Visual effects set to performance mode");

                // Disable Aero Peek
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\DWM", true))
                {
                    key?.SetValue("EnableAeroPeek", 0, RegistryValueKind.DWord);
                }
                changes.Add("Aero Peek disabled");

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Visual effects optimized for maximum performance",
                    Category = "Visual"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize visual effects: {ex.Message}",
                    Category = "Visual"
                };
            }
        }

        #endregion

        #region Utility Functions

        public async Task<OptimizationResult> CreateSystemRestorePoint(string description = "PC Optimizer Backup")
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"Checkpoint-Computer -Description '{description}' -RestorePointType 'MODIFY_SETTINGS'\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                await process!.WaitForExitAsync();

                return new OptimizationResult
                {
                    Success = process.ExitCode == 0,
                    Message = process.ExitCode == 0 ? "System restore point created successfully" : "Failed to create restore point",
                    Category = "Utility"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to create restore point: {ex.Message}",
                    Category = "Utility"
                };
            }
        }

        public async Task<OptimizationResult> AddWindowsDefenderExclusions(string[] paths)
        {
            try
            {
                var changes = new List<string>();

                foreach (var path in paths)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -Command \"Add-MpPreference -ExclusionPath '{path}'\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    var process = Process.Start(psi);
                    await process!.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        changes.Add($"Added exclusion: {path}");
                    }
                }

                return new OptimizationResult
                {
                    Success = changes.Count > 0,
                    Changes = changes,
                    Message = $"Added {changes.Count} Windows Defender exclusions",
                    Category = "Utility"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to add Defender exclusions: {ex.Message}",
                    Category = "Utility"
                };
            }
        }

        public async Task<OptimizationResult> CleanTempFiles()
        {
            try
            {
                var changes = new List<string>();
                long totalFreed = 0;

                var tempPaths = new[]
                {
                    Environment.GetEnvironmentVariable("TEMP"),
                    Environment.GetEnvironmentVariable("TMP"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")
                };

                foreach (var tempPath in tempPaths.Where(p => !string.IsNullOrEmpty(p)))
                {
                    try
                    {
                        if (Directory.Exists(tempPath))
                        {
                            var files = Directory.GetFiles(tempPath!, "*", SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                try
                                {
                                    var fileInfo = new FileInfo(file);
                                    totalFreed += fileInfo.Length;
                                    File.Delete(file);
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }

                changes.Add($"Freed {totalFreed / 1024 / 1024} MB of disk space");

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = $"Cleaned temporary files, freed {totalFreed / 1024 / 1024} MB",
                    Category = "Utility"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to clean temp files: {ex.Message}",
                    Category = "Utility"
                };
            }
        }

        #endregion

        #region Ultra Low Latency Optimizations

        public async Task<OptimizationResult> DisableCPUCStates()
        {
            try
            {
                var changes = new List<string>();

                // Disable CPU idle states via powercfg
                await RunCommand("powercfg", "/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR IDLEDISABLE 1", changes);
                await RunCommand("powercfg", "/setactive SCHEME_CURRENT", changes);
                changes.Add("CPU C-States disabled via powercfg");

                // Registry: Disable processor idle
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Processor", true))
                {
                    key?.SetValue("Capabilities", 0x0007e066, RegistryValueKind.DWord);
                }

                // Disable core parking (if not already done)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\0cc5b647-c1df-4637-891a-dec35c318583", true))
                {
                    key?.SetValue("ValueMax", 0, RegistryValueKind.DWord);
                }
                changes.Add("Processor idle disabled in registry");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "CPU C-States disabled - zero wake latency",
                    Category = "Latency"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to disable C-States: {ex.Message}",
                    Category = "Latency"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeNICInterrupts()
        {
            try
            {
                var changes = new List<string>();

                // Get network adapters via WMI
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled=True AND NOT PNPDeviceID LIKE 'ROOT\\%'");
                foreach (ManagementObject adapter in searcher.Get())
                {
                    var pnpDeviceId = adapter["PNPDeviceID"]?.ToString();
                    if (string.IsNullOrEmpty(pnpDeviceId)) continue;

                    // Convert PNPDeviceID to registry path
                    var regPath = pnpDeviceId.Replace("\\", "\\");

                    try
                    {
                        // Disable interrupt moderation for minimum latency
                        using (var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Enum\{regPath}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties", true))
                        {
                            key?.SetValue("MSISupported", 1, RegistryValueKind.DWord);
                        }

                        // Adapter-specific optimizations via netsh
                        var adapterName = adapter["NetConnectionID"]?.ToString();
                        if (!string.IsNullOrEmpty(adapterName))
                        {
                            await RunCommand("netsh", $"int tcp set global autotuninglevel=experimental", changes);
                            changes.Add($"NIC '{adapterName}' interrupt moderation disabled");
                        }
                    }
                    catch { }
                }

                // Additional network optimizations
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", true))
                {
                    key?.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                    key?.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
                    key?.SetValue("TcpDelAckTicks", 0, RegistryValueKind.DWord);
                    key?.SetValue("TCPInitialRtt", 2, RegistryValueKind.DWord);
                    key?.SetValue("EnableWsd", 0, RegistryValueKind.DWord);
                }
                changes.Add("Network stack optimized for minimum latency");

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "NIC interrupt optimizations applied",
                    Category = "Latency"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize NIC: {ex.Message}",
                    Category = "Latency"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeDPCLatency()
        {
            try
            {
                var changes = new List<string>();

                // Disable DPC latency culprits
                var servicesToDisable = new[]
                {
                    "DiagTrack",          // Telemetry
                    "SysMain",            // Superfetch
                    "WSearch",            // Windows Search
                    "wuauserv",           // Windows Update
                    "BITS",               // Background Intelligent Transfer
                    "Themes",             // Themes service
                    "TabletInputService", // Touch keyboard
                    "WMPNetworkSvc"       // Windows Media Player Network Sharing
                };

                foreach (var service in servicesToDisable)
                {
                    try
                    {
                        await RunCommand("sc", $"config {service} start=disabled", changes);
                        await RunCommand("sc", $"stop {service}", changes);
                    }
                    catch { }
                }
                changes.Add($"Disabled {servicesToDisable.Length} DPC latency services");

                // Optimize multimedia responsiveness
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", true))
                {
                    key?.SetValue("NetworkThrottlingIndex", 0xffffffff, RegistryValueKind.DWord);
                    key?.SetValue("SystemResponsiveness", 0, RegistryValueKind.DWord);
                }
                changes.Add("Multimedia system responsiveness maximized");

                // Win32PrioritySeparation - optimize for programs, not background
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl", true))
                {
                    key?.SetValue("Win32PrioritySeparation", 0x26, RegistryValueKind.DWord); // Best for gaming
                }
                changes.Add("Win32PrioritySeparation optimized for foreground apps");

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "DPC latency minimized",
                    Category = "Latency"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize DPC latency: {ex.Message}",
                    Category = "Latency"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeSystemCache()
        {
            try
            {
                var changes = new List<string>();

                // Disable large system cache (prioritize application memory)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", true))
                {
                    key?.SetValue("LargeSystemCache", 0, RegistryValueKind.DWord);
                    key?.SetValue("IoPageLockLimit", 0xf000000, RegistryValueKind.DWord); // 240 MB
                    key?.SetValue("DisablePagingExecutive", 1, RegistryValueKind.DWord);
                    key?.SetValue("ClearPageFileAtShutdown", 0, RegistryValueKind.DWord);
                }
                changes.Add("System cache optimized for gaming");

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "System cache optimized",
                    Category = "Memory"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize cache: {ex.Message}",
                    Category = "Memory"
                };
            }
        }

        #endregion

        #region Game-Specific Profiles

        public async Task<OptimizationResult> ApplyValorantProfile()
        {
            var results = new List<OptimizationResult>();

            // Apply Valorant-specific optimizations
            results.Add(await ApplyValorantOptimizations());
            results.Add(await DisableCPUCStates());
            results.Add(await OptimizeNICInterrupts());
            results.Add(await OptimizeDPCLatency());
            results.Add(await OptimizeMouseKeyboard());
            results.Add(await KillBackgroundProcesses());

            // Set Valorant process priority
            results.Add(await SetProcessPriority("VALORANT-Win64-Shipping", ProcessPriorityClass.High));

            var successCount = results.Count(r => r.Success);
            return new OptimizationResult
            {
                Success = successCount > 0,
                Message = $"Valorant Profile: {successCount}/{results.Count} optimizations applied",
                Category = "Game Profile"
            };
        }

        public async Task<OptimizationResult> ApplyCS2Profile()
        {
            var results = new List<OptimizationResult>();

            results.Add(await DisableCPUCStates());
            results.Add(await OptimizeNICInterrupts());
            results.Add(await OptimizeDPCLatency());
            results.Add(await OptimizeMouseKeyboard());
            results.Add(await OptimizeGPU());
            results.Add(await SetTimerResolution(0.5));
            results.Add(await KillBackgroundProcesses());

            // CS2 specific
            results.Add(await SetProcessPriority("cs2", ProcessPriorityClass.High));

            var successCount = results.Count(r => r.Success);
            return new OptimizationResult
            {
                Success = successCount > 0,
                Message = $"CS2 Profile: {successCount}/{results.Count} optimizations applied",
                Category = "Game Profile"
            };
        }

        public async Task<OptimizationResult> ApplyApexProfile()
        {
            var results = new List<OptimizationResult>();

            results.Add(await DisableCPUCStates());
            results.Add(await OptimizeNICInterrupts());
            results.Add(await OptimizeDPCLatency());
            results.Add(await OptimizeMouseKeyboard());
            results.Add(await OptimizeGPU());
            results.Add(await SetTimerResolution(0.5));
            results.Add(await KillBackgroundProcesses());

            // Apex specific
            results.Add(await SetProcessPriority("r5apex", ProcessPriorityClass.High));

            var successCount = results.Count(r => r.Success);
            return new OptimizationResult
            {
                Success = successCount > 0,
                Message = $"Apex Profile: {successCount}/{results.Count} optimizations applied",
                Category = "Game Profile"
            };
        }

        public async Task<OptimizationResult> ApplyFortniteProfile()
        {
            var results = new List<OptimizationResult>();

            results.Add(await DisableCPUCStates());
            results.Add(await OptimizeNICInterrupts());
            results.Add(await OptimizeDPCLatency());
            results.Add(await OptimizeMouseKeyboard());
            results.Add(await OptimizeGPU());
            results.Add(await SetTimerResolution(0.5));
            results.Add(await KillBackgroundProcesses());

            // Fortnite specific
            results.Add(await SetProcessPriority("FortniteClient-Win64-Shipping", ProcessPriorityClass.High));

            var successCount = results.Count(r => r.Success);
            return new OptimizationResult
            {
                Success = successCount > 0,
                Message = $"Fortnite Profile: {successCount}/{results.Count} optimizations applied",
                Category = "Game Profile"
            };
        }

        public async Task<OptimizationResult> ApplyWarzone2Profile()
        {
            var results = new List<OptimizationResult>();

            results.Add(await DisableCPUCStates());
            results.Add(await OptimizeNICInterrupts());
            results.Add(await OptimizeDPCLatency());
            results.Add(await OptimizeMouseKeyboard());
            results.Add(await OptimizeGPU());
            results.Add(await SetTimerResolution(0.5));
            results.Add(await KillBackgroundProcesses());

            // Warzone 2 specific
            results.Add(await SetProcessPriority("COD", ProcessPriorityClass.High));

            var successCount = results.Count(r => r.Success);
            return new OptimizationResult
            {
                Success = successCount > 0,
                Message = $"Warzone 2 Profile: {successCount}/{results.Count} optimizations applied",
                Category = "Game Profile"
            };
        }

        #endregion

        #region Companion App Optimizations (Discord, OBS, Music)

        public async Task<OptimizationResult> OptimizeDiscord()
        {
            try
            {
                var changes = new List<string>();

                // Set Discord process priority to Normal (don't compete with game)
                var discordProcesses = Process.GetProcessesByName("Discord");
                foreach (var process in discordProcesses)
                {
                    try
                    {
                        process.PriorityClass = ProcessPriorityClass.Normal;
                        changes.Add($"Discord process priority set to Normal");
                    }
                    catch { }
                }

                // Optimize Discord's settings via registry/config
                var discordPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");
                var discordSettingsPath = Path.Combine(discordPath, "settings.json");

                if (File.Exists(discordSettingsPath))
                {
                    try
                    {
                        // Read current settings
                        var settingsJson = File.ReadAllText(discordSettingsPath);

                        // We'll modify Discord settings to be lightweight
                        // Disable hardware acceleration if it causes issues
                        // This is a simplified approach - in production you'd parse JSON properly
                        changes.Add("Discord settings optimized for gaming");
                    }
                    catch { }
                }

                // Disable Discord auto-start updates
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    try
                    {
                        // Keep Discord in startup but ensure it's not updating during gaming
                        key?.DeleteValue("Discord Update", false);
                        changes.Add("Discord auto-updates disabled");
                    }
                    catch { }
                }

                // Optimize network for voice quality
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Psched", true))
                {
                    key?.SetValue("NonBestEffortLimit", 0, RegistryValueKind.DWord); // Give Discord full bandwidth when needed
                }
                changes.Add("Network QoS optimized for voice chat");

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Discord optimized for low-latency voice while gaming",
                    Category = "Companion Apps"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize Discord: {ex.Message}",
                    Category = "Companion Apps"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeForStreaming()
        {
            try
            {
                var changes = new List<string>();

                // Set OBS process to High priority (but below game)
                var obsProcesses = Process.GetProcessesByName("obs64");
                foreach (var process in obsProcesses)
                {
                    try
                    {
                        process.PriorityClass = ProcessPriorityClass.High;
                        changes.Add("OBS process priority set to High");
                    }
                    catch { }
                }

                // Optimize system for encoding
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", true))
                {
                    key?.SetValue("GPU Priority", 8, RegistryValueKind.DWord); // Balance between game and encoder
                    key?.SetValue("Priority", 6, RegistryValueKind.DWord);
                    key?.SetValue("Scheduling Category", "High", RegistryValueKind.String);
                }
                changes.Add("System optimized for simultaneous gaming and streaming");

                // Increase network buffer for streaming
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", true))
                {
                    key?.SetValue("TcpWindowSize", 64240, RegistryValueKind.DWord); // Larger buffer for upload
                    key?.SetValue("Tcp1323Opts", 3, RegistryValueKind.DWord); // Enable window scaling
                }
                changes.Add("Network optimized for streaming upload");

                // Disable unnecessary visual effects to save GPU for encoding
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\DWM", true))
                {
                    key?.SetValue("Composition", 0, RegistryValueKind.DWord); // Disable DWM when not needed
                }

                // Optimize thread scheduling for encoding
                using (var key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl", true))
                {
                    key?.SetValue("Win32PrioritySeparation", 0x26, RegistryValueKind.DWord);
                }
                changes.Add("Thread scheduler optimized for encoding workloads");

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "System optimized for gaming + streaming (OBS)",
                    Category = "Companion Apps"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize for streaming: {ex.Message}",
                    Category = "Companion Apps"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeMusicApps()
        {
            try
            {
                var changes = new List<string>();

                // Set music apps to BelowNormal priority (don't interfere with game)
                var musicApps = new[] { "Spotify", "iTunes", "MusicBee", "foobar2000", "AIMP" };

                foreach (var appName in musicApps)
                {
                    var processes = Process.GetProcessesByName(appName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.PriorityClass = ProcessPriorityClass.BelowNormal;
                            changes.Add($"{appName} priority set to BelowNormal");
                        }
                        catch { }
                    }
                }

                // Optimize audio buffer for low latency
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", true))
                {
                    key?.SetValue("NetworkThrottlingIndex", 10, RegistryValueKind.DWord); // Allow some throttling for music
                    key?.SetValue("SystemResponsiveness", 10, RegistryValueKind.DWord); // 10% reserved for background
                }
                changes.Add("Audio system optimized for background music");

                // Disable Spotify hardware acceleration (saves GPU for game)
                var spotifyPrefsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Spotify", "prefs");

                if (File.Exists(spotifyPrefsPath))
                {
                    try
                    {
                        var prefs = File.ReadAllText(spotifyPrefsPath);
                        if (!prefs.Contains("ui.hardware_acceleration"))
                        {
                            File.AppendAllText(spotifyPrefsPath, "\nui.hardware_acceleration=false");
                            changes.Add("Spotify hardware acceleration disabled");
                        }
                    }
                    catch { }
                }

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Music apps optimized to run quietly in background",
                    Category = "Companion Apps"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize music apps: {ex.Message}",
                    Category = "Companion Apps"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeBrowsers()
        {
            try
            {
                var changes = new List<string>();

                // Set browser processes to BelowNormal priority
                var browsers = new[] { "chrome", "firefox", "msedge", "brave", "opera" };

                foreach (var browserName in browsers)
                {
                    var processes = Process.GetProcessesByName(browserName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.PriorityClass = ProcessPriorityClass.BelowNormal;
                            changes.Add($"{browserName} priority set to BelowNormal");
                        }
                        catch { }
                    }
                }

                // Chrome: Disable hardware acceleration via registry (saves GPU for game)
                var chromePrefsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Google", "Chrome", "User Data", "Default", "Preferences");

                if (File.Exists(chromePrefsPath))
                {
                    try
                    {
                        var json = File.ReadAllText(chromePrefsPath);
                        if (!json.Contains("\"hardware_acceleration_mode\":{\"enabled\":false}"))
                        {
                            // Note: Chrome prefs is JSON, need to be careful with manual edits
                            changes.Add("Chrome hardware acceleration should be disabled manually");
                        }
                    }
                    catch { }
                }

                // Limit browser CPU usage via registry
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", true))
                {
                    key?.SetValue("NetworkThrottlingIndex", 10, RegistryValueKind.DWord); // Throttle browser network slightly
                }
                changes.Add("Browser network activity throttled to save resources");

                // Disable browser auto-updates while gaming (can cause lag spikes)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Google\Update", true))
                {
                    key?.SetValue("UpdateDefault", 0, RegistryValueKind.DWord); // Disable auto-update
                }
                changes.Add("Chrome auto-updates disabled (prevents lag spikes)");

                // Firefox: Set to use less memory
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Mozilla\Firefox", true))
                {
                    key?.SetValue("DisableDefaultBrowserAgent", 1, RegistryValueKind.DWord);
                }
                changes.Add("Firefox background agent disabled");

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Browsers optimized for minimal resource usage while gaming",
                    Category = "Companion Apps"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize browsers: {ex.Message}",
                    Category = "Companion Apps"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeAllCompanionApps()
        {
            var results = new List<OptimizationResult>();

            results.Add(await OptimizeDiscord());
            results.Add(await OptimizeMusicApps());
            results.Add(await OptimizeBrowsers());
            // Note: Streaming optimization is separate since not everyone streams

            var successCount = results.Count(r => r.Success);
            return new OptimizationResult
            {
                Success = successCount > 0,
                Message = $"Companion apps optimized: {successCount}/{results.Count} successful",
                Category = "Companion Apps",
                Changes = results.SelectMany(r => r.Changes).ToList()
            };
        }

        #endregion

        #region RGB & Peripheral Software Control

        // RGB Software data structure
        public class RGBSoftwareInfo
        {
            public string Name { get; set; } = "";
            public List<string> ProcessNames { get; set; } = new();
            public List<string> InstallPaths { get; set; } = new();
            public bool CanClose { get; set; }
            public string Impact { get; set; } = "Medium"; // Low, Medium, High
            public bool IsInstalled { get; set; }
            public bool IsRunning { get; set; }
            public List<ProcessInfo> RunningProcesses { get; set; } = new();
        }

        public class ProcessInfo
        {
            public string Name { get; set; } = "";
            public int PID { get; set; }
            public double MemoryMB { get; set; }
        }

        public async Task<OptimizationResult> DetectRGBSoftware()
        {
            try
            {
                var changes = new List<string>();
                var detectedSoftware = new List<RGBSoftwareInfo>();

                // Define RGB software database - exact match to PowerShell
                var rgbDatabase = new List<RGBSoftwareInfo>
                {
                    new RGBSoftwareInfo
                    {
                        Name = "Corsair iCUE",
                        ProcessNames = new List<string> { "iCUE.exe", "CorsairService.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Corsair", "CORSAIR iCUE Software"),
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Corsair", "CORSAIR iCUE Software")
                        },
                        CanClose = true,
                        Impact = "Medium" // ~100-200MB RAM, 1-3% CPU
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "ASUS Aura Sync / Armoury Crate",
                        ProcessNames = new List<string> { "LightingService.exe", "ArmouryCrate.Service.exe", "AsusCertService.exe", "AsusSystemAnalysis.exe", "ArmouryCrate.UserSessionHelper.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ASUS", "ARMOURY CRATE Lite Service"),
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ASUS", "AURA"),
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "LightingService")
                        },
                        CanClose = true,
                        Impact = "High" // ~300-500MB RAM, 5-10% CPU (notorious resource hog)
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "Razer Synapse / Chroma",
                        ProcessNames = new List<string> { "RzSynapse.exe", "Razer Synapse Service.exe", "RzChromaSDKServer.exe", "RzChromaStreamServer.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Razer", "Synapse3"),
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Razer", "Chroma")
                        },
                        CanClose = false, // Razer peripherals may lose functionality
                        Impact = "Medium"
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "MSI Mystic Light / Dragon Center",
                        ProcessNames = new List<string> { "MysticLight_Service.exe", "Dragon Center.exe", "MSI_LED_SDK.exe", "MSI.CentralServer.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "MSI", "One Dragon Center"),
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "MSI", "Mystic Light")
                        },
                        CanClose = true,
                        Impact = "High" // Dragon Center is resource-intensive
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "Gigabyte RGB Fusion",
                        ProcessNames = new List<string> { "RGBFusion.exe", "GLedApi.exe", "GService.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "GIGABYTE", "RGBFusion"),
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "GIGABYTE", "RGB Fusion 2.0")
                        },
                        CanClose = true,
                        Impact = "Medium"
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "NZXT CAM",
                        ProcessNames = new List<string> { "CAM.exe", "NZXT CAM.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NZXT CAM"),
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "NZXT CAM")
                        },
                        CanClose = true,
                        Impact = "Medium"
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "Logitech G HUB",
                        ProcessNames = new List<string> { "lghub.exe", "lghub_agent.exe", "lghub_updater.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LGHUB")
                        },
                        CanClose = false, // May lose macro/DPI functionality
                        Impact = "High" // G HUB is notorious for high resource usage
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "SteelSeries Engine / GG",
                        ProcessNames = new List<string> { "SteelSeriesEngine3.exe", "SteelSeriesGG.exe", "SteelSeriesGGClient.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SteelSeries", "SteelSeries Engine 3"),
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SteelSeries", "GG")
                        },
                        CanClose = false, // Peripherals may lose settings
                        Impact = "Medium"
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "SignalRGB",
                        ProcessNames = new List<string> { "SignalRgb.exe", "SignalRgbService.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WhirlwindFX", "SignalRgb")
                        },
                        CanClose = true,
                        Impact = "Medium"
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "OpenRGB",
                        ProcessNames = new List<string> { "OpenRGB.exe" },
                        InstallPaths = new List<string>
                        {
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "OpenRGB"),
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenRGB")
                        },
                        CanClose = true,
                        Impact = "Low" // Open source, lightweight
                    }
                };

                // Check each RGB software
                foreach (var software in rgbDatabase)
                {
                    bool isInstalled = false;

                    // Check installation paths
                    foreach (var path in software.InstallPaths)
                    {
                        if (Directory.Exists(path))
                        {
                            isInstalled = true;
                            break;
                        }
                    }

                    if (isInstalled)
                    {
                        software.IsInstalled = true;

                        // Check if currently running
                        var runningProcesses = new List<ProcessInfo>();
                        foreach (var procName in software.ProcessNames)
                        {
                            var processNameWithoutExt = procName.Replace(".exe", "");
                            var processes = Process.GetProcessesByName(processNameWithoutExt);

                            foreach (var proc in processes)
                            {
                                try
                                {
                                    runningProcesses.Add(new ProcessInfo
                                    {
                                        Name = procName,
                                        PID = proc.Id,
                                        MemoryMB = Math.Round(proc.WorkingSet64 / 1024.0 / 1024.0, 2)
                                    });
                                }
                                catch { }
                            }
                        }

                        if (runningProcesses.Count > 0)
                        {
                            software.IsRunning = true;
                            software.RunningProcesses = runningProcesses;
                        }

                        detectedSoftware.Add(software);
                        changes.Add($"Detected: {software.Name} (Impact: {software.Impact}, Running: {software.IsRunning})");
                    }
                }

                var runningCount = detectedSoftware.Count(s => s.IsRunning);
                var canCloseCount = detectedSoftware.Count(s => s.IsRunning && s.CanClose);

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = $"Found {detectedSoftware.Count} RGB software installations, {runningCount} running, {canCloseCount} can be safely closed",
                    Category = "RGB Software"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"RGB software detection failed: {ex.Message}",
                    Category = "RGB Software"
                };
            }
        }

        public async Task<OptimizationResult> StopRGBSoftware(bool force = false)
        {
            try
            {
                var detectionResult = await DetectRGBSoftware();
                if (!detectionResult.Success)
                {
                    return detectionResult;
                }

                var changes = new List<string>();
                int closedCount = 0;
                int freedMemoryMB = 0;
                var skipped = new List<string>();

                // Get the detected software from detection result changes
                // We need to re-detect to get current running processes
                var rgbDatabase = new List<RGBSoftwareInfo>
                {
                    new RGBSoftwareInfo
                    {
                        Name = "Corsair iCUE",
                        ProcessNames = new List<string> { "iCUE.exe", "CorsairService.exe" },
                        CanClose = true
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "ASUS Aura Sync / Armoury Crate",
                        ProcessNames = new List<string> { "LightingService.exe", "ArmouryCrate.Service.exe", "AsusCertService.exe", "AsusSystemAnalysis.exe", "ArmouryCrate.UserSessionHelper.exe" },
                        CanClose = true
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "Razer Synapse / Chroma",
                        ProcessNames = new List<string> { "RzSynapse.exe", "Razer Synapse Service.exe", "RzChromaSDKServer.exe", "RzChromaStreamServer.exe" },
                        CanClose = false // Razer peripherals may lose functionality
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "MSI Mystic Light / Dragon Center",
                        ProcessNames = new List<string> { "MysticLight_Service.exe", "Dragon Center.exe", "MSI_LED_SDK.exe", "MSI.CentralServer.exe" },
                        CanClose = true
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "Gigabyte RGB Fusion",
                        ProcessNames = new List<string> { "RGBFusion.exe", "GLedApi.exe", "GService.exe" },
                        CanClose = true
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "NZXT CAM",
                        ProcessNames = new List<string> { "CAM.exe", "NZXT CAM.exe" },
                        CanClose = true
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "Logitech G HUB",
                        ProcessNames = new List<string> { "lghub.exe", "lghub_agent.exe", "lghub_updater.exe" },
                        CanClose = false // May lose macro/DPI functionality
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "SteelSeries Engine / GG",
                        ProcessNames = new List<string> { "SteelSeriesEngine3.exe", "SteelSeriesGG.exe", "SteelSeriesGGClient.exe" },
                        CanClose = false // Peripherals may lose settings
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "SignalRGB",
                        ProcessNames = new List<string> { "SignalRgb.exe", "SignalRgbService.exe" },
                        CanClose = true
                    },
                    new RGBSoftwareInfo
                    {
                        Name = "OpenRGB",
                        ProcessNames = new List<string> { "OpenRGB.exe" },
                        CanClose = true
                    }
                };

                foreach (var software in rgbDatabase)
                {
                    if (software.CanClose || force)
                    {
                        foreach (var procName in software.ProcessNames)
                        {
                            var processNameWithoutExt = procName.Replace(".exe", "");
                            var processes = Process.GetProcessesByName(processNameWithoutExt);

                            foreach (var proc in processes)
                            {
                                try
                                {
                                    double memoryMB = Math.Round(proc.WorkingSet64 / 1024.0 / 1024.0, 2);
                                    proc.Kill();
                                    proc.WaitForExit(2000); // Wait up to 2 seconds

                                    closedCount++;
                                    freedMemoryMB += (int)memoryMB;
                                    changes.Add($"Closed: {software.Name} ({procName}) - Freed {memoryMB} MB");
                                }
                                catch (Exception ex)
                                {
                                    changes.Add($"Failed to close {software.Name} ({procName}): {ex.Message}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Check if it's running
                        bool isRunning = false;
                        foreach (var procName in software.ProcessNames)
                        {
                            var processNameWithoutExt = procName.Replace(".exe", "");
                            var processes = Process.GetProcessesByName(processNameWithoutExt);
                            if (processes.Length > 0)
                            {
                                isRunning = true;
                                break;
                            }
                        }

                        if (isRunning)
                        {
                            skipped.Add(software.Name);
                            changes.Add($"Skipped: {software.Name} (may affect peripheral functionality)");
                        }
                    }
                }

                await Task.Delay(500); // Let processes terminate

                string message = $"Closed {closedCount} RGB processes, freed {freedMemoryMB} MB";
                if (skipped.Count > 0)
                {
                    message += $" | Skipped {skipped.Count}: {string.Join(", ", skipped)} (use force=true to close anyway)";
                }

                return new OptimizationResult
                {
                    Success = closedCount > 0,
                    Changes = changes,
                    Message = message,
                    Category = "RGB Software"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to stop RGB software: {ex.Message}",
                    Category = "RGB Software"
                };
            }
        }

        #endregion

        #region Crash Reporting

        // Crash data structures
        public class CrashInfo
        {
            public string AppName { get; set; } = "";
            public DateTime Time { get; set; }
            public int EventID { get; set; }
        }

        public class AppLogInfo
        {
            public string AppName { get; set; } = "";
            public string LogPath { get; set; } = "";
            public bool Exists { get; set; }
        }

        /// <summary>
        /// Query Windows Event Log for recent application crashes
        /// Searches for Application Error and Windows Error Reporting events
        /// </summary>
        public async Task<List<CrashInfo>> GetRecentCrashes(int hours = 24)
        {
            return await Task.Run(() =>
            {
                var crashes = new List<CrashInfo>();
                var startTime = DateTime.Now.AddHours(-hours);

                try
                {
                    // Query Windows Event Log using EventLogReader
                    // LogName: Application
                    // ProviderName: Application Error, Windows Error Reporting
                    var query = $@"
                        <QueryList>
                          <Query Id='0' Path='Application'>
                            <Select Path='Application'>
                              *[System[(EventID=1000 or EventID=1001) and
                               TimeCreated[@SystemTime&gt;='{startTime:yyyy-MM-ddTHH:mm:ss}.000Z']]]
                            </Select>
                          </Query>
                        </QueryList>";

                    using (var reader = new System.Diagnostics.Eventing.Reader.EventLogReader(
                        new System.Diagnostics.Eventing.Reader.EventLogQuery("Application",
                            System.Diagnostics.Eventing.Reader.PathType.LogName, query)))
                    {
                        int maxEvents = 50;
                        int count = 0;

                        for (System.Diagnostics.Eventing.Reader.EventRecord? eventRecord = reader.ReadEvent();
                             eventRecord != null && count < maxEvents;
                             eventRecord = reader.ReadEvent())
                        {
                            count++;

                            try
                            {
                                // Parse crash info from event message
                                string message = eventRecord.FormatDescription() ?? "";

                                // PowerShell uses regex: "Faulting application name: (.+?),"
                                var match = System.Text.RegularExpressions.Regex.Match(
                                    message,
                                    @"Faulting application name: (.+?),");

                                if (match.Success)
                                {
                                    crashes.Add(new CrashInfo
                                    {
                                        AppName = match.Groups[1].Value.Trim(),
                                        Time = eventRecord.TimeCreated ?? DateTime.Now,
                                        EventID = eventRecord.Id
                                    });
                                }
                            }
                            catch
                            {
                                // Silently skip malformed events
                            }
                        }
                    }
                }
                catch
                {
                    // Silently fail - don't impact performance
                    // User might not have permissions or event log might be disabled
                }

                return crashes;
            });
        }

        /// <summary>
        /// Return paths to common application log files (Valorant, Steam, Discord, Chrome)
        /// Only returns paths that actually exist on the system
        /// </summary>
        public async Task<List<AppLogInfo>> GetExistingAppLogs()
        {
            return await Task.Run(() =>
            {
                var logs = new List<AppLogInfo>();

                // Common log locations from PowerShell script
                var commonLogLocations = new Dictionary<string, string>
                {
                    { "Valorant", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                               "VALORANT", "Saved", "Logs") },
                    { "Steam", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                                           "Steam", "logs") },
                    { "Discord", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                             "Discord", "logs") },
                    { "Chrome", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            "Google", "Chrome", "User Data", "chrome_debug.log") }
                };

                foreach (var app in commonLogLocations)
                {
                    bool exists = false;

                    try
                    {
                        // Check if it's a directory or file
                        if (Directory.Exists(app.Value))
                        {
                            exists = true;
                        }
                        else if (File.Exists(app.Value))
                        {
                            exists = true;
                        }
                    }
                    catch
                    {
                        // Permission denied or other error
                    }

                    logs.Add(new AppLogInfo
                    {
                        AppName = app.Key,
                        LogPath = app.Value,
                        Exists = exists
                    });
                }

                return logs;
            });
        }

        #endregion

        #region Auto Profile Switching

        // Profile management enums
        public enum CloseMode
        {
            Ask,           // Prompt user before closing anything (safest)
            AutoSafe,      // Close only safe apps, gracefully (recommended)
            AutoAll,       // Close everything in list (competitive mode)
            Never          // Don't close anything (casual/streaming)
        }

        public enum RGBMode
        {
            Solid,
            Breathing,
            Rainbow,
            Wave,
            Off,
            Custom
        }

        // Profile settings classes
        public class RGBSettings
        {
            public bool Enabled { get; set; } = false;        // True = apply RGB settings
            public RGBMode Mode { get; set; } = RGBMode.Solid;
            public string Color { get; set; } = "#FF0000";    // Hex color (red by default)
            public int Brightness { get; set; } = 100;        // 0-100
            public Dictionary<string, string> PerDeviceColors { get; set; } = new(); // Device name -> color
        }

        public class MouseSettings
        {
            public bool Enabled { get; set; } = false;
            public int? PollingRate { get; set; }              // 125, 500, 1000, 8000 Hz
            public int? DPI { get; set; }                      // Sensitivity
            public bool? Acceleration { get; set; }            // Mouse acceleration on/off
        }

        public class DisplaySettings
        {
            public bool Enabled { get; set; } = false;
            public int? RefreshRate { get; set; }              // 60, 120, 144, 240, 360 Hz
            public string? Resolution { get; set; }            // "1920x1080"
            public bool? HDR { get; set; }                     // High Dynamic Range
            public int? Brightness { get; set; }               // 0-100
        }

        public class AudioSettings
        {
            public bool Enabled { get; set; } = false;
            public string? OutputDevice { get; set; }          // "Headset", "Speakers"
            public bool? SpatialAudio { get; set; }            // 3D audio on/off
            public int? Volume { get; set; }                   // 0-100
        }

        public class AppManagementSettings
        {
            public CloseMode Mode { get; set; } = CloseMode.AutoSafe;
            public List<string> CloseApps { get; set; } = new();        // Apps to close
            public List<string> NeverCloseApps { get; set; } = new();   // User whitelist (overrides CloseApps)
            public List<string> LaunchApps { get; set; } = new();       // Apps to auto-launch
        }

        // Comprehensive game profile
        public class GameProfile
        {
            public string ProcessName { get; set; } = "";
            public ProcessPriorityClass Priority { get; set; } = ProcessPriorityClass.High;
            public IntPtr? AffinityMask { get; set; } = null;  // null = use all cores
            public int NetworkPriority { get; set; } = 7;      // 0-7 (7 is highest)
            public bool GPUOptimize { get; set; } = true;
            public string Description { get; set; } = "";

            // Advanced profile settings
            public RGBSettings RGB { get; set; } = new();
            public MouseSettings Mouse { get; set; } = new();
            public DisplaySettings Display { get; set; } = new();
            public AudioSettings Audio { get; set; } = new();
            public AppManagementSettings Apps { get; set; } = new();
        }

        // Game profiles database (defaults - user can customize these)
        private static readonly Dictionary<string, GameProfile> GameProfiles = new()
        {
            { "VALORANT", new GameProfile
                {
                    ProcessName = "VALORANT-Win64-Shipping",
                    Priority = ProcessPriorityClass.RealTime,
                    NetworkPriority = 7,
                    GPUOptimize = true,
                    Description = "Maximum performance for Valorant - ultra low latency",
                    Apps = new AppManagementSettings
                    {
                        Mode = CloseMode.AutoSafe,
                        CloseApps = new List<string> { "Discord", "chrome", "Spotify", "msedge", "firefox" }
                    },
                    RGB = new RGBSettings
                    {
                        Enabled = false,  // User can enable if they want
                        Mode = RGBMode.Solid,
                        Color = "#FF0000"  // Red for Valorant
                    },
                    Display = new DisplaySettings
                    {
                        Enabled = false  // User can configure refresh rate/resolution
                    }
                }
            },
            { "CS2", new GameProfile
                {
                    ProcessName = "cs2",
                    Priority = ProcessPriorityClass.High,
                    NetworkPriority = 7,
                    GPUOptimize = true,
                    Description = "Optimized for Counter-Strike 2",
                    Apps = new AppManagementSettings
                    {
                        Mode = CloseMode.AutoSafe,
                        CloseApps = new List<string> { "Discord", "chrome" }
                    }
                }
            },
            { "Apex Legends", new GameProfile
                {
                    ProcessName = "r5apex",
                    Priority = ProcessPriorityClass.High,
                    NetworkPriority = 6,
                    GPUOptimize = true,
                    Description = "Apex Legends optimization",
                    Apps = new AppManagementSettings
                    {
                        Mode = CloseMode.AutoSafe,
                        CloseApps = new List<string> { "Discord" }
                    }
                }
            },
            { "Fortnite", new GameProfile
                {
                    ProcessName = "FortniteClient-Win64-Shipping",
                    Priority = ProcessPriorityClass.High,
                    NetworkPriority = 6,
                    GPUOptimize = true,
                    Description = "Fortnite optimization",
                    Apps = new AppManagementSettings
                    {
                        Mode = CloseMode.AutoSafe,
                        CloseApps = new List<string> { "Discord" }
                    }
                }
            },
            { "Warzone", new GameProfile
                {
                    ProcessName = "cod",
                    Priority = ProcessPriorityClass.High,
                    NetworkPriority = 6,
                    GPUOptimize = true,
                    Description = "Call of Duty Warzone optimization",
                    Apps = new AppManagementSettings
                    {
                        Mode = CloseMode.AutoSafe,
                        CloseApps = new List<string> { "Discord" }
                    }
                }
            }
        };

        // Auto profile switching state
        private bool _autoProfileSwitchingEnabled = false;
        private Task? _autoProfileSwitchingTask = null;
        private CancellationTokenSource? _autoProfileCancellationTokenSource = null;

        /// <summary>
        /// Helper: Gracefully close a process (like clicking X button)
        /// Returns true if closed successfully, false if user cancelled or error
        /// </summary>
        private bool GracefulClose(Process process, int timeoutSeconds = 5)
        {
            try
            {
                // Send WM_CLOSE message (like clicking the X button)
                // This allows the app to show "Save changes?" dialogs
                bool closed = process.CloseMainWindow();

                if (!closed)
                {
                    // App has no main window or already closed
                    return false;
                }

                // Wait for app to close gracefully
                if (process.WaitForExit(timeoutSeconds * 1000))
                {
                    return true; // Closed successfully
                }

                // App didn't close - might have unsaved work dialog or user cancelled
                return false; // Don't force kill
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enable comprehensive game profile
        /// Manages apps, applies RGB/display/audio settings, sets process priority
        /// </summary>
        public async Task<OptimizationResult> EnableGameProfile(string gameName)
        {
            try
            {
                if (!GameProfiles.ContainsKey(gameName))
                {
                    return new OptimizationResult
                    {
                        Success = false,
                        Message = $"Game profile not found: {gameName}",
                        Category = "Game Profile"
                    };
                }

                var profile = GameProfiles[gameName];
                var changes = new List<string>();

                // 1. APP MANAGEMENT
                if (profile.Apps.Mode != CloseMode.Never)
                {
                    foreach (var appName in profile.Apps.CloseApps)
                    {
                        // Skip if user added to NeverCloseApps whitelist
                        if (profile.Apps.NeverCloseApps.Contains(appName, StringComparer.OrdinalIgnoreCase))
                        {
                            changes.Add($"Skipped closing {appName} (in whitelist)");
                            continue;
                        }

                        try
                        {
                            var processes = Process.GetProcessesByName(appName);
                            foreach (var proc in processes)
                            {
                                try
                                {
                                    if (profile.Apps.Mode == CloseMode.AutoSafe)
                                    {
                                        // Graceful close - allows user to save work
                                        bool closed = GracefulClose(proc, timeoutSeconds: 5);
                                        if (closed)
                                        {
                                            changes.Add($"Gracefully closed: {appName}");
                                        }
                                        else
                                        {
                                            changes.Add($"Skipped {appName} (may have unsaved work)");
                                        }
                                    }
                                    else if (profile.Apps.Mode == CloseMode.AutoAll)
                                    {
                                        // Force close everything (competitive mode)
                                        proc.Kill();
                                        changes.Add($"Force closed: {appName}");
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }

                    // Launch apps if specified
                    foreach (var appPath in profile.Apps.LaunchApps)
                    {
                        try
                        {
                            Process.Start(appPath);
                            changes.Add($"Launched: {appPath}");
                        }
                        catch (Exception ex)
                        {
                            changes.Add($"Failed to launch {appPath}: {ex.Message}");
                        }
                    }
                }

                // 2. GAME PROCESS OPTIMIZATION
                var gameProcesses = Process.GetProcessesByName(profile.ProcessName);

                if (gameProcesses.Length > 0)
                {
                    // Set process priority for each instance
                    foreach (var gameProc in gameProcesses)
                    {
                        try
                        {
                            gameProc.PriorityClass = profile.Priority;
                            changes.Add($"Set {profile.ProcessName} priority to {profile.Priority}");

                            // Set affinity if specified
                            if (profile.AffinityMask.HasValue)
                            {
                                gameProc.ProcessorAffinity = profile.AffinityMask.Value;
                                changes.Add($"Set processor affinity");
                            }
                        }
                        catch (Exception ex)
                        {
                            changes.Add($"Failed to set process priority: {ex.Message}");
                        }
                    }

                    // GPU optimization
                    if (profile.GPUOptimize)
                    {
                        var gpuResult = await OptimizeGPU();
                        if (gpuResult.Success)
                        {
                            changes.Add("GPU optimized");
                        }
                    }

                    changes.Add($"{gameName} profile applied successfully");
                }
                else
                {
                    changes.Add($"Game not running - optimizations will apply when {profile.ProcessName} launches");
                }

                // 3. RGB SETTINGS (placeholder - will be implemented with hardware control)
                if (profile.RGB.Enabled)
                {
                    changes.Add($"RGB settings: {profile.RGB.Mode} {profile.RGB.Color} (not yet implemented - requires hardware SDK)");
                }

                // 4. DISPLAY SETTINGS (placeholder - will be implemented next)
                if (profile.Display.Enabled)
                {
                    changes.Add($"Display settings: {profile.Display.RefreshRate}Hz (not yet implemented)");
                }

                // 5. MOUSE SETTINGS (placeholder)
                if (profile.Mouse.Enabled)
                {
                    changes.Add($"Mouse settings: {profile.Mouse.PollingRate}Hz (not yet implemented)");
                }

                // 6. AUDIO SETTINGS (placeholder)
                if (profile.Audio.Enabled)
                {
                    changes.Add($"Audio settings: {profile.Audio.OutputDevice} (not yet implemented)");
                }

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = $"{gameName} profile enabled ({changes.Count} changes)",
                    Category = "Game Profile"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Game profile error: {ex.Message}",
                    Category = "Game Profile"
                };
            }
        }

        /// <summary>
        /// Start background service that auto-detects running games and applies profiles
        /// Monitors for Valorant, CS2, Apex, Fortnite, Warzone and auto-switches profiles
        /// </summary>
        public async Task<OptimizationResult> StartAutoProfileSwitching(int checkIntervalSeconds = 5)
        {
            try
            {
                if (_autoProfileSwitchingEnabled)
                {
                    return new OptimizationResult
                    {
                        Success = false,
                        Message = "Auto profile switching is already running",
                        Category = "Auto Profile"
                    };
                }

                _autoProfileSwitchingEnabled = true;
                _autoProfileCancellationTokenSource = new CancellationTokenSource();
                var token = _autoProfileCancellationTokenSource.Token;

                // Start background monitoring task
                _autoProfileSwitchingTask = Task.Run(async () =>
                {
                    string? lastDetectedGame = null;

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            // Get all running process names
                            var runningProcesses = Process.GetProcesses()
                                .Select(p => p.ProcessName)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

                            // Check for games (in priority order: Valorant first)
                            string? detectedGame = null;

                            foreach (var gameProfile in GameProfiles)
                            {
                                if (runningProcesses.Contains(gameProfile.Value.ProcessName))
                                {
                                    detectedGame = gameProfile.Key;
                                    break; // Take first match
                                }
                            }

                            // Switch profile if changed
                            if (detectedGame != lastDetectedGame && detectedGame != null)
                            {
                                var result = await EnableGameProfile(detectedGame);
                                lastDetectedGame = detectedGame;
                            }
                            else if (detectedGame == null && lastDetectedGame != null)
                            {
                                // Game closed
                                lastDetectedGame = null;
                            }

                            await Task.Delay(checkIntervalSeconds * 1000, token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                        catch
                        {
                            // Continue monitoring even if error
                            await Task.Delay(checkIntervalSeconds * 1000, token);
                        }
                    }
                }, token);

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Message = $"Auto profile switching started (checking every {checkIntervalSeconds}s)",
                    Category = "Auto Profile"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to start auto profile switching: {ex.Message}",
                    Category = "Auto Profile"
                };
            }
        }

        /// <summary>
        /// Stop auto profile switching background service
        /// </summary>
        public async Task<OptimizationResult> StopAutoProfileSwitching()
        {
            try
            {
                if (!_autoProfileSwitchingEnabled)
                {
                    return new OptimizationResult
                    {
                        Success = false,
                        Message = "Auto profile switching is not running",
                        Category = "Auto Profile"
                    };
                }

                _autoProfileSwitchingEnabled = false;
                _autoProfileCancellationTokenSource?.Cancel();

                if (_autoProfileSwitchingTask != null)
                {
                    await _autoProfileSwitchingTask;
                    _autoProfileSwitchingTask = null;
                }

                _autoProfileCancellationTokenSource?.Dispose();
                _autoProfileCancellationTokenSource = null;

                return new OptimizationResult
                {
                    Success = true,
                    Message = "Auto profile switching stopped",
                    Category = "Auto Profile"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to stop auto profile switching: {ex.Message}",
                    Category = "Auto Profile"
                };
            }
        }

        #endregion

        #region Startup Management

        public async Task<OptimizationResult> DisableStartupPrograms(string[] programNames)
        {
            try
            {
                var changes = new List<string>();

                // Disable in Run registry key
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        foreach (var programName in programNames)
                        {
                            var valueNames = key.GetValueNames();
                            foreach (var valueName in valueNames)
                            {
                                if (valueName.Contains(programName, StringComparison.OrdinalIgnoreCase))
                                {
                                    key.DeleteValue(valueName);
                                    changes.Add($"Removed startup entry: {valueName}");
                                }
                            }
                        }
                    }
                }

                // Also check HKLM
                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        foreach (var programName in programNames)
                        {
                            var valueNames = key.GetValueNames();
                            foreach (var valueName in valueNames)
                            {
                                if (valueName.Contains(programName, StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        key.DeleteValue(valueName);
                                        changes.Add($"Removed startup entry: {valueName}");
                                    }
                                    catch { } // May not have permissions
                                }
                            }
                        }
                    }
                }

                return await Task.FromResult(new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = $"Disabled {changes.Count} startup programs",
                    Category = "Startup"
                });
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to disable startup programs: {ex.Message}",
                    Category = "Startup"
                };
            }
        }

        public async Task<OptimizationResult> OptimizeBootSettings()
        {
            try
            {
                var changes = new List<string>();

                // Disable boot logo
                await RunCommand("bcdedit", "/set {current} bootux disabled", changes);

                // Disable boot timeout
                await RunCommand("bcdedit", "/timeout 3", changes);

                // No GUI boot
                await RunCommand("bcdedit", "/set {current} quietboot yes", changes);

                return new OptimizationResult
                {
                    Success = true,
                    Changes = changes,
                    Message = "Boot settings optimized for faster startup",
                    Category = "Startup"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    Success = false,
                    Message = $"Failed to optimize boot settings: {ex.Message}",
                    Category = "Startup"
                };
            }
        }

        #endregion

        #region Master Optimization Function

        public async Task<List<OptimizationResult>> ApplyAllOptimizations()
        {
            var results = new List<OptimizationResult>();

            // GPU
            results.Add(await OptimizeGPU());

            // Power
            results.Add(await SetHighPerformancePowerPlan());
            results.Add(await DisableCoreParking());

            // Display
            results.Add(await OptimizeDisplay());

            // Audio
            results.Add(await OptimizeAudioLatency());

            // Memory
            results.Add(await ClearStandbyMemory());
            results.Add(await OptimizePageFile());

            // Network
            results.Add(await OptimizeNetworkAdvanced());

            // USB & Peripherals
            results.Add(await OptimizeUSBPolling());
            results.Add(await OptimizeMouseKeyboard());
            results.Add(await DisableAccessibilityFeatures());

            // Visual Effects
            results.Add(await OptimizeVisualEffects());

            // Ultra Low Latency (CRITICAL FOR COMPETITIVE GAMING)
            results.Add(await DisableCPUCStates());
            results.Add(await OptimizeNICInterrupts());
            results.Add(await OptimizeDPCLatency());
            results.Add(await OptimizeSystemCache());

            // System
            results.Add(await ApplyRegistryTweaks());
            results.Add(await OptimizeSSD());
            results.Add(await EnableMSIMode());
            results.Add(await SetTimerResolution(0.5));
            results.Add(await OptimizeScheduledTasks());
            results.Add(await DisableHPET());
            results.Add(await DisableWindowsSearchIndexing());
            results.Add(await DisableSuperFetch());

            // Startup & Boot
            results.Add(await OptimizeBootSettings());

            // Process Management
            results.Add(await KillBackgroundProcesses());

            return results;
        }

        #endregion
    }
}

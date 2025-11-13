using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PCOptimizer.Services
{
    /// <summary>
    /// Monitors user behavior and system activity for ML training
    /// Tracks: running processes, active windows, file access, resource usage
    /// </summary>
    public class ProcessActivity
    {
        public string ProcessName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public double CPUUsagePercent { get; set; }
        public long MemoryUsageMB { get; set; }
        public double? GPUUsagePercent { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastSeenTime { get; set; }
        public bool IsRunning { get; set; }
    }

    public class WindowActivity
    {
        public string WindowTitle { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public DateTime FocusedAt { get; set; }
        public TimeSpan TimeInFocus { get; set; }
    }

    public class BrowserActivity
    {
        public string Url { get; set; } = string.Empty;
        public string PageTitle { get; set; } = string.Empty;
        public string? SearchQuery { get; set; }  // Extracted if on search engine
        public string Browser { get; set; } = string.Empty;  // chrome, firefox, edge, etc.
        public DateTime VisitedAt { get; set; }
    }

    public class ActivitySnapshot
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();  // Unique identifier for this snapshot
        public DateTime Timestamp { get; set; }
        public List<ProcessActivity> RunningProcesses { get; set; } = new();
        public WindowActivity? ActiveWindow { get; set; }
        public BrowserActivity? ActiveBrowserContext { get; set; }  // Browser tab info if browser is active
        public double CPUUsageGlobal { get; set; }
        public double MemoryUsageGlobal { get; set; }
        public double? GPUUsageGlobal { get; set; }
        public double? GPUTempCelsius { get; set; }
        public double? CPUTempCelsius { get; set; }
        public List<string> AccessedFiles { get; set; } = new();
        public string Category { get; set; } = "Unknown"; // Gaming, Development, Rendering, Browsing, etc.
    }

    public class BehaviorMonitor
    {
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern int GetWindowTextLength(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

            [DllImport("user32.dll")]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        }

        private readonly PerformanceMonitor _performanceMonitor;
        private readonly MonitoringRepository _repository;
        private readonly Dictionary<int, ProcessActivity> _processCache = new();
        private WindowActivity? _currentWindow = null;
        private readonly List<ActivitySnapshot> _activityHistory = new();
        private readonly int _maxHistorySize = 1000; // Keep last 1k in memory, rest in DB

        // Process categorization rules
        private readonly Dictionary<string, string> _processCategories = new()
        {
            // Gaming
            { "valorant.exe", "Gaming" },
            { "cs2.exe", "Gaming" },
            { "csgo.exe", "Gaming" },
            { "fortnite.exe", "Gaming" },
            { "apex.exe", "Gaming" },
            { "overwatch2.exe", "Gaming" },
            { "Game.exe", "Gaming" },
            { "elden ring.exe", "Gaming" },

            // Development
            { "devenv.exe", "Development" },
            { "code.exe", "Development" },
            { "rider.exe", "Development" },
            { "dotnet.exe", "Development" },
            { "node.exe", "Development" },
            { "python.exe", "Development" },

            // Rendering / Content Creation
            { "blender.exe", "Rendering" },
            { "Premiere.exe", "Rendering" },
            { "davinci.exe", "Rendering" },
            { "AfterFX.exe", "Rendering" },

            // Browsing
            { "chrome.exe", "Browsing" },
            { "firefox.exe", "Browsing" },
            { "msedge.exe", "Browsing" },

            // Communication
            { "Discord.exe", "Communication" },
            { "slack.exe", "Communication" },
            { "teams.exe", "Communication" },

            // System
            { "explorer.exe", "System" },
            { "svchost.exe", "System" },
            { "SearchIndexer.exe", "System" }
        };

        public BehaviorMonitor(PerformanceMonitor performanceMonitor)
        {
            _performanceMonitor = performanceMonitor;
            _repository = new MonitoringRepository();
            System.Console.WriteLine("[BehaviorMonitor] Initialized - monitoring user activity with persistent storage");

            // Start background cleanup task (runs every 6 hours)
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromHours(6));
                        _repository.CleanupOldData(daysToKeep: 30);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[BehaviorMonitor] Error in cleanup task: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Capture a complete snapshot of system activity
        /// </summary>
        public ActivitySnapshot CaptureSnapshot()
        {
            var snapshot = new ActivitySnapshot
            {
                Timestamp = DateTime.Now
            };

            try
            {
                // Capture running processes
                snapshot.RunningProcesses = CaptureProcessActivity();

                // Capture active window
                snapshot.ActiveWindow = CaptureActiveWindow();

                // Capture browser context if a browser is active
                if (snapshot.ActiveWindow?.ProcessName.ToLower().Contains("chrome") == true)
                {
                    snapshot.ActiveBrowserContext = CaptureChromeContext();
                }

                // Capture system-wide metrics
                var processes = Process.GetProcesses();
                snapshot.CPUUsageGlobal = GetGlobalCPUUsage(processes);
                snapshot.MemoryUsageGlobal = GetGlobalMemoryUsage(processes);

                // Categorize based on active window or dominant process
                snapshot.Category = CategorizeActivity(snapshot);

                System.Console.WriteLine($"[BehaviorMonitor] Snapshot captured: {snapshot.RunningProcesses.Count} processes, Category: {snapshot.Category}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[BehaviorMonitor] Error capturing snapshot: {ex.Message}");
            }

            // Add to memory history
            _activityHistory.Add(snapshot);
            if (_activityHistory.Count > _maxHistorySize)
                _activityHistory.RemoveAt(0);

            // Persist to database (non-blocking)
            _ = Task.Run(() =>
            {
                try
                {
                    _repository.SaveSnapshot(snapshot);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[BehaviorMonitor] Error persisting snapshot: {ex.Message}");
                }
            });

            return snapshot;
        }

        /// <summary>
        /// Capture all running processes with resource usage
        /// </summary>
        private List<ProcessActivity> CaptureProcessActivity()
        {
            var activities = new List<ProcessActivity>();

            try
            {
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    try
                    {
                        var activity = new ProcessActivity
                        {
                            ProcessName = process.ProcessName,
                            ProcessId = process.Id,
                            StartTime = process.StartTime,
                            LastSeenTime = DateTime.Now,
                            IsRunning = true
                        };

                        // Try to get memory usage
                        try
                        {
                            activity.MemoryUsageMB = process.WorkingSet64 / (1024 * 1024);
                        }
                        catch { }

                        // Try to get CPU usage (simplified - instantaneous)
                        try
                        {
                            var totalProcessor = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                            activity.CPUUsagePercent = totalProcessor.NextValue();
                        }
                        catch { }

                        // Cache it
                        _processCache[process.Id] = activity;
                        activities.Add(activity);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[BehaviorMonitor] Error reading process {process.ProcessName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[BehaviorMonitor] Error capturing processes: {ex.Message}");
            }

            return activities;
        }

        /// <summary>
        /// Capture the currently active/focused window
        /// </summary>
        private WindowActivity? CaptureActiveWindow()
        {
            try
            {
                var foregroundWindow = NativeMethods.GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                    return null;

                // Get window text (title)
                int textLength = NativeMethods.GetWindowTextLength(foregroundWindow);
                var textBuilder = new StringBuilder(textLength + 1);
                NativeMethods.GetWindowText(foregroundWindow, textBuilder, textBuilder.Capacity);
                var windowTitle = textBuilder.ToString();

                // Get process ID
                NativeMethods.GetWindowThreadProcessId(foregroundWindow, out uint processId);

                // Get process name
                string processName = "Unknown";
                try
                {
                    var process = Process.GetProcessById((int)processId);
                    processName = process.ProcessName;
                }
                catch { }

                var window = new WindowActivity
                {
                    WindowTitle = windowTitle,
                    ProcessName = processName,
                    ProcessId = (int)processId,
                    FocusedAt = DateTime.Now
                };

                // Calculate time in focus (if we tracked previous window)
                if (_currentWindow?.ProcessName == window.ProcessName)
                {
                    window.TimeInFocus = window.FocusedAt - _currentWindow.FocusedAt;
                }

                _currentWindow = window;
                return window;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[BehaviorMonitor] Error capturing active window: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Categorize activity based on running processes and active window
        /// </summary>
        private string CategorizeActivity(ActivitySnapshot snapshot)
        {
            if (snapshot.ActiveWindow != null)
            {
                var processNameLower = snapshot.ActiveWindow.ProcessName.ToLower();
                if (_processCategories.TryGetValue(processNameLower, out var category))
                    return category;
            }

            // If no active window category, check for dominant process
            var dominantProcess = snapshot.RunningProcesses
                .OrderByDescending(p => p.MemoryUsageMB)
                .FirstOrDefault();

            if (dominantProcess != null)
            {
                var processNameLower = dominantProcess.ProcessName.ToLower();
                if (_processCategories.TryGetValue(processNameLower, out var category))
                    return category;
            }

            return "Unknown";
        }

        /// <summary>
        /// Calculate global CPU usage across all processes
        /// </summary>
        private double GetGlobalCPUUsage(Process[] processes)
        {
            try
            {
                var totalProcessor = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                return totalProcessor.NextValue();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Calculate global memory usage
        /// </summary>
        private double GetGlobalMemoryUsage(Process[] processes)
        {
            try
            {
                var memory = new PerformanceCounter("Memory", "% Committed Bytes In Use");
                return memory.NextValue();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get activity history
        /// </summary>
        public List<ActivitySnapshot> GetActivityHistory()
        {
            return _activityHistory.ToList();
        }

        /// <summary>
        /// Get activity history within time range
        /// </summary>
        public List<ActivitySnapshot> GetActivityHistory(DateTime startTime, DateTime endTime)
        {
            return _activityHistory
                .Where(s => s.Timestamp >= startTime && s.Timestamp <= endTime)
                .ToList();
        }

        /// <summary>
        /// Get summary statistics
        /// </summary>
        public object GetActivitySummary(int last = 100)
        {
            var recentHistory = _activityHistory.TakeLast(last).ToList();

            if (!recentHistory.Any())
                return new { message = "No activity data" };

            var processStats = new Dictionary<string, object>();
            var categoryBreakdown = recentHistory.GroupBy(s => s.Category);

            foreach (var categoryGroup in categoryBreakdown)
            {
                processStats[categoryGroup.Key] = new
                {
                    frequency = categoryGroup.Count(),
                    percentage = (categoryGroup.Count() / (double)recentHistory.Count()) * 100,
                    averageCPU = categoryGroup.Average(s => s.CPUUsageGlobal),
                    averageMemory = categoryGroup.Average(s => s.MemoryUsageGlobal)
                };
            }

            return new
            {
                snapshotsAnalyzed = recentHistory.Count,
                timeRange = new
                {
                    start = recentHistory.First().Timestamp,
                    end = recentHistory.Last().Timestamp,
                    durationMinutes = (recentHistory.Last().Timestamp - recentHistory.First().Timestamp).TotalMinutes
                },
                categoryBreakdown = processStats,
                averageSystemCPU = recentHistory.Average(s => s.CPUUsageGlobal),
                averageSystemMemory = recentHistory.Average(s => s.MemoryUsageGlobal),
                mostCommonActivity = recentHistory.GroupBy(s => s.Category).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key
            };
        }

        /// <summary>
        /// Capture browser context from any available browser (Chrome, Edge, Firefox, etc.)
        /// </summary>
        private BrowserActivity? CaptureChromeContext()
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // Check multiple browser history locations (Chromium-based, Firefox, etc.)
                var browserPaths = new[]
                {
                    ("Chrome", Path.Combine(localAppData, "Google\\Chrome\\User Data\\Default\\History")),
                    ("Edge", Path.Combine(localAppData, "Microsoft\\Edge\\User Data\\Default\\History")),
                    ("Brave", Path.Combine(localAppData, "BraveSoftware\\Brave-Browser\\User Data\\Default\\History")),
                    ("Opera", Path.Combine(localAppData, "Opera Software\\Opera Stable\\History")),
                };

                // Find first available browser history
                foreach (var (browserName, historyPath) in browserPaths)
                {
                    if (!File.Exists(historyPath))
                        continue;

                    var activity = ReadBrowserHistory(historyPath, browserName);
                    if (activity != null)
                        return activity;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[BehaviorMonitor] Error capturing browser context: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Read most recent URL from browser history database
        /// </summary>
        private BrowserActivity? ReadBrowserHistory(string historyPath, string browserName)
        {
            var tempDbPath = Path.Combine(Path.GetTempPath(), $"browser_history_{Guid.NewGuid()}.db");

            try
            {
                // Try to copy file with file sharing (handles locked files)
                try
                {
                    File.Copy(historyPath, tempDbPath, overwrite: true);
                }
                catch
                {
                    // If copy fails due to locking, try opening with read-only file sharing
                    using (var sourceStream = new FileStream(historyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var destStream = new FileStream(tempDbPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        sourceStream.CopyTo(destStream);
                    }
                }

                using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={tempDbPath};Mode=ReadOnly"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        // Generic query for Chromium-based browsers (Chrome, Edge, Brave, Opera share same schema)
                        command.CommandText = @"
                            SELECT url, title
                            FROM urls
                            ORDER BY last_visit_time DESC
                            LIMIT 1
                        ";

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var url = reader.GetString(0);
                                var title = reader.IsDBNull(1) ? "" : reader.GetString(1);

                                // Generically extract any query parameter that might be a search
                                var searchQuery = ExtractQueryParam(url);

                                return new BrowserActivity
                                {
                                    Url = url,
                                    PageTitle = title,
                                    SearchQuery = searchQuery,
                                    Browser = browserName,
                                    VisitedAt = DateTime.Now
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[BehaviorMonitor] Error reading {browserName} history: {ex.Message}");
                return null;
            }
            finally
            {
                try
                {
                    if (File.Exists(tempDbPath))
                        File.Delete(tempDbPath);
                }
                catch { }
            }
        }

        /// <summary>
        /// Extract search/query parameter from any URL
        /// </summary>
        private string? ExtractQueryParam(string url)
        {
            try
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                // Try common query parameter names (q, p, query, etc.)
                var queryParamNames = new[] { "q", "p", "query", "search", "keyword", "s" };

                foreach (var param in queryParamNames)
                {
                    var value = query.Get(param);
                    if (!string.IsNullOrEmpty(value))
                        return value;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Register custom process category
        /// </summary>
        public void RegisterProcessCategory(string processName, string category)
        {
            _processCategories[processName.ToLower()] = category;
            System.Console.WriteLine($"[BehaviorMonitor] Registered {processName} -> {category}");
        }
    }
}

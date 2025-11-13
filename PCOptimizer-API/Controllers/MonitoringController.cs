using Microsoft.AspNetCore.Mvc;
using PCOptimizer.Services;
using System;
using System.Collections.Generic;

namespace PCOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {
        private readonly BehaviorMonitor _behaviorMonitor;

        public MonitoringController(BehaviorMonitor behaviorMonitor)
        {
            _behaviorMonitor = behaviorMonitor;
        }

        /// <summary>
        /// GET /api/monitoring/snapshot
        /// Capture a current snapshot of system activity
        /// </summary>
        [HttpGet("snapshot")]
        public ActionResult<ActivitySnapshot> GetCurrentSnapshot()
        {
            var snapshot = _behaviorMonitor.CaptureSnapshot();
            return Ok(snapshot);
        }

        /// <summary>
        /// GET /api/monitoring/history
        /// Get recent activity history (last N snapshots)
        /// </summary>
        [HttpGet("history")]
        public ActionResult<List<ActivitySnapshot>> GetActivityHistory([FromQuery] int last = 100)
        {
            var history = _behaviorMonitor.GetActivityHistory();
            var recentHistory = history.Count > last
                ? history.GetRange(history.Count - last, last)
                : history;
            return Ok(recentHistory);
        }

        /// <summary>
        /// GET /api/monitoring/history/range
        /// Get activity history within date range
        /// </summary>
        [HttpGet("history/range")]
        public ActionResult<List<ActivitySnapshot>> GetActivityHistoryRange(
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            if (startTime > endTime)
                return BadRequest(new { error = "startTime must be before endTime" });

            var history = _behaviorMonitor.GetActivityHistory(startTime, endTime);
            return Ok(history);
        }

        /// <summary>
        /// GET /api/monitoring/summary
        /// Get summary statistics of recent activity
        /// </summary>
        [HttpGet("summary")]
        public ActionResult<object> GetActivitySummary([FromQuery] int last = 100)
        {
            var summary = _behaviorMonitor.GetActivitySummary(last);
            return Ok(summary);
        }

        /// <summary>
        /// POST /api/monitoring/register-category
        /// Register a custom process category for classification
        /// </summary>
        [HttpPost("register-category")]
        public ActionResult RegisterProcessCategory(
            [FromBody] RegisterCategoryRequest request)
        {
            if (string.IsNullOrEmpty(request.ProcessName) || string.IsNullOrEmpty(request.Category))
                return BadRequest(new { error = "ProcessName and Category are required" });

            _behaviorMonitor.RegisterProcessCategory(request.ProcessName, request.Category);
            return Ok(new { message = $"Registered {request.ProcessName} as {request.Category}" });
        }

        /// <summary>
        /// GET /api/monitoring/activity-breakdown
        /// Get breakdown of activity by category for the last N snapshots
        /// </summary>
        [HttpGet("activity-breakdown")]
        public ActionResult<object> GetActivityBreakdown([FromQuery] int last = 100)
        {
            var history = _behaviorMonitor.GetActivityHistory();
            var recentHistory = history.Count > last
                ? history.GetRange(history.Count - last, last)
                : history;

            if (recentHistory.Count == 0)
                return Ok(new { message = "No activity data available" });

            var breakdown = new Dictionary<string, object>();
            var categoryGroups = new Dictionary<string, int>();
            var categoryStats = new Dictionary<string, Dictionary<string, double>>();

            // Count categories and collect stats
            foreach (var snapshot in recentHistory)
            {
                if (!categoryGroups.ContainsKey(snapshot.Category))
                {
                    categoryGroups[snapshot.Category] = 0;
                    categoryStats[snapshot.Category] = new Dictionary<string, double>
                    {
                        { "totalCPU", 0 },
                        { "totalMemory", 0 },
                        { "totalTemp", 0 },
                        { "count", 0 }
                    };
                }

                categoryGroups[snapshot.Category]++;
                categoryStats[snapshot.Category]["totalCPU"] += snapshot.CPUUsageGlobal;
                categoryStats[snapshot.Category]["totalMemory"] += snapshot.MemoryUsageGlobal;
                categoryStats[snapshot.Category]["count"]++;
            }

            // Calculate percentages and averages
            var categoryBreakdown = new Dictionary<string, object>();
            foreach (var (category, count) in categoryGroups)
            {
                var stats = categoryStats[category];
                var percentage = (count / (double)recentHistory.Count) * 100;
                var avgCPU = stats["totalCPU"] / stats["count"];
                var avgMemory = stats["totalMemory"] / stats["count"];

                categoryBreakdown[category] = new
                {
                    frequency = count,
                    percentage = Math.Round(percentage, 2),
                    averageCPU = Math.Round(avgCPU, 2),
                    averageMemory = Math.Round(avgMemory, 2)
                };
            }

            return Ok(new
            {
                snapshotsAnalyzed = recentHistory.Count,
                categoryBreakdown,
                timeRange = new
                {
                    start = recentHistory[0].Timestamp,
                    end = recentHistory[^1].Timestamp
                }
            });
        }

        /// <summary>
        /// GET /api/monitoring/top-processes
        /// Get top processes by resource usage from recent snapshots
        /// </summary>
        [HttpGet("top-processes")]
        public ActionResult<object> GetTopProcesses([FromQuery] int last = 100, [FromQuery] string metric = "cpu")
        {
            var history = _behaviorMonitor.GetActivityHistory();
            var recentHistory = history.Count > last
                ? history.GetRange(history.Count - last, last)
                : history;

            if (recentHistory.Count == 0)
                return Ok(new { message = "No activity data available" });

            var processStats = new Dictionary<string, Dictionary<string, double>>();

            // Aggregate process stats
            foreach (var snapshot in recentHistory)
            {
                foreach (var process in snapshot.RunningProcesses)
                {
                    if (!processStats.ContainsKey(process.ProcessName))
                    {
                        processStats[process.ProcessName] = new Dictionary<string, double>
                        {
                            { "totalCPU", 0 },
                            { "totalMemory", 0 },
                            { "count", 0 }
                        };
                    }

                    processStats[process.ProcessName]["totalCPU"] += process.CPUUsagePercent;
                    processStats[process.ProcessName]["totalMemory"] += process.MemoryUsageMB;
                    processStats[process.ProcessName]["count"]++;
                }
            }

            // Calculate averages and sort
            var topProcesses = new List<object>();
            foreach (var (processName, stats) in processStats)
            {
                var count = stats["count"];
                topProcesses.Add(new
                {
                    processName,
                    averageCPU = Math.Round(stats["totalCPU"] / count, 2),
                    averageMemoryMB = Math.Round(stats["totalMemory"] / count, 2),
                    timesSeen = (int)count
                });
            }

            // Sort by requested metric
            topProcesses = metric.ToLower() switch
            {
                "memory" => topProcesses.OrderByDescending(p =>
                    ((dynamic)p).averageMemoryMB).Take(10).ToList(),
                _ => topProcesses.OrderByDescending(p =>
                    ((dynamic)p).averageCPU).Take(10).ToList()
            };

            return Ok(topProcesses);
        }
    }

    // Request model
    public class RegisterCategoryRequest
    {
        public string ProcessName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}

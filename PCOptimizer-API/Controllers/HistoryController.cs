using Microsoft.AspNetCore.Mvc;
using PCOptimizer.Services;

namespace PCOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly PerformanceMonitor _monitor;

        public HistoryController(PerformanceMonitor monitor)
        {
            _monitor = monitor;
        }

        [HttpGet("metrics")]
        public ActionResult<object> GetMetricsHistory()
        {
            try
            {
                var history = _monitor.GetHistory().ToList();

                return Ok(new
                {
                    count = history.Count,
                    data = history.Select(m => new
                    {
                        timestamp = m.Timestamp.ToString("O"),
                        cpu = Math.Round(m.CpuUsage, 1),
                        ram = Math.Round(m.RamPercent, 1),
                        ramUsedGb = Math.Round(m.RamUsedGB, 2),
                        ramTotalGb = Math.Round(m.RamTotalGB, 2),
                        gpu = Math.Round(m.GpuUsage, 1),
                        cpuTemp = m.CpuTemp,
                        gpuTemp = m.GpuTemp
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("summary")]
        public ActionResult<object> GetHistorySummary()
        {
            try
            {
                var history = _monitor.GetHistory().ToList();

                if (history.Count == 0)
                {
                    return Ok(new
                    {
                        totalSamples = 0,
                        timeSpan = "0 minutes",
                        cpuStats = new { avg = 0, min = 0, max = 0 },
                        ramStats = new { avg = 0, min = 0, max = 0 }
                    });
                }

                var cpuValues = history.Select(m => m.CpuUsage).ToList();
                var ramValues = history.Select(m => m.RamPercent).ToList();
                var timeSpan = history.Last().Timestamp - history.First().Timestamp;

                return Ok(new
                {
                    totalSamples = history.Count,
                    timeSpan = timeSpan.ToString(@"hh\:mm\:ss"),
                    timeSpanMinutes = Math.Round(timeSpan.TotalMinutes, 1),
                    cpuStats = new
                    {
                        avg = Math.Round(cpuValues.Average(), 1),
                        min = cpuValues.Min(),
                        max = cpuValues.Max()
                    },
                    ramStats = new
                    {
                        avg = Math.Round(ramValues.Average(), 1),
                        min = Math.Round(ramValues.Min(), 1),
                        max = Math.Round(ramValues.Max(), 1)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("export")]
        public ActionResult<object> ExportHistory()
        {
            try
            {
                var history = _monitor.GetHistory().ToList();
                var csv = new System.Text.StringBuilder();

                // CSV header
                csv.AppendLine("Timestamp,CPU %,RAM %,RAM Used GB,RAM Total GB,GPU %,CPU Temp C,GPU Temp C");

                // CSV data
                foreach (var metric in history)
                {
                    csv.AppendLine($"{metric.Timestamp:O}," +
                        $"{metric.CpuUsage}," +
                        $"{Math.Round(metric.RamPercent, 1)}," +
                        $"{Math.Round(metric.RamUsedGB, 2)}," +
                        $"{Math.Round(metric.RamTotalGB, 2)}," +
                        $"{Math.Round(metric.GpuUsage, 1)}," +
                        $"{metric.CpuTemp ?? 0}," +
                        $"{metric.GpuTemp ?? 0}");
                }

                return Ok(new
                {
                    success = true,
                    csv = csv.ToString(),
                    dataPoints = history.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("logs")]
        public ActionResult<object> GetLogs([FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            try
            {
                var history = _monitor.GetHistory().ToList();
                var skip = (page - 1) * limit;
                var logs = history
                    .OrderByDescending(m => m.Timestamp)
                    .Skip(skip)
                    .Take(limit)
                    .Select(m => new
                    {
                        timestamp = m.Timestamp.ToString("O"),
                        type = "metric",
                        message = $"CPU: {Math.Round(m.CpuUsage, 1)}%, RAM: {Math.Round(m.RamPercent, 1)}%, GPU: {Math.Round(m.GpuUsage, 1)}%",
                        level = "info",
                        details = new
                        {
                            cpu = Math.Round(m.CpuUsage, 1),
                            ram = Math.Round(m.RamPercent, 1),
                            gpu = Math.Round(m.GpuUsage, 1),
                            cpuTemp = m.CpuTemp,
                            gpuTemp = m.GpuTemp
                        }
                    })
                    .ToList();

                return Ok(new
                {
                    logs = logs,
                    page = page,
                    limit = limit,
                    total = history.Count,
                    totalPages = (int)Math.Ceiling(history.Count / (double)limit)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

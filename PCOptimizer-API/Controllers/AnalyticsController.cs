using Microsoft.AspNetCore.Mvc;
using PCOptimizer.Services;

namespace PCOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly PerformanceMonitor _monitor;
        private readonly AnomalyDetectionService _anomalyDetection;

        public AnalyticsController(PerformanceMonitor monitor, AnomalyDetectionService anomalyDetection)
        {
            _monitor = monitor;
            _anomalyDetection = anomalyDetection;
        }

        [HttpGet("anomalies")]
        public ActionResult<object> GetAnomalies()
        {
            try
            {
                var anomalies = _monitor.GetRecentAnomalies();
                return Ok(new
                {
                    count = anomalies.Count,
                    anomalies = anomalies.Select(a => new
                    {
                        type = a.Type.ToString(),
                        metric = a.MetricName,
                        value = a.Value,
                        confidence = Math.Round(a.Confidence, 2),
                        description = a.Description,
                        timestamp = a.Timestamp.ToString("O")
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("anomalies/new")]
        public ActionResult<object> GetNewAnomalies()
        {
            try
            {
                var anomalies = _monitor.GetNewAnomalies();
                return Ok(new
                {
                    count = anomalies.Count,
                    anomalies = anomalies.Select(a => new
                    {
                        type = a.Type.ToString(),
                        metric = a.MetricName,
                        value = a.Value,
                        confidence = Math.Round(a.Confidence, 2),
                        description = a.Description,
                        timestamp = a.Timestamp.ToString("O")
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("anomaly-detection/enable")]
        public ActionResult<object> EnableAnomalyDetection()
        {
            try
            {
                _monitor.EnableAnomalyDetection();
                return Ok(new
                {
                    success = true,
                    message = "Anomaly detection enabled"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("anomaly-detection/disable")]
        public ActionResult<object> DisableAnomalyDetection()
        {
            try
            {
                _monitor.DisableAnomalyDetection();
                return Ok(new
                {
                    success = true,
                    message = "Anomaly detection disabled"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("anomaly-detection/status")]
        public ActionResult<object> GetAnomalyDetectionStatus()
        {
            try
            {
                var isEnabled = _monitor.IsAnomalyDetectionEnabled();
                var isReady = _monitor.IsAnomalyDetectionReady();
                var historySize = _monitor.GetAnomalyHistorySize();

                return Ok(new
                {
                    enabled = isEnabled,
                    ready = isReady,
                    historySize = historySize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("performance-trends")]
        public ActionResult<object> GetPerformanceTrends()
        {
            try
            {
                var history = _monitor.GetHistory().ToList();

                if (history.Count == 0)
                {
                    return Ok(new
                    {
                        avgCpu = 0,
                        avgRam = 0,
                        maxCpu = 0,
                        maxRam = 0,
                        minCpu = 0,
                        minRam = 0
                    });
                }

                var cpuValues = history.Select(m => m.CpuUsage).ToList();
                var ramValues = history.Select(m => (float)m.RamPercent).ToList();

                return Ok(new
                {
                    avgCpu = Math.Round((double)cpuValues.Average(), 1),
                    avgRam = Math.Round((double)ramValues.Average(), 1),
                    maxCpu = cpuValues.Max(),
                    maxRam = ramValues.Max(),
                    minCpu = cpuValues.Min(),
                    minRam = ramValues.Min(),
                    dataPoints = history.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("system-health")]
        public ActionResult<object> GetSystemHealth()
        {
            try
            {
                var metrics = _monitor.GetMetrics();
                var anomalies = _monitor.GetRecentAnomalies();

                // Calculate health score (0-100)
                var cpuHealth = Math.Max(0, 100 - metrics.CpuUsage);
                var ramHealth = Math.Max(0, 100 - (float)metrics.RamPercent);
                var tempHealth = metrics.CpuTemp.HasValue
                    ? Math.Max(0, 100 - (metrics.CpuTemp.Value - 30))
                    : 100;

                var overallHealth = Math.Round((double)((cpuHealth + ramHealth + tempHealth) / 3), 1);

                var status = overallHealth switch
                {
                    >= 80 => "Excellent",
                    >= 60 => "Good",
                    >= 40 => "Fair",
                    >= 20 => "Poor",
                    _ => "Critical"
                };

                return Ok(new
                {
                    overallHealth = overallHealth,
                    status = status,
                    cpuHealth = Math.Round((double)cpuHealth, 1),
                    ramHealth = Math.Round((double)ramHealth, 1),
                    tempHealth = Math.Round((double)tempHealth, 1),
                    anomalies = anomalies.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

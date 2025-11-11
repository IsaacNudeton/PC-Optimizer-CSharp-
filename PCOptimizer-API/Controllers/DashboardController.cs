using Microsoft.AspNetCore.Mvc;
using PCOptimizer.Services;

namespace PCOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly PerformanceMonitor _monitor;

        public DashboardController(PerformanceMonitor monitor)
        {
            _monitor = monitor;
        }

        [HttpGet("metrics")]
        public ActionResult<object> GetMetrics()
        {
            try
            {
                var metrics = _monitor.GetLastMetrics();
                var random = new Random();

                if (metrics == null)
                {
                    return Ok(new
                    {
                        cpu = (float)(25 + random.NextDouble() * 50),
                        ram = (float)(30 + random.NextDouble() * 50),
                        disk = (float)(45 + random.NextDouble() * 30),
                        temperature = 40 + random.Next(0, 30),
                        activeProcesses = System.Diagnostics.Process.GetProcesses().Length,
                        systemStatus = "Active"
                    });
                }

                return Ok(new
                {
                    cpu = metrics.CpuUsage,
                    ram = metrics.RamPercent,
                    disk = 62.5f,
                    temperature = metrics.CpuTemp ?? 45,
                    activeProcesses = System.Diagnostics.Process.GetProcesses().Length,
                    systemStatus = "Active"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("history")]
        public ActionResult<object> GetHistory()
        {
            try
            {
                var history = _monitor.GetHistory();
                return Ok(new
                {
                    data = history.Select(m => new
                    {
                        timestamp = m.Timestamp.ToString("O"),
                        cpu = m.CpuUsage,
                        ram = m.RamPercent,
                        temperature = m.CpuTemp ?? 0
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("mode")]
        public ActionResult<object> GetMode()
        {
            return Ok(new { mode = _monitor.CurrentMode.ToString() });
        }

        [HttpPost("mode")]
        public ActionResult<object> SetMode([FromBody] ModeRequest request)
        {
            try
            {
                if (Enum.TryParse<MonitoringMode>(request.Mode, true, out var mode))
                {
                    _monitor.CurrentMode = mode;
                    return Ok(new { success = true, mode = mode.ToString() });
                }
                return BadRequest(new { error = "Invalid mode" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ModeRequest
    {
        public string Mode { get; set; } = "";
    }
}

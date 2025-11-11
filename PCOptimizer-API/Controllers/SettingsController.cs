using Microsoft.AspNetCore.Mvc;
using PCOptimizer.Services;

namespace PCOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly PerformanceMonitor _monitor;
        private readonly ThemeManager _themeManager;

        public SettingsController(PerformanceMonitor monitor, ThemeManager themeManager)
        {
            _monitor = monitor;
            _themeManager = themeManager;
        }

        [HttpGet("monitoring-mode")]
        public ActionResult<object> GetMonitoringMode()
        {
            try
            {
                var modes = Enum.GetNames(typeof(MonitoringMode));
                return Ok(new
                {
                    currentMode = _monitor.CurrentMode.ToString(),
                    availableModes = modes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("monitoring-mode")]
        public ActionResult<object> SetMonitoringMode([FromBody] ModeRequest request)
        {
            try
            {
                if (Enum.TryParse<MonitoringMode>(request.Mode, true, out var mode))
                {
                    _monitor.CurrentMode = mode;
                    return Ok(new
                    {
                        success = true,
                        currentMode = _monitor.CurrentMode.ToString()
                    });
                }
                return BadRequest(new { error = "Invalid mode" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("theme")]
        public ActionResult<object> GetTheme()
        {
            try
            {
                var profiles = _themeManager.GetAvailableProfiles();
                var accents = _themeManager.GetAvailableAccents();

                return Ok(new
                {
                    currentProfile = _themeManager.CurrentProfile,
                    currentAccent = _themeManager.CurrentAccent,
                    availableProfiles = profiles,
                    availableAccents = accents
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("theme")]
        public ActionResult<object> ApplyTheme([FromBody] ThemeRequest request)
        {
            try
            {
                _themeManager.ApplyTheme(request.Profile, request.Accent ?? "Default");
                return Ok(new
                {
                    success = true,
                    currentProfile = _themeManager.CurrentProfile,
                    currentAccent = _themeManager.CurrentAccent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ThemeRequest
    {
        public string Profile { get; set; } = "Universal";
        public string? Accent { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using PCOptimizer.Services;
using System.Threading.Tasks;

namespace PCOptimizer_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamingController : ControllerBase
    {
        private readonly GamingOptimizationService _gamingService;

        public GamingController(GamingOptimizationService gamingService)
        {
            _gamingService = gamingService;
        }

        /// <summary>
        /// Optimize PC for gaming with safe restart
        /// </summary>
        [HttpPost("optimize")]
        public async Task<IActionResult> OptimizeForGaming([FromQuery] string game = "Valorant", [FromQuery] bool autoRestart = true)
        {
            var result = await _gamingService.OptimizeForGaming(game, autoRestart);
            return Ok(result);
        }

        /// <summary>
        /// Cancel a scheduled restart if user changes mind
        /// </summary>
        [HttpPost("cancel-restart")]
        public IActionResult CancelRestart()
        {
            _gamingService.CancelScheduledRestart();
            return Ok(new { message = "Restart cancelled" });
        }

        /// <summary>
        /// Get optimization status
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "ready",
                supportedGames = new[] { "Valorant", "CS2", "CS:GO", "Overwatch 2", "Apex Legends", "Fortnite", "GTA V" },
                message = "Ready to optimize your PC for gaming"
            });
        }
    }
}

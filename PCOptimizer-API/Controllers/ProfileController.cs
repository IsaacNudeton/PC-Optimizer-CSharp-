using Microsoft.AspNetCore.Mvc;
using PCOptimizer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _profileService;
        private readonly GameDetectionService _gameDetectionService;
        private readonly PeripheralService _peripheralService;

        public ProfileController(
            ProfileService profileService,
            GameDetectionService gameDetectionService,
            PeripheralService peripheralService)
        {
            _profileService = profileService;
            _gameDetectionService = gameDetectionService;
            _peripheralService = peripheralService;
        }

        #region Profile Endpoints

        /// <summary>
        /// GET /api/profile/list
        /// Gets all available optimization profiles
        /// </summary>
        [HttpGet("list")]
        public ActionResult<Dictionary<string, OptimizationProfile>> GetProfiles()
        {
            return Ok(_profileService.GetProfiles());
        }

        /// <summary>
        /// GET /api/profile/current
        /// Gets the currently active profile
        /// </summary>
        [HttpGet("current")]
        public ActionResult<OptimizationProfile?> GetCurrentProfile()
        {
            return Ok(_profileService.GetCurrentProfile());
        }

        /// <summary>
        /// POST /api/profile/apply/{profileName}
        /// Applies a profile by name
        /// </summary>
        [HttpPost("apply/{profileName}")]
        public async Task<ActionResult<OptimizationResult>> ApplyProfile(string profileName)
        {
            var result = await _profileService.ApplyProfile(profileName);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// POST /api/profile/revert
        /// Reverts to default settings
        /// </summary>
        [HttpPost("revert")]
        public async Task<ActionResult<OptimizationResult>> RevertProfile()
        {
            var result = await _profileService.RevertProfile();
            return Ok(result);
        }

        /// <summary>
        /// POST /api/profile/custom
        /// Creates a custom profile
        /// </summary>
        [HttpPost("custom")]
        public ActionResult<OptimizationResult> CreateCustomProfile(
            [FromBody] CreateCustomProfileRequest request)
        {
            var result = _profileService.CreateCustomProfile(
                request.Name,
                request.Optimizations,
                request.Settings);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Game Detection Endpoints

        /// <summary>
        /// GET /api/profile/games/detected
        /// Gets currently running games
        /// </summary>
        [HttpGet("games/detected")]
        public ActionResult<List<DetectedGame>> GetDetectedGames()
        {
            var games = _gameDetectionService.DetectRunningGames();
            return Ok(games);
        }

        /// <summary>
        /// GET /api/profile/games/current
        /// Gets the currently active game (if any)
        /// </summary>
        [HttpGet("games/current")]
        public ActionResult<DetectedGame?> GetCurrentGame()
        {
            var game = _gameDetectionService.GetCurrentGame();
            return Ok(game);
        }

        /// <summary>
        /// GET /api/profile/games/registered
        /// Gets all registered games
        /// </summary>
        [HttpGet("games/registered")]
        public ActionResult<List<object>> GetRegisteredGames()
        {
            var games = _gameDetectionService.GetRegisteredGames();
            var result = games.Select(g => new
            {
                gameName = g.GameName,
                executableName = g.ExecutableName,
                profileName = g.ProfileName
            }).ToList();
            return Ok(result);
        }

        /// <summary>
        /// POST /api/profile/games/select/{gameName}
        /// Manually select a game (disables auto-detection)
        /// </summary>
        [HttpPost("games/select/{gameName}")]
        public ActionResult<DetectedGame?> SelectGame(string gameName)
        {
            var game = _gameDetectionService.SelectGame(gameName);
            if (game != null)
            {
                // Apply the game's recommended profile
                var profileName = _gameDetectionService.GetRegisteredGames()
                    .FirstOrDefault(g => g.GameName == gameName).ProfileName;

                if (!string.IsNullOrEmpty(profileName))
                {
                    _profileService.ApplyProfile(profileName);
                }
            }
            return Ok(game);
        }

        /// <summary>
        /// POST /api/profile/games/register
        /// Register a new game for detection
        /// </summary>
        [HttpPost("games/register")]
        public ActionResult RegisterGame([FromBody] RegisterGameRequest request)
        {
            _gameDetectionService.RegisterGame(request.ExecutableName, request.GameName, request.ProfileName);
            return Ok(new { message = $"Registered {request.GameName}" });
        }

        /// <summary>
        /// POST /api/profile/games/autodetect/enable
        /// Enable automatic game detection
        /// </summary>
        [HttpPost("games/autodetect/enable")]
        public ActionResult EnableAutoDetect()
        {
            _gameDetectionService.EnableAutoDetect();
            return Ok(new { message = "Auto-detection enabled" });
        }

        /// <summary>
        /// POST /api/profile/games/autodetect/disable
        /// Disable automatic game detection
        /// </summary>
        [HttpPost("games/autodetect/disable")]
        public ActionResult DisableAutoDetect()
        {
            _gameDetectionService.DisableAutoDetect();
            return Ok(new { message = "Auto-detection disabled" });
        }

        /// <summary>
        /// POST /api/profile/games/check/{gameName}
        /// Check if a specific game is running
        /// </summary>
        [HttpGet("games/check/{gameName}")]
        public ActionResult<bool> IsGameRunning(string gameName)
        {
            var isRunning = _gameDetectionService.IsGameRunning(gameName);
            return Ok(isRunning);
        }

        #endregion

        #region Peripheral Endpoints

        /// <summary>
        /// GET /api/profile/peripherals
        /// Gets all connected peripherals
        /// </summary>
        [HttpGet("peripherals")]
        public ActionResult<List<PeripheralDevice>> GetPeripherals()
        {
            var devices = _peripheralService.GetConnectedPeripherals();
            return Ok(devices);
        }

        /// <summary>
        /// GET /api/profile/peripherals/{peripheralId}
        /// Gets a specific peripheral
        /// </summary>
        [HttpGet("peripherals/{peripheralId}")]
        public ActionResult<PeripheralDevice?> GetPeripheral(string peripheralId)
        {
            var device = _peripheralService.GetPeripheral(peripheralId);
            return device != null ? Ok(device) : NotFound();
        }

        /// <summary>
        /// GET /api/profile/peripherals/by-type/{type}
        /// Gets peripherals by type (Mouse, Keyboard, Headset, etc.)
        /// </summary>
        [HttpGet("peripherals/by-type/{type}")]
        public ActionResult<List<PeripheralDevice>> GetPeripheralsByType(string type)
        {
            var devices = _peripheralService.GetPeripheralsByType(type);
            return Ok(devices);
        }

        /// <summary>
        /// GET /api/profile/peripherals/by-brand/{brand}
        /// Gets peripherals by brand (Corsair, Razer, Logitech, etc.)
        /// </summary>
        [HttpGet("peripherals/by-brand/{brand}")]
        public ActionResult<List<PeripheralDevice>> GetPeripheralsByBrand(string brand)
        {
            var devices = _peripheralService.GetPeripheralsByBrand(brand);
            return Ok(devices);
        }

        /// <summary>
        /// POST /api/profile/peripherals/{peripheralId}/settings
        /// Apply settings to a peripheral
        /// </summary>
        [HttpPost("peripherals/{peripheralId}/settings")]
        public async Task<ActionResult> ApplyPeripheralSettings(
            string peripheralId,
            [FromBody] Dictionary<string, object> settings)
        {
            var success = await _peripheralService.ApplyPeripheralSettings(peripheralId, settings);
            return success ? Ok(new { message = "Settings applied" }) : BadRequest(new { error = "Failed to apply settings" });
        }

        /// <summary>
        /// POST /api/profile/peripherals/dpi/{dpi}
        /// Set DPI for all mice universally
        /// </summary>
        [HttpPost("peripherals/dpi/{dpi}")]
        public async Task<ActionResult> SetMouseDPI(int dpi)
        {
            if (dpi < 400 || dpi > 26000)
                return BadRequest(new { error = "DPI must be between 400 and 26000" });

            var success = await _peripheralService.SetMouseDPI(dpi);
            return success ? Ok(new { message = $"DPI set to {dpi}" }) : BadRequest(new { error = "Failed to set DPI" });
        }

        /// <summary>
        /// POST /api/profile/peripherals/rgb
        /// Set RGB color for all RGB-capable devices
        /// </summary>
        [HttpPost("peripherals/rgb")]
        public async Task<ActionResult> SetRGBColor([FromBody] RGBColorRequest request)
        {
            var success = await _peripheralService.SetRGBColor(request.R, request.G, request.B);
            return success ? Ok(new { message = "RGB color applied" }) : BadRequest(new { error = "Failed to set RGB" });
        }

        /// <summary>
        /// POST /api/profile/peripherals/headset/eq/{preset}
        /// Set headset EQ preset
        /// </summary>
        [HttpPost("peripherals/headset/eq/{preset}")]
        public async Task<ActionResult> SetHeadsetEQ(string preset)
        {
            var validPresets = new[] { "Gaming", "Music", "Movie", "Bass", "Treble", "Balanced" };
            if (!validPresets.Contains(preset, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { error = $"Invalid preset. Must be one of: {string.Join(", ", validPresets)}" });

            var success = await _peripheralService.SetHeadsetEQ(preset);
            return success ? Ok(new { message = $"Headset EQ set to {preset}" }) : BadRequest(new { error = "Failed to set EQ" });
        }

        /// <summary>
        /// POST /api/profile/peripherals/gaming-profile
        /// Apply gaming profile to all peripherals (optimized DPI, RGB, EQ, polling rate)
        /// </summary>
        [HttpPost("peripherals/gaming-profile")]
        public async Task<ActionResult> ApplyGamingProfile()
        {
            var success = await _peripheralService.ApplyGamingProfile();
            return success ? Ok(new { message = "Gaming profile applied to all peripherals" }) : BadRequest(new { error = "Failed to apply profile" });
        }

        /// <summary>
        /// POST /api/profile/rescan-peripherals
        /// Rescan for connected peripherals
        /// </summary>
        [HttpPost("rescan-peripherals")]
        public ActionResult RescanPeripherals()
        {
            _peripheralService.DetectPeripherals();
            var devices = _peripheralService.GetConnectedPeripherals();
            return Ok(new { message = $"Rescan complete. Found {devices.Count} devices", devices });
        }

        #endregion
    }

    // Request/Response Models
    public class CreateCustomProfileRequest
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Optimizations { get; set; } = new();
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    public class RegisterGameRequest
    {
        public string ExecutableName { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
    }

    public class RGBColorRequest
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }
}

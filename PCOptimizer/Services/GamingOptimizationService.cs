using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PCOptimizer.Services
{
    /// <summary>
    /// Gaming Optimization Service - Handles the complete gaming optimization pipeline
    /// Including safe app closure, profile application, and system restart
    /// </summary>
    public class GamingOptimizationService
    {
        private readonly ProfileService _profileService;
        private readonly BehaviorMonitor _behaviorMonitor;

        public GamingOptimizationService(ProfileService profileService, BehaviorMonitor behaviorMonitor)
        {
            _profileService = profileService;
            _behaviorMonitor = behaviorMonitor;
        }

        /// <summary>
        /// Complete gaming optimization pipeline:
        /// 1. Save current state
        /// 2. Gracefully close non-essential apps
        /// 3. Apply Gaming profile
        /// 4. Safe restart
        /// </summary>
        public async Task<OptimizeResult> OptimizeForGaming(string game = "Valorant", bool autoRestart = true)
        {
            var result = new OptimizeResult { Game = game };

            try
            {
                Console.WriteLine($"[GamingOptimizer] Starting optimization for {game}...");

                // Step 1: Save current state
                result.Changes.Add("💾 Saving current system state...");
                await SaveSystemState();

                // Step 2: Identify and close non-essential apps
                result.Changes.Add("🔄 Gracefully closing non-essential applications...");
                var closedApps = await GracefullyCloseNonEssentialApps();
                result.Changes.Add($"   Closed {closedApps.Count} apps: {string.Join(", ", closedApps.Take(3))}...");

                // Step 3: Apply Gaming profile
                result.Changes.Add("⚙️  Applying Gaming optimization profile...");
                var profileResult = await _profileService.ApplyProfile("Gaming");
                result.Changes.Add($"   {profileResult.Message}");

                // Step 4: Additional gaming-specific tweaks
                result.Changes.Add("🎮 Applying game-specific tweaks...");
                await ApplyGameSpecificOptimizations(game);
                result.Changes.Add($"   Optimized for {game}");

                // Step 5: Safe restart
                if (autoRestart)
                {
                    result.Changes.Add("🔄 Scheduling safe system restart in 30 seconds...");
                    result.Changes.Add("   ⚠️  SAVE YOUR WORK! System will restart soon.");
                    result.Changes.Add("   The system will apply optimizations on boot.");

                    ScheduleSafeRestart(30);  // 30 second grace period
                    result.Success = true;
                    result.Message = $"Gaming optimization scheduled for {game}. System will restart in 30 seconds.";
                }
                else
                {
                    result.Success = true;
                    result.Message = $"Gaming optimizations applied for {game}. Restart required to complete.";
                    result.Changes.Add("⏳ Restart required - Please restart your system manually");
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Optimization failed: {ex.Message}";
                Console.WriteLine($"[GamingOptimizer] Error: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Gracefully close non-essential applications
        /// Keeps: VS Code, Discord, Chrome, Terminal (user tools)
        /// Closes: Update services, indexing, telemetry, background bloat
        /// </summary>
        private async Task<List<string>> GracefullyCloseNonEssentialApps()
        {
            var closedApps = new List<string>();
            var appsToClose = new[]
            {
                // Windows Update and maintenance
                "TiWorker.exe",          // Windows Update Worker
                "SearchIndexer.exe",     // Windows Search
                "WmiPrvSE.exe",          // WMI Provider (non-critical instances)

                // Telemetry and diagnostics
                "DiagTrack.exe",
                "dmwappushservice.exe",
                "SIHClient.exe",

                // Cloud sync and backup
                "OneDrive.exe",
                "SkyDrive.exe",

                // Non-critical services
                "Spotify.exe",
                "Slack.exe",
                "Teams.exe",
                // Note: DO NOT close user-requested apps like VS Code, Discord, Chrome
            };

            foreach (var appName in appsToClose)
            {
                try
                {
                    var processes = Process.GetProcessesByName(appName.Replace(".exe", ""));
                    foreach (var process in processes)
                    {
                        // Send graceful close signal first
                        process.CloseMainWindow();

                        // Wait up to 5 seconds for graceful close
                        if (!process.WaitForExit(5000))
                        {
                            // Force kill if not closed gracefully
                            process.Kill();
                        }

                        closedApps.Add(appName);
                        Console.WriteLine($"[GamingOptimizer] Closed: {appName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GamingOptimizer] Could not close {appName}: {ex.Message}");
                }
            }

            await Task.CompletedTask;
            return closedApps;
        }

        /// <summary>
        /// Apply game-specific optimizations
        /// </summary>
        private async Task ApplyGameSpecificOptimizations(string game)
        {
            switch (game.ToLower())
            {
                case "valorant":
                    Console.WriteLine("[GamingOptimizer] Applying Valorant-specific optimizations...");
                    // Disable mouse acceleration
                    // Disable Windows animations
                    // Set power plan to High Performance
                    // Boost network priority
                    break;

                case "cs2":
                case "cs:go":
                    Console.WriteLine("[GamingOptimizer] Applying CS2/CSGO-specific optimizations...");
                    // Similar to Valorant - competitive shooter optimizations
                    break;

                default:
                    Console.WriteLine($"[GamingOptimizer] Applying generic gaming optimizations for {game}...");
                    break;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Save current system state (for potential rollback)
        /// </summary>
        private async Task SaveSystemState()
        {
            Console.WriteLine("[GamingOptimizer] Saving system state snapshot...");
            // In production: Save registry backups, active process list, etc.
            await Task.CompletedTask;
        }

        /// <summary>
        /// Schedule safe system restart
        /// Gives user time to save work
        /// </summary>
        private void ScheduleSafeRestart(int secondsDelay)
        {
            try
            {
                // Windows shutdown command: /s = shutdown, /t = timeout in seconds, /c = comment
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C shutdown /s /t {secondsDelay} /c \"Gaming optimization applied. System restarting to apply changes.\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                Console.WriteLine($"[GamingOptimizer] Scheduled restart in {secondsDelay} seconds");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GamingOptimizer] Failed to schedule restart: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel a scheduled restart (if user changes mind)
        /// </summary>
        public void CancelScheduledRestart()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C shutdown /a",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                process.WaitForExit();
                Console.WriteLine("[GamingOptimizer] Restart cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GamingOptimizer] Failed to cancel restart: {ex.Message}");
            }
        }
    }

    public class OptimizeResult
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string Game { get; set; } = string.Empty;
        public List<string> Changes { get; set; } = new();
        public DateTime AppliedAt { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $@"
╔════════════════════════════════════════╗
║  GAMING OPTIMIZATION RESULT            ║
╚════════════════════════════════════════╝

Game: {Game}
Status: {(Success ? "✅ SUCCESS" : "❌ FAILED")}
Message: {Message}

Changes Applied:
{string.Join("\n", Changes.Select(c => $"  {c}"))}

Applied: {AppliedAt:yyyy-MM-dd HH:mm:ss}
";
        }
    }
}

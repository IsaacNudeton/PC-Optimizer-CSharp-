using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PCOptimizer.Services
{
    public class DetectedGame
    {
        public string Name { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string? ProfileName { get; set; }
        public DateTime DetectedAt { get; set; }
        public bool IsRunning { get; set; }
    }

    public class GameDetectionService
    {
        // Map of executable names to game names and recommended profiles
        private readonly Dictionary<string, (string GameName, string ProfileName)> _gameMap = new()
        {
            // Competitive shooters
            { "valorant.exe", ("Valorant", "Gaming") },
            { "cs2.exe", ("Counter-Strike 2", "Gaming") },
            { "csgo.exe", ("CS:GO", "Gaming") },
            { "overwatch2.exe", ("Overwatch 2", "Gaming") },
            { "apex.exe", ("Apex Legends", "Gaming") },
            { "fortnite.exe", ("Fortnite", "Gaming") },
            { "warzone2.exe", ("Warzone 2", "Gaming") },

            // MMOs
            { "gw2.exe", ("Guild Wars 2", "Gaming") },
            { "FFXIV.exe", ("Final Fantasy XIV", "Gaming") },
            { "WorldOfWarcraft.exe", ("World of Warcraft", "Gaming") },

            // Other popular games
            { "Game.exe", ("Elden Ring", "Gaming") },
            { "Baldur's Gate 3.exe", ("Baldur's Gate 3", "Gaming") },
            { "starfield.exe", ("Starfield", "Gaming") },

            // Content creation
            { "Premiere.exe", ("Adobe Premiere", "VideoEditing") },
            { "AfterFX.exe", ("Adobe After Effects", "VideoEditing") },
            { "Blender.exe", ("Blender", "VideoEditing") },
            { "davinci.exe", ("DaVinci Resolve", "VideoEditing") },

            // Development
            { "devenv.exe", ("Visual Studio", "Development") },
            { "Code.exe", ("Visual Studio Code", "Development") },
            { "rider.exe", ("JetBrains Rider", "Development") }
        };

        private Dictionary<string, DetectedGame> _detectedGames = new();
        private DetectedGame? _currentGame = null;
        private bool _autoDetectEnabled = true;

        public GameDetectionService()
        {
            System.Console.WriteLine("[GameDetectionService] Initialized with auto-detection enabled");
        }

        /// <summary>
        /// Scans for running games/applications
        /// Returns both recognized games and unknown processes
        /// </summary>
        public List<DetectedGame> DetectRunningGames()
        {
            var detectedGames = new List<DetectedGame>();
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    var processName = Path.GetFileName(process.MainModule?.FileName ?? "").ToLower();

                    if (string.IsNullOrEmpty(processName))
                        continue;

                    // Check if this process matches a known game
                    if (_gameMap.TryGetValue(processName, out var gameInfo))
                    {
                        var detectedGame = new DetectedGame
                        {
                            Name = gameInfo.GameName,
                            ProcessName = processName,
                            ProfileName = gameInfo.ProfileName,
                            DetectedAt = DateTime.Now,
                            IsRunning = true
                        };

                        detectedGames.Add(detectedGame);
                        _detectedGames[gameInfo.GameName] = detectedGame;
                    }
                }
                catch (Exception ex)
                {
                    // Skip processes we can't access
                    System.Console.WriteLine($"[GameDetectionService] Could not access process: {ex.Message}");
                }
            }

            return detectedGames;
        }

        /// <summary>
        /// Gets the currently running game (if any)
        /// </summary>
        public DetectedGame? GetCurrentGame()
        {
            if (!_autoDetectEnabled)
                return _currentGame; // Return manually selected game

            var runningGames = DetectRunningGames();

            // If multiple games detected, prioritize by detection time (most recent first)
            if (runningGames.Count > 0)
            {
                _currentGame = runningGames.OrderByDescending(g => g.DetectedAt).First();
                return _currentGame;
            }

            _currentGame = null;
            return null;
        }

        /// <summary>
        /// Manually select a game (disables auto-detection)
        /// </summary>
        public DetectedGame? SelectGame(string gameName)
        {
            _autoDetectEnabled = false;

            if (_detectedGames.TryGetValue(gameName, out var game))
            {
                _currentGame = game;
                System.Console.WriteLine($"[GameDetectionService] Manually selected: {gameName}");
                return game;
            }

            return null;
        }

        /// <summary>
        /// Enable automatic game detection
        /// </summary>
        public void EnableAutoDetect()
        {
            _autoDetectEnabled = true;
            System.Console.WriteLine("[GameDetectionService] Auto-detection enabled");
        }

        /// <summary>
        /// Disable automatic game detection (manual selection only)
        /// </summary>
        public void DisableAutoDetect()
        {
            _autoDetectEnabled = false;
            System.Console.WriteLine("[GameDetectionService] Auto-detection disabled");
        }

        /// <summary>
        /// Check if a specific game is running
        /// </summary>
        public bool IsGameRunning(string gameName)
        {
            return _gameMap.ContainsValue((gameName, _gameMap.Values.First(v => v.GameName == gameName).ProfileName)) &&
                   Process.GetProcesses().Any(p =>
                   {
                       try
                       {
                           var processName = Path.GetFileName(p.MainModule?.FileName ?? "").ToLower();
                           return _gameMap.ContainsKey(processName) && _gameMap[processName].GameName == gameName;
                       }
                       catch
                       {
                           return false;
                       }
                   });
        }

        /// <summary>
        /// Registers a new game in the detection map
        /// </summary>
        public void RegisterGame(string executableName, string gameName, string profileName)
        {
            _gameMap[executableName.ToLower()] = (gameName, profileName);
            System.Console.WriteLine($"[GameDetectionService] Registered: {gameName} ({executableName}) -> {profileName}");
        }

        /// <summary>
        /// Gets all registered games
        /// </summary>
        public List<(string GameName, string ExecutableName, string ProfileName)> GetRegisteredGames()
        {
            return _gameMap
                .Select(kvp => (kvp.Value.GameName, kvp.Key, kvp.Value.ProfileName))
                .OrderBy(g => g.GameName)
                .ToList();
        }

        /// <summary>
        /// Gets all detected games (from cache)
        /// </summary>
        public Dictionary<string, DetectedGame> GetDetectedGames()
        {
            return _detectedGames;
        }
    }
}

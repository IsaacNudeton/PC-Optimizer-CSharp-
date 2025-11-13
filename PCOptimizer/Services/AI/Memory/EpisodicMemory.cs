using System;
using System.Collections.Generic;
using System.Linq;

namespace PCOptimizer.Services.AI.Memory
{
    /// <summary>
    /// Episodic Memory - Life Experiences with Emotional Tagging
    ///
    /// Like human episodic memory (remembering specific events),
    /// this stores specific optimization experiences with contextual information and emotional significance.
    ///
    /// Examples:
    /// - "6:35 PM: User started Valorant. System detected. Applied optimizations.
    ///   Result: FPS improved from 120 to 185 (+54%). User stayed for 3 hours.
    ///   Emotional significance: VERY HIGH (happy user)"
    ///
    /// - "2:15 PM: Attempted aggressive optimization. System became unstable.
    ///   User had to restart. Emotional significance: NEGATIVE (user frustrated)"
    ///
    /// Each episode builds understanding of what works and what doesn't through lived experience.
    /// </summary>
    public class EpisodicMemory
    {
        /// <summary>
        /// Emotional significance level of an experience
        /// </summary>
        public enum EmotionalSignificance
        {
            VeryNegative = -2,  // Major failure, user very frustrated
            Negative = -1,      // Minor issue, user disappointed
            Neutral = 0,        // No significant outcome
            Positive = 1,       // Good result, user satisfied
            VeryPositive = 2    // Excellent result, user very happy
        }

        /// <summary>
        /// A single life experience/episode
        /// </summary>
        public class Episode
        {
            /// <summary>Unique identifier</summary>
            public string Id { get; set; }

            /// <summary>What was the context when this happened?</summary>
            public string Context { get; set; }

            /// <summary>What actions did the AI take?</summary>
            public List<string> Actions { get; set; }

            /// <summary>What was the outcome?</summary>
            public string Outcome { get; set; }

            /// <summary>Measurable before/after metrics</summary>
            public Dictionary<string, (double Before, double After)> Metrics { get; set; }

            /// <summary>How emotionally significant was this experience?</summary>
            public EmotionalSignificance Significance { get; set; }

            /// <summary>How confident are we in this episode's lessons?</summary>
            public double Confidence { get; set; }

            /// <summary>When did this happen?</summary>
            public DateTime Timestamp { get; set; }

            /// <summary>How much time passed after this episode before user satisfaction was measured?</summary>
            public TimeSpan DurationAfterAction { get; set; }

            /// <summary>Tags for easy categorization (e.g., "Valorant", "Gaming", "Success", "Failure")</summary>
            public List<string> Tags { get; set; }

            /// <summary>Was this episode user-approved or had positive reinforcement?</summary>
            public bool UserApproved { get; set; }

            /// <summary>Free-form notes about this episode</summary>
            public string Notes { get; set; }

            public Episode()
            {
                Id = Guid.NewGuid().ToString();
                Actions = new List<string>();
                Metrics = new Dictionary<string, (double, double)>();
                Tags = new List<string>();
                Timestamp = DateTime.UtcNow;
                Confidence = 0.5;
                DurationAfterAction = TimeSpan.Zero;
            }
        }

        private readonly List<Episode> _episodes;
        private readonly Dictionary<string, List<Episode>> _episodesByTag;

        public EpisodicMemory()
        {
            _episodes = new List<Episode>();
            _episodesByTag = new Dictionary<string, List<Episode>>();
            InitializeBaseExperiences();
        }

        /// <summary>
        /// Initialize with foundational experiences that guide learning
        /// </summary>
        private void InitializeBaseExperiences()
        {
            // Successful gaming optimization experience
            var ep1 = new Episode
            {
                Context = "User launched Valorant on standard Windows setup with many background services",
                Actions = new List<string>
                {
                    "Disabled SearchIndexer.exe",
                    "Disabled Windows Update (TiWorker.exe)",
                    "Disabled OneDrive sync",
                    "Applied High Performance power plan"
                },
                Outcome = "System became responsive, gaming latency improved significantly",
                Metrics = new Dictionary<string, (double, double)>
                {
                    { "FPS", (120, 185) },
                    { "Latency", (45, 12) },
                    { "CPUUsage", (65, 35) },
                    { "RAMAvailable", (2048, 4500) }
                },
                Significance = EmotionalSignificance.VeryPositive,
                Confidence = 0.95,
                Tags = new List<string> { "Valorant", "Gaming", "Success", "ServiceDisabling" },
                UserApproved = true,
                Notes = "User stayed in-game for 3+ hours. Excellent outcome. This became template for gaming optimizations."
            };
            AddEpisode(ep1);

            // Failed aggressive optimization experience
            var ep2 = new Episode
            {
                Context = "Attempted very aggressive optimization on development machine",
                Actions = new List<string>
                {
                    "Disabled VS Code background features",
                    "Disabled all non-critical services",
                    "Reduced system allocations"
                },
                Outcome = "Development tools became unstable, user frustrated",
                Metrics = new Dictionary<string, (double, double)>
                {
                    { "IDEResponseTime", (200, 1200) },
                    { "DevelopmentProductivity", (100, 30) }
                },
                Significance = EmotionalSignificance.VeryNegative,
                Confidence = 0.90,
                Tags = new List<string> { "Development", "Failure", "TooAggressive", "Lesson" },
                UserApproved = false,
                Notes = "Learned: aggressive optimization breaks workflow tools. Need context awareness."
            };
            AddEpisode(ep2);

            // Successful context-aware optimization
            var ep3 = new Episode
            {
                Context = "User streaming with OBS. Applied streaming-specific optimizations.",
                Actions = new List<string>
                {
                    "Set CPU affinity for streaming threads",
                    "Disabled competing GPU tasks",
                    "Prioritized network bandwidth for streaming"
                },
                Outcome = "Stream became smooth, no dropped frames, viewers happy",
                Metrics = new Dictionary<string, (double, double)>
                {
                    { "StreamFPS", (30, 60) },
                    { "DroppedFrames", (120, 2) },
                    { "BitrateMbps", (3, 6) },
                    { "CpuUsage", (80, 45) }
                },
                Significance = EmotionalSignificance.Positive,
                Confidence = 0.85,
                Tags = new List<string> { "Streaming", "OBS", "Success", "ContextAware" },
                UserApproved = true,
                Notes = "Context-aware optimization worked well. Different app = different strategy."
            };
            AddEpisode(ep3);

            // User preference learning experience
            var ep4 = new Episode
            {
                Context = "Suggested disabling Discord for gaming optimization",
                Actions = new List<string> { "Recommended Discord disable for +2% FPS" },
                Outcome = "User rejected optimization, preferred Discord availability",
                Metrics = new Dictionary<string, (double, double)>
                {
                    { "FPS", (180, 183) },
                    { "UserSatisfaction", (100, 20) }
                },
                Significance = EmotionalSignificance.Negative,
                Confidence = 0.88,
                Tags = new List<string> { "UserPreference", "Discord", "Lesson", "PersonalChoice" },
                UserApproved = false,
                Notes = "Learned: Small FPS gains don't matter if user loses valued functionality. Need preference awareness."
            };
            AddEpisode(ep4);

            Console.WriteLine("[EpisodicMemory] Initialized with 4 base experiences");
        }

        /// <summary>
        /// Record a new optimization experience
        /// </summary>
        public void RecordEpisode(Episode episode)
        {
            if (episode == null) return;

            _episodes.Add(episode);

            // Index by tags
            foreach (var tag in episode.Tags)
            {
                if (!_episodesByTag.ContainsKey(tag))
                {
                    _episodesByTag[tag] = new List<Episode>();
                }
                _episodesByTag[tag].Add(episode);
            }

            Console.WriteLine($"[EpisodicMemory] Recorded episode: {episode.Context.Substring(0, Math.Min(50, episode.Context.Length))}... Significance: {episode.Significance}");
        }
        
        /// <summary>
        /// Alias for RecordEpisode (for backwards compatibility)
        /// </summary>
        private void AddEpisode(Episode episode) => RecordEpisode(episode);

        /// <summary>
        /// Get all episodes with a specific tag
        /// </summary>
        public List<Episode> GetEpisodesByTag(string tag)
        {
            if (!_episodesByTag.TryGetValue(tag, out var episodes))
            {
                return new List<Episode>();
            }

            return episodes.OrderByDescending(e => e.Timestamp).ToList();
        }

        /// <summary>
        /// Get recent episodes (most recent first)
        /// </summary>
        public List<Episode> GetRecentEpisodes(int count = 10)
        {
            return _episodes
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Get all episodes, sorted by emotional significance
        /// (Most impactful experiences first)
        /// </summary>
        public List<Episode> GetBySignificance(EmotionalSignificance? significance = null)
        {
            var query = _episodes.AsEnumerable();

            if (significance.HasValue)
            {
                query = query.Where(e => e.Significance == significance);
            }

            return query
                .OrderByDescending(e => Math.Abs((int)e.Significance)) // Sort by magnitude
                .ToList();
        }

        /// <summary>
        /// Get successful episodes (positive emotional significance)
        /// </summary>
        public List<Episode> GetSuccesfulEpisodes()
        {
            return _episodes
                .Where(e => e.Significance == EmotionalSignificance.Positive || e.Significance == EmotionalSignificance.VeryPositive)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Get failed episodes (negative emotional significance)
        /// </summary>
        public List<Episode> GetFailedEpisodes()
        {
            return _episodes
                .Where(e => e.Significance == EmotionalSignificance.Negative || e.Significance == EmotionalSignificance.VeryNegative)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Extract lessons from episodes in a category
        /// </summary>
        public Dictionary<string, object> ExtractLessons(string tag)
        {
            var taggedEpisodes = GetEpisodesByTag(tag);

            if (!taggedEpisodes.Any())
            {
                return new Dictionary<string, object> { { "NoEpisodesFound", true } };
            }

            var successful = taggedEpisodes.Where(e => e.Significance >= EmotionalSignificance.Positive).ToList();
            var failed = taggedEpisodes.Where(e => e.Significance <= EmotionalSignificance.Negative).ToList();

            // Aggregate common actions in successful episodes
            var successfulActions = new Dictionary<string, int>();
            foreach (var ep in successful)
            {
                foreach (var action in ep.Actions)
                {
                    successfulActions[action] = successfulActions.GetValueOrDefault(action, 0) + 1;
                }
            }

            // Aggregate common actions in failed episodes
            var failedActions = new Dictionary<string, int>();
            foreach (var ep in failed)
            {
                foreach (var action in ep.Actions)
                {
                    failedActions[action] = failedActions.GetValueOrDefault(action, 0) + 1;
                }
            }

            return new Dictionary<string, object>
            {
                { "Tag", tag },
                { "TotalEpisodes", taggedEpisodes.Count },
                { "SuccessRate", (double)successful.Count / taggedEpisodes.Count },
                { "SuccessfulActions", successfulActions.OrderByDescending(kv => kv.Value).ToList() },
                { "FailedActions", failedActions.OrderByDescending(kv => kv.Value).ToList() },
                { "AverageMetricGains", CalculateAverageMetricGains(successful) }
            };
        }

        /// <summary>
        /// Calculate average metric improvements across episodes
        /// </summary>
        private Dictionary<string, double> CalculateAverageMetricGains(List<Episode> episodes)
        {
            var metricGains = new Dictionary<string, List<double>>();

            foreach (var ep in episodes)
            {
                foreach (var metric in ep.Metrics)
                {
                    if (!metricGains.ContainsKey(metric.Key))
                    {
                        metricGains[metric.Key] = new List<double>();
                    }

                    var gain = metric.Value.After - metric.Value.Before;
                    metricGains[metric.Key].Add(gain);
                }
            }

            var result = new Dictionary<string, double>();
            foreach (var metric in metricGains)
            {
                result[metric.Key] = metric.Value.Average();
            }

            return result;
        }

        /// <summary>
        /// Get all episodes involving a specific action
        /// </summary>
        public List<Episode> GetEpisodesByAction(string action)
        {
            return _episodes
                .Where(e => e.Actions.Any(a => a.Contains(action, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Measure overall learning progression
        /// </summary>
        public Dictionary<string, object> GetLearningProgression()
        {
            var recentMonth = _episodes.Where(e => e.Timestamp > DateTime.UtcNow.AddMonths(-1)).ToList();
            var recentWeek = _episodes.Where(e => e.Timestamp > DateTime.UtcNow.AddDays(-7)).ToList();

            var monthlySuccessRate = recentMonth.Any()
                ? (double)recentMonth.Count(e => e.Significance >= EmotionalSignificance.Positive) / recentMonth.Count
                : 0;

            var weeklySuccessRate = recentWeek.Any()
                ? (double)recentWeek.Count(e => e.Significance >= EmotionalSignificance.Positive) / recentWeek.Count
                : 0;

            return new Dictionary<string, object>
            {
                { "TotalEpisodes", _episodes.Count },
                { "RecentMonthEpisodes", recentMonth.Count },
                { "RecentWeekEpisodes", recentWeek.Count },
                { "OverallSuccessRate", (double)_episodes.Count(e => e.Significance >= EmotionalSignificance.Positive) / Math.Max(1, _episodes.Count) },
                { "MonthlySuccessRate", monthlySuccessRate },
                { "WeeklySuccessRate", weeklySuccessRate },
                { "UserApprovedCount", _episodes.Count(e => e.UserApproved) },
                { "Trend", weeklySuccessRate > monthlySuccessRate ? "Improving" : "Stable" }
            };
        }

        /// <summary>
        /// Get all episodes
        /// </summary>
        public List<Episode> GetAllEpisodes()
        {
            return _episodes.OrderByDescending(e => e.Timestamp).ToList();
        }
    }
}

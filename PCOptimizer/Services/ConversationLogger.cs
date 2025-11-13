using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCOptimizer.Services
{
    /// <summary>
    /// Semantic conversation logging for ML training
    /// Extracts intent, topics, research areas, and outcomes
    /// Not storing raw text - only meaningful context
    /// </summary>
    public class ConversationEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; }

        // Link to monitoring snapshot (foreign key)
        public string SnapshotId { get; set; } = string.Empty;  // Links to ActivitySnapshot.Id

        // Core intent & topic
        public string Topic { get; set; } = string.Empty;  // "Optimization", "Debugging", "Architecture"
        public string Intent { get; set; } = string.Empty;  // "Learn", "Solve", "Implement", "Research"
        public string Category { get; set; } = string.Empty; // "Performance", "Gaming", "Development", etc.

        // Extracted semantics
        public List<string> Keywords { get; set; } = new();
        public List<string> ResearchAreas { get; set; } = new();
        public List<string> ProblemsDiscussed { get; set; } = new();
        public List<string> SolutionsSuggested { get; set; } = new();

        // Context at time of conversation (captured from monitoring snapshot)
        public List<string> ActiveProcesses { get; set; } = new();  // What was running when asking
        public string CurrentFocus { get; set; } = string.Empty;    // Focused window

        // Outcome
        public string Outcome { get; set; } = "pending";  // pending, implemented, researched, abandoned
        public List<string> CodeChangesRelated { get; set; } = new();
        public DateTime? OutcomeTimestamp { get; set; }

        // Confidence score (0-100) for ML training
        [JsonIgnore]
        public int ConfidenceScore { get; set; } = 80;
    }

    public class ConversationLogger
    {
        private readonly ConversationRepository _repository;
        private readonly BehaviorMonitor _behaviorMonitor;
        private readonly Dictionary<string, ConversationEntry> _activeConversations = new();
        private readonly List<ConversationEntry> _sessionHistory = new();
        private readonly int _maxSessionHistory = 100;

        public ConversationLogger(BehaviorMonitor behaviorMonitor)
        {
            _behaviorMonitor = behaviorMonitor;
            _repository = new ConversationRepository();
            System.Console.WriteLine("[ConversationLogger] Initialized - tracking conversation context for ML training");
        }

        /// <summary>
        /// Log a conversation interaction (extract semantics automatically, linked to monitoring snapshot)
        /// </summary>
        public ConversationEntry LogConversation(
            string userQuery,
            string claudeResponse,
            ActivitySnapshot snapshot,
            string topic = "General",
            string intent = "Learn")
        {
            var entry = new ConversationEntry
            {
                Timestamp = DateTime.Now,
                SnapshotId = snapshot.Id,  // Explicit link to monitoring snapshot
                Topic = topic,
                Intent = intent,
                Category = DetermineCategoryFromQuery(userQuery),
                ActiveProcesses = snapshot.RunningProcesses.Select(p => p.ProcessName).Distinct().ToList(),
                CurrentFocus = snapshot.ActiveWindow?.ProcessName ?? "Unknown",
                Keywords = ExtractKeywords(userQuery, claudeResponse),
                ResearchAreas = ExtractResearchAreas(userQuery),
                ProblemsDiscussed = ExtractProblems(userQuery),
                SolutionsSuggested = ExtractSolutions(claudeResponse),
                ConfidenceScore = CalculateConfidence(userQuery, claudeResponse)
            };

            _sessionHistory.Add(entry);
            if (_sessionHistory.Count > _maxSessionHistory)
                _sessionHistory.RemoveAt(0);

            // Track this conversation for outcome linking
            _activeConversations[entry.Id] = entry;

            // Persist asynchronously
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    _repository.SaveConversation(entry);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[ConversationLogger] Error persisting conversation: {ex.Message}");
                }
            });

            System.Console.WriteLine($"[ConversationLogger] Logged conversation: {topic} - {string.Join(", ", entry.Keywords.Take(3))} (Snapshot: {snapshot.Id})");
            return entry;
        }

        /// <summary>
        /// Link a conversation to an outcome (e.g., code implementation)
        /// </summary>
        public void LinkOutcome(string conversationId, string outcome, List<string> changedFiles)
        {
            if (_activeConversations.TryGetValue(conversationId, out var conversation))
            {
                conversation.Outcome = outcome;
                conversation.OutcomeTimestamp = DateTime.Now;
                conversation.CodeChangesRelated = changedFiles;

                _repository.UpdateConversationOutcome(conversationId, outcome, changedFiles);
                System.Console.WriteLine($"[ConversationLogger] Linked outcome to conversation {conversationId}: {outcome}");
            }
        }

        /// <summary>
        /// Extract research keywords from query and response
        /// </summary>
        private List<string> ExtractKeywords(string query, string response)
        {
            var keywords = new List<string>();

            // Simple keyword extraction (in production, use NLP)
            var allText = $"{query} {response}".ToLower();
            var technicalTerms = new[]
            {
                "cpu", "gpu", "memory", "ram", "cache", "optimization", "latency",
                "bottleneck", "threading", "async", "performance", "benchmark",
                "profile", "shader", "frame", "fps", "rendering", "networking",
                "compression", "algorithm", "database", "query", "index"
            };

            foreach (var term in technicalTerms)
            {
                if (allText.Contains(term))
                    keywords.Add(term);
            }

            return keywords.Distinct().ToList();
        }

        /// <summary>
        /// Extract research areas (broader categories)
        /// </summary>
        private List<string> ExtractResearchAreas(string query)
        {
            var areas = new List<string>();
            var lowerQuery = query.ToLower();

            if (lowerQuery.Contains("game") || lowerQuery.Contains("fps") || lowerQuery.Contains("valorant"))
                areas.Add("Gaming Optimization");
            if (lowerQuery.Contains("code") || lowerQuery.Contains("compile") || lowerQuery.Contains("build"))
                areas.Add("Development");
            if (lowerQuery.Contains("memory") || lowerQuery.Contains("leak") || lowerQuery.Contains("allocation"))
                areas.Add("Memory Management");
            if (lowerQuery.Contains("thread") || lowerQuery.Contains("async") || lowerQuery.Contains("concurrent"))
                areas.Add("Concurrency");
            if (lowerQuery.Contains("gpu") || lowerQuery.Contains("shader") || lowerQuery.Contains("render"))
                areas.Add("Graphics");
            if (lowerQuery.Contains("network") || lowerQuery.Contains("ping") || lowerQuery.Contains("latency"))
                areas.Add("Networking");
            if (lowerQuery.Contains("database") || lowerQuery.Contains("query") || lowerQuery.Contains("index"))
                areas.Add("Data");

            return areas.Distinct().ToList();
        }

        /// <summary>
        /// Extract problems being discussed
        /// </summary>
        private List<string> ExtractProblems(string query)
        {
            var problems = new List<string>();
            var lowerQuery = query.ToLower();

            if (lowerQuery.Contains("slow") || lowerQuery.Contains("lag") || lowerQuery.Contains("stutter"))
                problems.Add("Performance Degradation");
            if (lowerQuery.Contains("crash") || lowerQuery.Contains("error") || lowerQuery.Contains("fail"))
                problems.Add("System Failure");
            if (lowerQuery.Contains("hot") || lowerQuery.Contains("thermal") || lowerQuery.Contains("temp"))
                problems.Add("Thermal Throttling");
            if (lowerQuery.Contains("high") && lowerQuery.Contains("usage"))
                problems.Add("Resource Contention");
            if (lowerQuery.Contains("bottleneck"))
                problems.Add("Bottleneck Identification");

            return problems.Distinct().ToList();
        }

        /// <summary>
        /// Extract suggested solutions
        /// </summary>
        private List<string> ExtractSolutions(string response)
        {
            var solutions = new List<string>();
            var lowerResponse = response.ToLower();

            if (lowerResponse.Contains("disable") || lowerResponse.Contains("off"))
                solutions.Add("Disable Service/Feature");
            if (lowerResponse.Contains("increase") || lowerResponse.Contains("boost"))
                solutions.Add("Increase Resource Allocation");
            if (lowerResponse.Contains("cache"))
                solutions.Add("Cache Optimization");
            if (lowerResponse.Contains("thread") || lowerResponse.Contains("parallel"))
                solutions.Add("Parallelization");
            if (lowerResponse.Contains("profile") || lowerResponse.Contains("monitor"))
                solutions.Add("Monitoring/Profiling");

            return solutions.Distinct().ToList();
        }

        /// <summary>
        /// Determine category from query content
        /// </summary>
        private string DetermineCategoryFromQuery(string query)
        {
            var lower = query.ToLower();

            if (lower.Contains("game") || lower.Contains("valorant") || lower.Contains("fps"))
                return "Gaming";
            if (lower.Contains("develop") || lower.Contains("code") || lower.Contains("build"))
                return "Development";
            if (lower.Contains("render") || lower.Contains("graphic") || lower.Contains("3d"))
                return "Rendering";
            if (lower.Contains("database") || lower.Contains("data"))
                return "Data";
            if (lower.Contains("network") || lower.Contains("internet"))
                return "Networking";

            return "General";
        }

        /// <summary>
        /// Calculate confidence score (0-100) for ML training weight
        /// </summary>
        private int CalculateConfidence(string query, string response)
        {
            int score = 70;

            // More specific queries = higher confidence
            if (query.Length > 100) score += 10;
            if (query.Contains("?")) score += 5;
            if (query.Contains("problem") || query.Contains("issue")) score += 5;

            // Response length and detail matter
            if (response.Length > 500) score += 10;
            if (response.Contains("specific") || response.Contains("recommend")) score += 5;

            return Math.Min(100, score);
        }

        /// <summary>
        /// Get conversation history
        /// </summary>
        public List<ConversationEntry> GetConversationHistory(int last = 100)
        {
            return _sessionHistory.TakeLast(last).ToList();
        }

        /// <summary>
        /// Get conversations by topic
        /// </summary>
        public List<ConversationEntry> GetConversationsByTopic(string topic)
        {
            return _sessionHistory.Where(c => c.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Get conversations by research area
        /// </summary>
        public List<ConversationEntry> GetConversationsByResearchArea(string area)
        {
            return _sessionHistory.Where(c => c.ResearchAreas.Contains(area)).ToList();
        }

        /// <summary>
        /// Get summary statistics
        /// </summary>
        public object GetConversationSummary(int last = 100)
        {
            var recent = _sessionHistory.TakeLast(last).ToList();

            if (!recent.Any())
                return new { message = "No conversation history" };

            return new
            {
                conversationsLogged = recent.Count,
                timeRange = new
                {
                    start = recent.First().Timestamp,
                    end = recent.Last().Timestamp,
                    durationMinutes = (recent.Last().Timestamp - recent.First().Timestamp).TotalMinutes
                },
                topTopics = recent.GroupBy(c => c.Topic)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Count()),
                topKeywords = recent.SelectMany(c => c.Keywords)
                    .GroupBy(k => k)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count()),
                outcomeBreakdown = recent.GroupBy(c => c.Outcome)
                    .ToDictionary(g => g.Key, g => g.Count()),
                averageConfidence = recent.Average(c => c.ConfidenceScore),
                researchAreasExplored = recent.SelectMany(c => c.ResearchAreas)
                    .Distinct()
                    .ToList()
            };
        }
    }
}

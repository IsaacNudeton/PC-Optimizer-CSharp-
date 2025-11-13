using System;
using System.Collections.Generic;
using System.Linq;

namespace PCOptimizer.Services.AI.Memory
{
    /// <summary>
    /// Attention Mechanism - Key/Value Vector Processing
    ///
    /// Like human attention (we can't think about everything at once),
    /// this mechanism focuses the AI's "thinking" on what's most important in the current moment.
    ///
    /// Inspired by transformer architecture but applied biologically:
    /// - Query: "What am I trying to accomplish right now?" (Goal/intent)
    /// - Key: "What aspects of memory are relevant to this?" (Memory tagging)
    /// - Value: "What's the actual knowledge/experience?" (Memory content)
    ///
    /// Example:
    /// - Query: "User wants to play Valorant"
    /// - Keys: ["Gaming", "Latency", "FPS", "CPU", "Network"]
    /// - Values: [All relevant facts, episodes, and causal chains tagged with these keys]
    /// - Attention Scores: [0.95, 0.88, 0.90, 0.75, 0.80]
    /// - Result: The system focuses on Latency and FPS facts most strongly
    /// </summary>
    public class AttentionMechanism
    {
        /// <summary>
        /// A single attention vector that focuses on specific aspects of memory
        /// </summary>
        public class AttentionVector
        {
            /// <summary>Unique identifier</summary>
            public string Id { get; set; }

            /// <summary>What is this attention focused on? (e.g., "Optimize for Valorant")</summary>
            public string Query { get; set; }

            /// <summary>Keys from memory that relate to this query</summary>
            public Dictionary<string, double> KeyWeights { get; set; }

            /// <summary>Attention scores for each key (0-1, how focused on this key)</summary>
            public Dictionary<string, double> AttentionScores { get; set; }

            /// <summary>Related memory IDs that this attention pulls up</summary>
            public List<string> RelatedMemoryIds { get; set; }

            /// <summary>The resulting value: integrated knowledge from relevant memories</summary>
            public Dictionary<string, object> Value { get; set; }

            /// <summary>How confident is this attention vector (0-1)</summary>
            public double Confidence { get; set; }

            /// <summary>When was this attention vector created</summary>
            public DateTime CreatedAt { get; set; }

            public AttentionVector()
            {
                Id = Guid.NewGuid().ToString();
                KeyWeights = new Dictionary<string, double>();
                AttentionScores = new Dictionary<string, double>();
                RelatedMemoryIds = new List<string>();
                Value = new Dictionary<string, object>();
                CreatedAt = DateTime.UtcNow;
                Confidence = 0.5;
            }
        }

        /// <summary>
        /// Multi-head attention - multiple simultaneous "thoughts"
        /// Like different parts of the brain thinking about different aspects
        /// </summary>
        public class AttentionHead
        {
            public string Id { get; set; }
            public string Focus { get; set; }
            public AttentionVector Vector { get; set; }

            public AttentionHead()
            {
                Id = Guid.NewGuid().ToString();
            }
        }

        private readonly UniversalMemorySystem _memory;
        private readonly List<AttentionVector> _attentionHistory;
        private readonly List<AttentionHead> _activeHeads;
        private const int MAX_ACTIVE_HEADS = 3; // Biological limit - can't focus on more than 3 things

        public AttentionMechanism(UniversalMemorySystem memory)
        {
            _memory = memory;
            _attentionHistory = new List<AttentionVector>();
            _activeHeads = new List<AttentionHead>();

            Console.WriteLine("[AttentionMechanism] Initialized with memory system");
        }

        /// <summary>
        /// Create attention vectors based on current situation
        /// Like the brain deciding what to pay attention to
        /// </summary>
        public AttentionVector CreateAttention(string query, Dictionary<string, double>? keyWeights = null)
        {
            var attention = new AttentionVector { Query = query };

            // If no specific keys provided, analyze the query to find relevant keys
            if (keyWeights == null || !keyWeights.Any())
            {
                keyWeights = DeriveKeysFromQuery(query);
            }

            attention.KeyWeights = keyWeights;

            // Compute attention scores for each key
            ComputeAttentionScores(attention);

            // Retrieve and integrate related values from memory
            RetrieveValues(attention);

            _attentionHistory.Add(attention);
            _activeHeads.Add(new AttentionHead { Focus = query, Vector = attention });

            Console.WriteLine($"[AttentionMechanism] Created attention vector for: {query}");

            return attention;
        }

        /// <summary>
        /// Derive relevant keys from a query
        /// e.g., "Optimize for Valorant" → Keys: ["Gaming", "FPS", "Latency", "Network", "CPU"]
        /// </summary>
        private Dictionary<string, double> DeriveKeysFromQuery(string query)
        {
            var keys = new Dictionary<string, double>();

            // Keyword matching
            if (query.Contains("valorant", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("gaming", StringComparison.OrdinalIgnoreCase))
            {
                keys["Gaming"] = 0.95;
                keys["FPS"] = 0.90;
                keys["Latency"] = 0.85;
                keys["Network"] = 0.75;
                keys["CPU"] = 0.70;
            }

            if (query.Contains("stream", StringComparison.OrdinalIgnoreCase))
            {
                keys["Streaming"] = 0.95;
                keys["Bandwidth"] = 0.85;
                keys["GPU"] = 0.80;
                keys["CPU"] = 0.75;
            }

            if (query.Contains("develop", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("code", StringComparison.OrdinalIgnoreCase))
            {
                keys["Development"] = 0.95;
                keys["Memory"] = 0.85;
                keys["CompileSpeed"] = 0.80;
                keys["Responsiveness"] = 0.75;
            }

            // Default keys if no match
            if (!keys.Any())
            {
                keys["General"] = 0.6;
                keys["Performance"] = 0.5;
                keys["Optimization"] = 0.5;
            }

            return keys;
        }

        /// <summary>
        /// Compute attention scores using softmax-like scoring
        /// Scores represent how much to "pay attention" to each key
        /// </summary>
        private void ComputeAttentionScores(AttentionVector attention)
        {
            var scores = new Dictionary<string, double>();

            // Use key weights as input to attention scoring
            var weightSum = attention.KeyWeights.Values.Sum();

            foreach (var kw in attention.KeyWeights)
            {
                // Softmax-like: normalize and exponentiate for sharp attention peaks
                var normalizedWeight = kw.Value / weightSum;
                var score = Math.Exp(normalizedWeight * 2) / (1 + Math.Exp(normalizedWeight * 2)); // Sigmoid-like
                scores[kw.Key] = Math.Min(1.0, score);
            }

            attention.AttentionScores = scores;

            // Confidence increases with focused attention (when one or few keys dominate)
            var maxScore = scores.Values.Max();
            var variance = Math.Sqrt(scores.Values.Select(s => Math.Pow(s - scores.Values.Average(), 2)).Average());
            attention.Confidence = maxScore - (variance * 0.1); // Higher confidence when attention is focused
        }

        /// <summary>
        /// Retrieve relevant values from memory based on attention scores
        /// </summary>
        private void RetrieveValues(AttentionVector attention)
        {
            var value = new Dictionary<string, object>();

            // For each attended key, pull relevant information
            foreach (var (key, score) in attention.AttentionScores.OrderByDescending(s => s.Value))
            {
                if (score < 0.3) continue; // Ignore low-attention keys

                // Recall knowledge relevant to this key
                var knowledge = _memory.RecallRelevantKnowledge("", key);

                if (knowledge.ContainsKey("SemanticFacts"))
                {
                    value[$"Facts_{key}"] = knowledge["SemanticFacts"];
                    if (knowledge["SemanticFacts"] is IEnumerable<object> facts)
                    {
                        attention.RelatedMemoryIds.AddRange(facts.Select(f => f?.ToString() ?? "").Where(s => !string.IsNullOrEmpty(s)));
                    }
                }

                if (knowledge.ContainsKey("EpisodicExperiences"))
                {
                    value[$"Experiences_{key}"] = knowledge["EpisodicExperiences"];
                }

                if (knowledge.ContainsKey("CausalChain"))
                {
                    value[$"Causality_{key}"] = knowledge["CausalChain"];
                }
            }

            attention.Value = value;
        }

        /// <summary>
        /// Multi-head attention - think about multiple aspects simultaneously
        /// Like different streams of consciousness in parallel
        /// </summary>
        public List<AttentionVector> CreateMultiHeadAttention(string query, int headCount = 3)
        {
            var heads = new List<AttentionVector>();

            // Generate different perspectives on the same query
            var perspectives = GenerateAttentionPerspectives(query, Math.Min(headCount, MAX_ACTIVE_HEADS));

            foreach (var perspective in perspectives)
            {
                var vector = CreateAttention(query, perspective);
                heads.Add(vector);
            }

            Console.WriteLine($"[AttentionMechanism] Created {heads.Count} parallel attention heads");

            return heads;
        }

        /// <summary>
        /// Generate multiple different perspectives on a query
        /// e.g., "Optimize Valorant" could be viewed from FPS perspective, Latency perspective, or Stability perspective
        /// </summary>
        private List<Dictionary<string, double>> GenerateAttentionPerspectives(string query, int count)
        {
            var perspectives = new List<Dictionary<string, double>>();

            if (query.Contains("valorant", StringComparison.OrdinalIgnoreCase))
            {
                // Perspective 1: FPS focus
                perspectives.Add(new Dictionary<string, double>
                {
                    { "FPS", 0.95 }, { "GPU", 0.85 }, { "CPU", 0.70 }, { "Memory", 0.60 }
                });

                // Perspective 2: Latency/responsiveness focus
                perspectives.Add(new Dictionary<string, double>
                {
                    { "Latency", 0.95 }, { "Network", 0.90 }, { "CPU", 0.75 }, { "Interrupts", 0.70 }
                });

                // Perspective 3: Stability focus
                perspectives.Add(new Dictionary<string, double>
                {
                    { "SystemStability", 0.95 }, { "Temperature", 0.80 }, { "Memory", 0.75 }, { "Crashes", 0.70 }
                });
            }

            // Default perspectives if query doesn't match
            while (perspectives.Count < count)
            {
                perspectives.Add(DeriveKeysFromQuery(query));
            }

            return perspectives.Take(count).ToList();
        }

        /// <summary>
        /// Integrate multi-head attention into a single decision
        /// Like combining thoughts from different brain regions
        /// </summary>
        public Dictionary<string, object> IntegrateAttention(List<AttentionVector> heads)
        {
            if (!heads.Any())
            {
                return new Dictionary<string, object> { { "Error", "No attention heads provided" } };
            }

            // Average attention scores across heads
            var integratedScores = new Dictionary<string, double>();

            foreach (var head in heads)
            {
                foreach (var (key, score) in head.AttentionScores)
                {
                    if (!integratedScores.ContainsKey(key))
                    {
                        integratedScores[key] = 0;
                    }
                    integratedScores[key] += score / heads.Count;
                }
            }

            // Average confidence
            var avgConfidence = heads.Average(h => h.Confidence);

            // Combine values from all heads
            var integratedValue = new Dictionary<string, object>();
            foreach (var head in heads)
            {
                foreach (var (key, value) in head.Value)
                {
                    if (!integratedValue.ContainsKey(key))
                    {
                        integratedValue[key] = new List<object>();
                    }
                    if (integratedValue[key] is List<object> list)
                    {
                        list.Add(value);
                    }
                }
            }

            return new Dictionary<string, object>
            {
                { "IntegratedAttentionScores", integratedScores },
                { "AverageConfidence", avgConfidence },
                { "IntegratedValue", integratedValue },
                { "HeadCount", heads.Count },
                { "Query", heads.First().Query }
            };
        }

        /// <summary>
        /// Shift attention - change focus when something more important happens
        /// Like a sudden loud noise capturing attention
        /// </summary>
        public void ShiftAttention(string newQuery, double urgency = 0.8)
        {
            // Clear low-priority heads if there's urgent focus
            if (urgency > 0.7 && _activeHeads.Count > MAX_ACTIVE_HEADS / 2)
            {
                var headToRemove = _activeHeads.OrderBy(h => h.Vector.Confidence).First();
                _activeHeads.Remove(headToRemove);
            }

            // Create new attention for urgent query
            var newAttention = CreateAttention(newQuery);
            newAttention.Confidence *= urgency;

            Console.WriteLine($"[AttentionMechanism] Shifted attention to: {newQuery} (urgency: {urgency})");
        }

        /// <summary>
        /// Get the current focus of the attention system
        /// </summary>
        public List<Dictionary<string, object>> GetCurrentFocus()
        {
            return _activeHeads.Select(h => new Dictionary<string, object>
            {
                { "Focus", h.Focus },
                { "Confidence", h.Vector.Confidence },
                { "AttentionScores", h.Vector.AttentionScores },
                { "RelatedMemoryCount", h.Vector.RelatedMemoryIds.Count }
            }).ToList();
        }

        /// <summary>
        /// Get attention history for debugging and analysis
        /// </summary>
        public List<AttentionVector> GetAttentionHistory(int limit = 10)
        {
            return _attentionHistory.OrderByDescending(a => a.CreatedAt).Take(limit).ToList();
        }

        /// <summary>
        /// Predict what the system will pay attention to next
        /// </summary>
        public string PredictNextAttention()
        {
            if (!_attentionHistory.Any())
            {
                return "No attention history yet";
            }

            var recentAttention = _attentionHistory.TakeLast(5);
            var commonFoci = recentAttention
                .SelectMany(a => a.AttentionScores.Keys)
                .GroupBy(k => k)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return commonFoci?.Key ?? "Unknown";
        }

        /// <summary>
        /// Get statistics about attention patterns
        /// </summary>
        public Dictionary<string, object> GetAttentionStatistics()
        {
            var avgConfidence = _attentionHistory.Any() ? _attentionHistory.Average(a => a.Confidence) : 0;
            var focusFrequency = new Dictionary<string, int>();

            foreach (var attention in _attentionHistory)
            {
                foreach (var key in attention.AttentionScores.Keys)
                {
                    focusFrequency[key] = focusFrequency.GetValueOrDefault(key, 0) + 1;
                }
            }

            return new Dictionary<string, object>
            {
                { "TotalAttentionVectors", _attentionHistory.Count },
                { "AverageConfidence", Math.Round(avgConfidence, 3) },
                { "ActiveHeads", _activeHeads.Count },
                { "MaxActiveHeads", MAX_ACTIVE_HEADS },
                { "FocusFrequency", focusFrequency.OrderByDescending(f => f.Value).ToList() },
                { "MostFrequentFocus", focusFrequency.OrderByDescending(f => f.Value).FirstOrDefault().Key }
            };
        }
    }
}

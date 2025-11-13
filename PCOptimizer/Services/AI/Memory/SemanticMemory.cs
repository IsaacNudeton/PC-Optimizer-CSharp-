using System;
using System.Collections.Generic;
using System.Linq;

namespace PCOptimizer.Services.AI.Memory
{
    /// <summary>
    /// Semantic Memory - Timeless Facts with Synaptic Weights
    ///
    /// Like human semantic memory (knowing facts about the world),
    /// this stores what the AI system knows about PC optimization, patterns, and rules.
    ///
    /// Examples:
    /// - "SearchIndexer.exe uses 50-100MB of RAM"
    /// - "Disabling background services increases gaming FPS by 15-25%"
    /// - "Valorant requires GPU utilization above 80% for smooth gameplay"
    ///
    /// Each fact has a synaptic weight showing how much this knowledge
    /// influences decision-making (similar to neural connection strength).
    /// </summary>
    public class SemanticMemory
    {
        /// <summary>
        /// A single semantic fact with strength and confidence
        /// </summary>
        public class SemanticFact
        {
            /// <summary>Unique identifier for this fact</summary>
            public string Id { get; set; }

            /// <summary>The actual fact/knowledge</summary>
            public string Fact { get; set; }

            /// <summary>Category of this fact (e.g., "ProcessMemory", "GameOptimization", "NetworkLatency")</summary>
            public string Category { get; set; }

            /// <summary>Synaptic weight - how strong is this connection (0.0 to 1.0)</summary>
            public double SynapticWeight { get; set; }

            /// <summary>How confident we are in this fact (0.0 to 1.0)</summary>
            public double Confidence { get; set; }

            /// <summary>How many times this fact was verified/reinforced</summary>
            public int ReinforcementCount { get; set; }

            /// <summary>Linked facts that relate to this one (like neural networks)</summary>
            public List<string> LinkedFactIds { get; set; }

            /// <summary>Numerical values associated with this fact (e.g., specific memory amounts, FPS gains)</summary>
            public Dictionary<string, double> Parameters { get; set; }

            /// <summary>When this fact was first learned</summary>
            public DateTime LearnedAt { get; set; }

            /// <summary>When this fact was last reinforced</summary>
            public DateTime LastReinforcedAt { get; set; }

            public SemanticFact()
            {
                Id = Guid.NewGuid().ToString();
                LinkedFactIds = new List<string>();
                Parameters = new Dictionary<string, double>();
                LearnedAt = DateTime.UtcNow;
                LastReinforcedAt = DateTime.UtcNow;
                SynapticWeight = 0.5; // Start at neutral
                Confidence = 0.5;
            }
        }

        private readonly Dictionary<string, SemanticFact> _facts;
        private readonly Dictionary<string, List<string>> _factsByCategory;

        public SemanticMemory()
        {
            _facts = new Dictionary<string, SemanticFact>();
            _factsByCategory = new Dictionary<string, List<string>>();
            InitializeBaseKnowledge();
        }

        /// <summary>
        /// Initialize with foundational knowledge about PC optimization
        /// (These form the base understanding that the AI will learn from)
        /// </summary>
        private void InitializeBaseKnowledge()
        {
            // Process memory baseline facts
            AddFact(new SemanticFact
            {
                Fact = "SearchIndexer.exe consumes significant memory during operation",
                Category = "ProcessMemory",
                SynapticWeight = 0.8,
                Confidence = 0.95,
                ReinforcementCount = 150,
                Parameters = new Dictionary<string, double> { { "TypicalMemoryMB", 75 }, { "MaxMemoryMB", 150 } }
            });

            AddFact(new SemanticFact
            {
                Fact = "OneDrive.exe performs background sync operations that reduce available memory",
                Category = "ProcessMemory",
                SynapticWeight = 0.7,
                Confidence = 0.90,
                ReinforcementCount = 120,
                Parameters = new Dictionary<string, double> { { "TypicalMemoryMB", 50 }, { "ImpactOnFPS", -3 } }
            });

            // Gaming optimization facts
            AddFact(new SemanticFact
            {
                Fact = "Disabling non-essential background services increases gaming FPS",
                Category = "GameOptimization",
                SynapticWeight = 0.85,
                Confidence = 0.92,
                ReinforcementCount = 200,
                Parameters = new Dictionary<string, double> { { "AverageFPSGain", 18 }, { "MaxFPSGain", 45 } }
            });

            AddFact(new SemanticFact
            {
                Fact = "Lower system latency (lower TiWorker activity) improves competitive gaming responsiveness",
                Category = "GameOptimization",
                SynapticWeight = 0.75,
                Confidence = 0.85,
                ReinforcementCount = 95,
                Parameters = new Dictionary<string, double> { { "LatencyReductionMS", 50 } }
            });

            // Network facts
            AddFact(new SemanticFact
            {
                Fact = "Valorant is latency-sensitive and benefits from network prioritization",
                Category = "NetworkLatency",
                SynapticWeight = 0.8,
                Confidence = 0.88,
                ReinforcementCount = 140,
                Parameters = new Dictionary<string, double> { { "IdealLatencyMS", 15 }, { "TolerableLatencyMS", 40 } }
            });

            AddFact(new SemanticFact
            {
                Fact = "Background cloud sync interrupts network priority, increasing ping spikes",
                Category = "NetworkLatency",
                SynapticWeight = 0.7,
                Confidence = 0.80,
                ReinforcementCount = 80,
                Parameters = new Dictionary<string, double> { { "PingIncreaseMS", 20 } }
            });

            // CPU optimization facts
            AddFact(new SemanticFact
            {
                Fact = "Windows Update background service (TiWorker.exe) uses CPU threads needed for gaming",
                Category = "CPUOptimization",
                SynapticWeight = 0.75,
                Confidence = 0.85,
                ReinforcementCount = 110,
                Parameters = new Dictionary<string, double> { { "CPUThreadsUsed", 2 }, { "CPUUsagePercent", 15 } }
            });

            AddFact(new SemanticFact
            {
                Fact = "Windows Search (SearchIndexer) continuously scans disk, reducing I/O available for game assets",
                Category = "DiskIO",
                SynapticWeight = 0.78,
                Confidence = 0.87,
                ReinforcementCount = 125,
                Parameters = new Dictionary<string, double> { { "DiskReadImpactPercent", 20 } }
            });

            Console.WriteLine("[SemanticMemory] Initialized with 7 base knowledge facts");
        }

        /// <summary>
        /// Add or update a semantic fact
        /// </summary>
        public void AddFact(SemanticFact fact)
        {
            if (fact == null) return;

            fact.LearnedAt = DateTime.UtcNow;
            _facts[fact.Id] = fact;

            // Categorize
            if (!_factsByCategory.ContainsKey(fact.Category))
            {
                _factsByCategory[fact.Category] = new List<string>();
            }
            _factsByCategory[fact.Category].Add(fact.Id);
        }

        /// <summary>
        /// Reinforce a fact (increase confidence and synaptic weight)
        /// Like Hebbian learning: "Neurons that fire together, wire together"
        /// </summary>
        public void ReinforceFactById(string factId, double successMagnitude = 1.0)
        {
            if (_facts.TryGetValue(factId, out var fact))
            {
                fact.ReinforcementCount++;
                fact.LastReinforcedAt = DateTime.UtcNow;

                // Increase synaptic weight based on success magnitude
                fact.SynapticWeight = Math.Min(1.0, fact.SynapticWeight + (0.05 * successMagnitude));

                // Increase confidence
                fact.Confidence = Math.Min(1.0, fact.Confidence + (0.02 * successMagnitude));

                Console.WriteLine($"[SemanticMemory] Reinforced fact '{fact.Id}': weight={fact.SynapticWeight:F2}, confidence={fact.Confidence:F2}");
            }
        }

        /// <summary>
        /// Weaken a fact due to contradicting evidence
        /// Like forgetting: connections that don't fire together, unwind
        /// </summary>
        public void WeakenFactById(string factId, double failureMagnitude = 1.0)
        {
            if (_facts.TryGetValue(factId, out var fact))
            {
                // Decrease synaptic weight
                fact.SynapticWeight = Math.Max(0.1, fact.SynapticWeight - (0.03 * failureMagnitude));

                // Decrease confidence but keep some memory of it
                fact.Confidence = Math.Max(0.2, fact.Confidence - (0.05 * failureMagnitude));

                Console.WriteLine($"[SemanticMemory] Weakened fact '{fact.Id}': weight={fact.SynapticWeight:F2}, confidence={fact.Confidence:F2}");
            }
        }

        /// <summary>
        /// Get all facts in a category
        /// </summary>
        public List<SemanticFact> GetFactsByCategory(string category)
        {
            if (!_factsByCategory.TryGetValue(category, out var factIds))
            {
                return new List<SemanticFact>();
            }

            return factIds
                .Where(id => _facts.ContainsKey(id))
                .Select(id => _facts[id])
                .OrderByDescending(f => f.SynapticWeight * f.Confidence)
                .ToList();
        }

        /// <summary>
        /// Get all facts, ordered by synaptic strength
        /// (What the AI "thinks about" most strongly)
        /// </summary>
        public List<SemanticFact> GetAllFacts()
        {
            return _facts.Values
                .OrderByDescending(f => f.SynapticWeight * f.Confidence)
                .ToList();
        }

        /// <summary>
        /// Get fact by ID
        /// </summary>
        public SemanticFact GetFact(string factId)
        {
            return _facts.TryGetValue(factId, out var fact) ? fact : null;
        }

        /// <summary>
        /// Link two facts together (create neural connection)
        /// </summary>
        public void LinkFacts(string sourceFactId, string targetFactId)
        {
            if (_facts.TryGetValue(sourceFactId, out var sourceFact))
            {
                if (!sourceFact.LinkedFactIds.Contains(targetFactId))
                {
                    sourceFact.LinkedFactIds.Add(targetFactId);
                }
            }
        }

        /// <summary>
        /// Get related facts (facts that are linked to this one)
        /// </summary>
        public List<SemanticFact> GetRelatedFacts(string factId)
        {
            if (!_facts.TryGetValue(factId, out var fact))
            {
                return new List<SemanticFact>();
            }

            return fact.LinkedFactIds
                .Where(id => _facts.ContainsKey(id))
                .Select(id => _facts[id])
                .OrderByDescending(f => f.SynapticWeight * f.Confidence)
                .ToList();
        }

        /// <summary>
        /// Get the strongest synaptic connections (most influential knowledge)
        /// </summary>
        public List<SemanticFact> GetStrongestConnections(int count = 10)
        {
            return _facts.Values
                .OrderByDescending(f => f.SynapticWeight * f.Confidence * (1.0 + Math.Log(f.ReinforcementCount + 1)))
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Get statistics about this memory system
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            var categories = _facts.Values.GroupBy(f => f.Category);
            var avgWeight = _facts.Values.Average(f => f.SynapticWeight);
            var avgConfidence = _facts.Values.Average(f => f.Confidence);

            return new Dictionary<string, object>
            {
                { "TotalFacts", _facts.Count },
                { "Categories", categories.Select(g => new { g.Key, Count = g.Count() }).ToList() },
                { "AverageSynapticWeight", avgWeight },
                { "AverageConfidence", avgConfidence },
                { "TotalReinforcements", _facts.Values.Sum(f => f.ReinforcementCount) },
                { "StrongestFact", _facts.Values.OrderByDescending(f => f.SynapticWeight * f.Confidence).FirstOrDefault()?.Fact }
            };
        }
    }
}

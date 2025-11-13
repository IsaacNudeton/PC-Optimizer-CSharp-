using System;
using System.Collections.Generic;
using System.Linq;

namespace PCOptimizer.Services.AI.Memory
{
    /// <summary>
    /// Causal Memory - Understanding Cause and Effect Chains
    ///
    /// Like understanding why things happen (causal reasoning),
    /// this stores chains of causality and exceptions that help predict outcomes.
    ///
    /// Examples:
    /// - "SearchIndexer.exe runs → Uses CPU cycles → Causes higher CPU% →
    ///   Reduces available cycles for Valorant → FPS drops → Disabling improves FPS"
    ///
    /// - "OneDrive cloud sync starts → Network I/O increases →
    ///   Ping spikes → Gaming latency increases → BUT: User approved OneDrive,
    ///   so don't disable it despite negative impact"
    ///
    /// Causal chains help the AI understand not just what works, but WHY it works,
    /// and when exceptions should apply.
    /// </summary>
    public class CausalMemory
    {
        /// <summary>
        /// A single causal link in a chain
        /// </summary>
        public class CausalNode
        {
            /// <summary>Unique identifier</summary>
            public string Id { get; set; }

            /// <summary>What is this event/state?</summary>
            public string Event { get; set; }

            /// <summary>Category of this event</summary>
            public string Category { get; set; }

            /// <summary>Confidence that this event causes its children (0-1)</summary>
            public double CausalStrength { get; set; }

            /// <summary>How many times this causal link was observed</summary>
            public int ObservationCount { get; set; }

            public CausalNode()
            {
                Id = Guid.NewGuid().ToString();
                CausalStrength = 0.5;
                ObservationCount = 0;
            }
        }

        /// <summary>
        /// A directed causal relationship
        /// </summary>
        public class CausalLink
        {
            /// <summary>Unique identifier</summary>
            public string Id { get; set; }

            /// <summary>What causes what</summary>
            public string CauseNodeId { get; set; }
            public string EffectNodeId { get; set; }

            /// <summary>How strong is this causal relationship (0-1)</summary>
            public double Strength { get; set; }

            /// <summary>How many times this link was observed</summary>
            public int ObservationCount { get; set; }

            /// <summary>Expected latency between cause and effect</summary>
            public TimeSpan TypicalLatency { get; set; }

            /// <summary>Any exceptions to this rule</summary>
            public List<string> ExceptionIds { get; set; }

            /// <summary>Percentage of times this link holds true</summary>
            public double ReliabilityPercent { get; set; }

            public CausalLink()
            {
                Id = Guid.NewGuid().ToString();
                Strength = 0.5;
                ObservationCount = 0;
                ExceptionIds = new List<string>();
                ReliabilityPercent = 50;
            }
        }

        /// <summary>
        /// An exception to a causal rule
        /// </summary>
        public class CausalException
        {
            /// <summary>Unique identifier</summary>
            public string Id { get; set; }

            /// <summary>Which causal link does this exempt from?</summary>
            public string CausalLinkId { get; set; }

            /// <summary>What condition makes this exception apply?</summary>
            public string Condition { get; set; }

            /// <summary>Why does this exception exist?</summary>
            public string Reason { get; set; }

            /// <summary>How confident are we in this exception?</summary>
            public double Confidence { get; set; }

            /// <summary>When was this exception learned?</summary>
            public DateTime LearnedAt { get; set; }

            public CausalException()
            {
                Id = Guid.NewGuid().ToString();
                LearnedAt = DateTime.UtcNow;
                Confidence = 0.5;
            }
        }

        /// <summary>
        /// A complete causal chain
        /// </summary>
        public class CausalChain
        {
            public string Id { get; set; }

            /// <summary>The nodes in this chain, in order</summary>
            public List<string> NodeIds { get; set; }

            /// <summary>Overall confidence in this chain</summary>
            public double Confidence { get; set; }

            /// <summary>Category/type of chain</summary>
            public string ChainType { get; set; }

            /// <summary>When was this chain first established?</summary>
            public DateTime EstablishedAt { get; set; }

            /// <summary>How many times was this chain observed/verified?</summary>
            public int VerificationCount { get; set; }

            public CausalChain()
            {
                Id = Guid.NewGuid().ToString();
                NodeIds = new List<string>();
                Confidence = 0.5;
                EstablishedAt = DateTime.UtcNow;
                VerificationCount = 0;
            }
        }

        private readonly Dictionary<string, CausalNode> _nodes;
        private readonly Dictionary<string, CausalLink> _links;
        private readonly Dictionary<string, CausalException> _exceptions;
        private readonly List<CausalChain> _chains;

        public CausalMemory()
        {
            _nodes = new Dictionary<string, CausalNode>();
            _links = new Dictionary<string, CausalLink>();
            _exceptions = new Dictionary<string, CausalException>();
            _chains = new List<CausalChain>();
            InitializeBaseCausalChains();
        }

        /// <summary>
        /// Initialize with foundational causal understanding
        /// </summary>
        private void InitializeBaseCausalChains()
        {
            // Chain 1: SearchIndexer → CPU Usage → Gaming Impact
            var chain1 = BuildCausalChain(
                "Gaming Performance Degradation Chain",
                new[] {
                    "SearchIndexer.exe is active",
                    "Disk I/O scans continuous files",
                    "CPU cycles consumed by indexing",
                    "Available cycles reduced for game",
                    "Game performance decreases"
                }
            );

            // Chain 2: OneDrive → Network I/O → Latency
            var chain2 = BuildCausalChain(
                "Network Latency Chain",
                new[] {
                    "OneDrive sync processes data",
                    "Network bandwidth consumed",
                    "Packet priority reduced for gaming",
                    "Network latency increases",
                    "Gaming responsiveness decreases"
                }
            );

            // Chain 3: TiWorker → System Interrupts → Responsiveness
            var chain3 = BuildCausalChain(
                "System Responsiveness Chain",
                new[] {
                    "Windows Update (TiWorker) runs",
                    "System interrupts increase",
                    "Real-time thread scheduling affected",
                    "Input latency increases",
                    "User perceives game lag"
                }
            );

            // Chain 4: Power Plan → CPU Speed → Performance
            var chain4 = BuildCausalChain(
                "Power Plan Performance Chain",
                new[] {
                    "Power plan changed to High Performance",
                    "CPU base clock maintained at max",
                    "Thermal headroom available",
                    "Sustained performance possible",
                    "FPS stability improves"
                }
            );

            Console.WriteLine("[CausalMemory] Initialized with 4 foundational causal chains");
        }

        /// <summary>
        /// Build a causal chain from events
        /// </summary>
        private CausalChain BuildCausalChain(string chainType, string[] events)
        {
            var chain = new CausalChain { ChainType = chainType };

            CausalNode previousNode = null;

            foreach (var eventText in events)
            {
                var node = new CausalNode
                {
                    Event = eventText,
                    Category = chainType,
                    CausalStrength = 0.85,
                    ObservationCount = 10
                };

                _nodes[node.Id] = node;
                chain.NodeIds.Add(node.Id);

                // Create link from previous node
                if (previousNode != null)
                {
                    var link = new CausalLink
                    {
                        CauseNodeId = previousNode.Id,
                        EffectNodeId = node.Id,
                        Strength = 0.85,
                        ObservationCount = 10,
                        TypicalLatency = TimeSpan.FromMilliseconds(100),
                        ReliabilityPercent = 90
                    };

                    _links[link.Id] = link;
                }

                previousNode = node;
            }

            chain.Confidence = 0.85;
            chain.VerificationCount = 10;
            _chains.Add(chain);

            return chain;
        }

        /// <summary>
        /// Add a new causal node
        /// </summary>
        public string AddNode(string eventName, string category)
        {
            var node = new CausalNode
            {
                Event = eventName,
                Category = category
            };

            _nodes[node.Id] = node;
            return node.Id;
        }

        /// <summary>
        /// Link two events causally
        /// </summary>
        public string LinkNodes(string causeNodeId, string effectNodeId, double initialStrength = 0.5)
        {
            var link = new CausalLink
            {
                CauseNodeId = causeNodeId,
                EffectNodeId = effectNodeId,
                Strength = initialStrength
            };

            _links[link.Id] = link;

            Console.WriteLine($"[CausalMemory] Created causal link: {causeNodeId} → {effectNodeId}");

            return link.Id;
        }

        /// <summary>
        /// Strengthen a causal link when confirmed
        /// </summary>
        public void ReinforceLink(string linkId, double successMagnitude = 1.0)
        {
            if (_links.TryGetValue(linkId, out var link))
            {
                link.ObservationCount++;
                link.Strength = Math.Min(1.0, link.Strength + (0.05 * successMagnitude));
                link.ReliabilityPercent = Math.Min(100, link.ReliabilityPercent + (2 * successMagnitude));

                Console.WriteLine($"[CausalMemory] Reinforced link {linkId}: strength={link.Strength:F2}, reliability={link.ReliabilityPercent:F1}%");
            }
        }

        /// <summary>
        /// Weaken a causal link when contradicted
        /// </summary>
        public void WeakenLink(string linkId, double failureMagnitude = 1.0)
        {
            if (_links.TryGetValue(linkId, out var link))
            {
                link.Strength = Math.Max(0.1, link.Strength - (0.05 * failureMagnitude));
                link.ReliabilityPercent = Math.Max(10, link.ReliabilityPercent - (3 * failureMagnitude));

                Console.WriteLine($"[CausalMemory] Weakened link {linkId}: strength={link.Strength:F2}, reliability={link.ReliabilityPercent:F1}%");
            }
        }

        /// <summary>
        /// Add an exception to a causal link
        /// e.g., "Normally disable OneDrive for performance, but NOT if user explicitly approved it"
        /// </summary>
        public void AddException(string linkId, string condition, string reason, double confidence = 0.8)
        {
            if (!_links.ContainsKey(linkId)) return;

            var exception = new CausalException
            {
                CausalLinkId = linkId,
                Condition = condition,
                Reason = reason,
                Confidence = confidence
            };

            _exceptions[exception.Id] = exception;

            // Add to the link
            if (_links.TryGetValue(linkId, out var link))
            {
                link.ExceptionIds.Add(exception.Id);
            }

            Console.WriteLine($"[CausalMemory] Added exception to link {linkId}: {condition}");
        }

        /// <summary>
        /// Get all causal chains
        /// </summary>
        public List<CausalChain> GetAllChains()
        {
            return _chains.OrderByDescending(c => c.Confidence * c.VerificationCount).ToList();
        }

        /// <summary>
        /// Get chains of a specific type
        /// </summary>
        public List<CausalChain> GetChainsByType(string chainType)
        {
            return _chains
                .Where(c => c.ChainType.Contains(chainType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.Confidence)
                .ToList();
        }

        /// <summary>
        /// Trace the full causal path from start to end
        /// </summary>
        public List<string> TraceChain(string chainId)
        {
            var chain = _chains.FirstOrDefault(c => c.Id == chainId);
            if (chain == null) return new List<string>();

            var result = new List<string>();

            foreach (var nodeId in chain.NodeIds)
            {
                if (_nodes.TryGetValue(nodeId, out var node))
                {
                    result.Add(node.Event);
                }
            }

            return result;
        }

        /// <summary>
        /// Get all exceptions for a causal link
        /// </summary>
        public List<CausalException> GetExceptionsForLink(string linkId)
        {
            if (!_links.TryGetValue(linkId, out var link))
            {
                return new List<CausalException>();
            }

            return link.ExceptionIds
                .Where(id => _exceptions.ContainsKey(id))
                .Select(id => _exceptions[id])
                .OrderByDescending(e => e.Confidence)
                .ToList();
        }

        /// <summary>
        /// Predict outcomes given a starting event
        /// Uses causal chains to predict what will happen
        /// </summary>
        public Dictionary<string, object> PredictOutcome(string startingEventName)
        {
            var relevantChains = _chains
                .Where(c => c.NodeIds.Any(nid => _nodes[nid].Event.Contains(startingEventName, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(c => c.Confidence)
                .ToList();

            if (!relevantChains.Any())
            {
                return new Dictionary<string, object> { { "ChainsFound", 0 } };
            }

            var topChain = relevantChains.First();
            var outcomes = TraceChain(topChain.Id);

            return new Dictionary<string, object>
            {
                { "PredictedOutcome", outcomes.LastOrDefault() },
                { "FullChain", outcomes },
                { "ChainConfidence", topChain.Confidence },
                { "ChainVerifications", topChain.VerificationCount },
                { "AlternativeOutcomes", relevantChains.Skip(1).Select(c => c.ChainType).ToList() }
            };
        }

        /// <summary>
        /// Get all causal links
        /// </summary>
        public List<CausalLink> GetAllLinks()
        {
            return _links.Values
                .OrderByDescending(l => l.Strength * l.ReliabilityPercent)
                .ToList();
        }

        /// <summary>
        /// Get strongest causal relationships
        /// </summary>
        public List<(string Cause, string Effect, double Strength)> GetStrongestRelationships(int count = 10)
        {
            return _links.Values
                .OrderByDescending(l => l.Strength * (l.ReliabilityPercent / 100.0))
                .Take(count)
                .Select(l => (
                    _nodes[l.CauseNodeId].Event,
                    _nodes[l.EffectNodeId].Event,
                    l.Strength
                ))
                .ToList();
        }

        /// <summary>
        /// Get statistics about causal understanding
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "TotalNodes", _nodes.Count },
                { "TotalLinks", _links.Count },
                { "TotalExceptions", _exceptions.Count },
                { "TotalChains", _chains.Count },
                { "AverageLinkStrength", _links.Values.Average(l => l.Strength) },
                { "AverageChainConfidence", _chains.Average(c => c.Confidence) },
                { "StrongestChain", _chains.OrderByDescending(c => c.Confidence).FirstOrDefault()?.ChainType },
                { "ChainTypes", _chains.Select(c => c.ChainType).Distinct().ToList() }
            };
        }
    }
}

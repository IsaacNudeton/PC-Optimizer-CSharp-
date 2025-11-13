using System;
using System.Collections.Generic;
using System.Linq;

namespace PCOptimizer.Services.AI.Memory
{
    /// <summary>
    /// Universal Memory System - Orchestrator for all three memory types
    ///
    /// This is the biological brain of Universe K.I.D.
    /// It coordinates Semantic, Episodic, and Causal memory to create a unified understanding.
    ///
    /// Like a human brain:
    /// - Semantic Memory = Facts we know
    /// - Episodic Memory = Experiences we remember
    /// - Causal Memory = Understanding why things work
    ///
    /// Together they create wisdom - the ability to apply knowledge in new situations.
    /// </summary>
    public class UniversalMemorySystem
    {
        private readonly SemanticMemory _semanticMemory;
        private readonly EpisodicMemory _episodicMemory;
        private readonly CausalMemory _causalMemory;

        /// <summary>
        /// Consolidation happens when learning moves from short-term to long-term storage
        /// Like sleeping consolidates memories in humans
        /// </summary>
        private class MemoryConsolidation
        {
            public string Id { get; set; }
            public string SourceMemoryType { get; set; }
            public string Content { get; set; }
            public double StrengthGain { get; set; }
            public DateTime ConsolidatedAt { get; set; }

            public MemoryConsolidation()
            {
                Id = Guid.NewGuid().ToString();
                ConsolidatedAt = DateTime.UtcNow;
            }
        }

        private readonly List<MemoryConsolidation> _consolidations;
        private int _learningCycleCount = 0;

        public UniversalMemorySystem()
        {
            _semanticMemory = new SemanticMemory();
            _episodicMemory = new EpisodicMemory();
            _causalMemory = new CausalMemory();
            _consolidations = new List<MemoryConsolidation>();

            Console.WriteLine("[UniversalMemorySystem] Initialized with Semantic, Episodic, and Causal memories");
        }

        /// <summary>
        /// Learn from a new experience
        /// This is the core learning function that integrates all memory types
        /// </summary>
        public void LearnFromExperience(EpisodicMemory.Episode episode)
        {
            Console.WriteLine($"\n[UniversalMemorySystem] Learning from experience: {episode.Context}");

            // Step 1: Record the episode
            _episodicMemory.RecordEpisode(episode);

            // Step 2: Extract semantic facts from this episode
            ExtractSemanticFactsFromEpisode(episode);

            // Step 3: Update causal understanding based on outcome
            UpdateCausalUnderstanding(episode);

            // Step 4: Reinforce or weaken related facts based on success
            ReinforceLearning(episode);

            _learningCycleCount++;
            Console.WriteLine($"[UniversalMemorySystem] Learning cycle #{_learningCycleCount} complete");
        }

        /// <summary>
        /// Extract semantic facts from an episodic experience
        /// e.g., if we successfully optimized for Valorant by disabling SearchIndexer,
        /// we learn: "Disabling SearchIndexer improves gaming performance"
        /// </summary>
        private void ExtractSemanticFactsFromEpisode(EpisodicMemory.Episode episode)
        {
            // If this was a successful episode, extract learnings
            if (episode.Significance >= EpisodicMemory.EmotionalSignificance.Positive)
            {
                foreach (var action in episode.Actions)
                {
                    // Create a fact about this action leading to success
                    var fact = new SemanticMemory.SemanticFact
                    {
                        Fact = $"{action} leads to optimization success",
                        Category = episode.Tags.FirstOrDefault() ?? "General",
                        SynapticWeight = Math.Min(1.0, 0.5 + (int)episode.Significance * 0.1),
                        Confidence = episode.Confidence,
                        ReinforcementCount = 1,
                        Parameters = new Dictionary<string, double>(episode.Metrics.Select(m =>
                            new KeyValuePair<string, double>(m.Key, m.Value.After - m.Value.Before)).ToDictionary(x => x.Key, x => x.Value))
                    };

                    _semanticMemory.AddFact(fact);
                }
            }
        }

        /// <summary>
        /// Update causal understanding based on new experience
        /// If we tried something and it worked, strengthen the causal links
        /// </summary>
        private void UpdateCausalUnderstanding(EpisodicMemory.Episode episode)
        {
            // For each action in the episode, find related causal chains
            foreach (var action in episode.Actions)
            {
                var relevantChains = _causalMemory.GetAllChains()
                    .Where(c => c.ChainType.Contains(episode.Tags.FirstOrDefault() ?? "", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var chain in relevantChains)
                {
                    if (episode.Significance >= EpisodicMemory.EmotionalSignificance.Positive)
                    {
                        // Success - strengthen causal understanding
                        foreach (var link in _causalMemory.GetAllLinks())
                        {
                            _causalMemory.ReinforceLink(link.Id, (double)episode.Significance);
                        }
                    }
                    else if (episode.Significance <= EpisodicMemory.EmotionalSignificance.Negative)
                    {
                        // Failure - weaken causal understanding
                        foreach (var link in _causalMemory.GetAllLinks())
                        {
                            _causalMemory.WeakenLink(link.Id, Math.Abs((double)episode.Significance));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reinforce or weaken facts based on episode outcome
        /// Hebbian learning: "Neurons that fire together, wire together"
        /// </summary>
        private void ReinforceLearning(EpisodicMemory.Episode episode)
        {
            var allFacts = _semanticMemory.GetAllFacts();

            foreach (var fact in allFacts)
            {
                // If fact is in the category of this episode and episode was successful
                if (episode.Tags.Contains(fact.Category) && episode.Significance >= EpisodicMemory.EmotionalSignificance.Positive)
                {
                    _semanticMemory.ReinforceFactById(fact.Id, (double)episode.Significance);
                }

                // If episode failed, weaken related facts
                if (episode.Tags.Contains(fact.Category) && episode.Significance <= EpisodicMemory.EmotionalSignificance.Negative)
                {
                    _semanticMemory.WeakenFactById(fact.Id, Math.Abs((double)episode.Significance));
                }
            }
        }

        /// <summary>
        /// Recall information relevant to a situation
        /// Integrates knowledge from all three memory systems
        /// </summary>
        public Dictionary<string, object> RecallRelevantKnowledge(string context, string tags = "")
        {
            var result = new Dictionary<string, object>();

            // Semantic recall - what do we know about this?
            var relevantFacts = _semanticMemory.GetAllFacts()
                .Where(f => f.Fact.Contains(context, StringComparison.OrdinalIgnoreCase) ||
                           f.Category.Contains(tags, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.SynapticWeight * f.Confidence)
                .Take(5)
                .ToList();

            result["SemanticFacts"] = relevantFacts.Select(f => new { f.Fact, f.SynapticWeight, f.Confidence }).ToList();

            // Episodic recall - what have we experienced like this?
            var relevantEpisodes = _episodicMemory.GetEpisodesByTag(tags)
                .Where(e => e.Context.Contains(context, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => Math.Abs((int)e.Significance))
                .Take(3)
                .ToList();

            result["EpisodicExperiences"] = relevantEpisodes.Select(e => new { e.Context, e.Outcome, e.Significance }).ToList();

            // Causal recall - why do we think things work this way?
            var causalExplanations = _causalMemory.PredictOutcome(context);
            result["CausalChain"] = causalExplanations;

            // Combine for integrated understanding
            var confidenceScore = (relevantFacts.Average(f => f.Confidence) +
                                  relevantEpisodes.Average(e => e.Confidence) +
                                  (causalExplanations.ContainsKey("ChainConfidence") ? (double)causalExplanations["ChainConfidence"] : 0.5)) / 3.0;

            result["IntegratedConfidence"] = confidenceScore;
            result["ReadyToAct"] = confidenceScore > 0.6;

            return result;
        }

        /// <summary>
        /// Get a recommendation based on all memory systems
        /// </summary>
        public Dictionary<string, object> GetRecommendation(string situation, string tags = "Gaming")
        {
            var knowledge = RecallRelevantKnowledge(situation, tags);

            var facts = (List<dynamic>)knowledge["SemanticFacts"];
            var causalChain = (Dictionary<string, object>)knowledge["CausalChain"];
            var readyToAct = (bool)knowledge["ReadyToAct"];

            var recommendation = new Dictionary<string, object>
            {
                { "Situation", situation },
                { "Confidence", knowledge["IntegratedConfidence"] },
                { "ReadyToAct", readyToAct },
                { "RecommendedActions", facts.Select(f => f.Fact).ToList() },
                { "ExpectedOutcome", causalChain.ContainsKey("PredictedOutcome") ? causalChain["PredictedOutcome"] : "Unknown" },
                { "Reasoning", $"Based on {facts.Count} semantic facts, {(List<dynamic>)knowledge["EpisodicExperiences"]}.Count episodic memories" }
            };

            return recommendation;
        }

        /// <summary>
        /// Memory consolidation - strengthen important memories and prune weak ones
        /// Like sleep consolidation in biological brains
        /// </summary>
        public void ConsolidateMemories()
        {
            Console.WriteLine("\n[UniversalMemorySystem] Starting memory consolidation cycle...");

            var consolidation = new MemoryConsolidation
            {
                SourceMemoryType = "All",
                Content = "Consolidating all memory systems"
            };

            // Strengthen strong facts, weaken weak facts
            var strongFacts = _semanticMemory.GetStrongestConnections(10);
            foreach (var fact in strongFacts)
            {
                _semanticMemory.ReinforceFactById(fact.Id, 0.1);
            }

            // Consolidate episodic memory - extract lessons from recent successes
            var recentSuccesses = _episodicMemory.GetSuccesfulEpisodes().Take(5).ToList();
            foreach (var episode in recentSuccesses)
            {
                ExtractSemanticFactsFromEpisode(episode);
            }

            // Learn from failures to avoid repeating them
            var recentFailures = _episodicMemory.GetFailedEpisodes().Take(3).ToList();
            foreach (var episode in recentFailures)
            {
                Console.WriteLine($"[UniversalMemorySystem] Learning from failure: {episode.Context}");
                // Strengthen the memory that this approach doesn't work
            }

            consolidation.StrengthGain = 0.15;
            _consolidations.Add(consolidation);

            Console.WriteLine("[UniversalMemorySystem] Memory consolidation complete");
        }

        /// <summary>
        /// Get comprehensive statistics about all memories
        /// </summary>
        public Dictionary<string, object> GetMemoryStatistics()
        {
            return new Dictionary<string, object>
            {
                { "SemanticMemory", _semanticMemory.GetStatistics() },
                { "EpisodicMemory", _episodicMemory.GetLearningProgression() },
                { "CausalMemory", _causalMemory.GetStatistics() },
                { "LearningCycles", _learningCycleCount },
                { "ConsolidationCount", _consolidations.Count },
                { "OverallHealth", CalculateMemoryHealth() }
            };
        }

        /// <summary>
        /// Calculate overall health of the memory system
        /// </summary>
        private Dictionary<string, object> CalculateMemoryHealth()
        {
            var semanticStats = _semanticMemory.GetStatistics();
            var episodicStats = _episodicMemory.GetLearningProgression();
            var causalStats = _causalMemory.GetStatistics();

            var avgSynapticWeight = (double)semanticStats["AverageSynapticWeight"];
            var successRate = (double)episodicStats["OverallSuccessRate"];
            var avgChainConfidence = (double)causalStats["AverageChainConfidence"];

            var overallHealth = (avgSynapticWeight + successRate + avgChainConfidence) / 3.0;

            return new Dictionary<string, object>
            {
                { "Score", Math.Round(overallHealth, 3) },
                { "Status", overallHealth > 0.7 ? "Healthy" : overallHealth > 0.5 ? "Learning" : "Developing" },
                { "SemanticHealth", Math.Round(avgSynapticWeight, 3) },
                { "EpisodicHealth", Math.Round(successRate, 3) },
                { "CausalHealth", Math.Round(avgChainConfidence, 3) }
            };
        }

        /// <summary>
        /// Link a semantic fact to a causal chain (cross-memory association)
        /// This is like making connections between different types of knowledge
        /// </summary>
        public void LinkFactToChain(SemanticMemory.SemanticFact fact, CausalMemory.CausalChain chain)
        {
            // Future: Create an association database linking semantic facts to causal chains
            Console.WriteLine($"[UniversalMemorySystem] Linked fact '{fact.Fact}' to causal chain '{chain.ChainType}'");
        }

        /// <summary>
        /// Export all memory for analysis or debugging
        /// </summary>
        public Dictionary<string, object> ExportAllMemories()
        {
            return new Dictionary<string, object>
            {
                { "SemanticFacts", _semanticMemory.GetAllFacts() },
                { "Episodes", _episodicMemory.GetAllEpisodes() },
                { "CausalChains", _causalMemory.GetAllChains() },
                { "CausalLinks", _causalMemory.GetAllLinks() },
                { "LearningCycles", _learningCycleCount },
                { "ExportedAt", DateTime.UtcNow }
            };
        }

        /// <summary>
        /// Get the "state of mind" - what the system is currently thinking about
        /// </summary>
        public Dictionary<string, object> GetStateOfMind()
        {
            return new Dictionary<string, object>
            {
                { "StrongestKnowledge", _semanticMemory.GetStrongestConnections(3).Select(f => f.Fact).ToList() },
                { "RecentLessons", _episodicMemory.GetRecentEpisodes(3).Select(e => e.Outcome).ToList() },
                { "ActiveChains", _causalMemory.GetAllChains().Take(3).Select(c => c.ChainType).ToList() },
                { "LearningTrend", _episodicMemory.GetLearningProgression()["Trend"] },
                { "Memory", GetMemoryStatistics() }
            };
        }
    }
}

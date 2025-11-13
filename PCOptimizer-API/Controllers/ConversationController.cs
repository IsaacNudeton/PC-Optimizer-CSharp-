using Microsoft.AspNetCore.Mvc;
using PCOptimizer.Services;

namespace PCOptimizer.Controllers
{
    /// <summary>
    /// API endpoints for conversation logging and context tracking
    /// All conversations are linked to monitoring snapshots for context awareness
    /// </summary>
    [ApiController]
    [Route("api/conversations")]
    public class ConversationController : ControllerBase
    {
        private readonly ConversationLogger _conversationLogger;
        private readonly BehaviorMonitor _behaviorMonitor;

        public ConversationController(ConversationLogger conversationLogger, BehaviorMonitor behaviorMonitor)
        {
            _conversationLogger = conversationLogger;
            _behaviorMonitor = behaviorMonitor;
        }

        /// <summary>
        /// Log a conversation interaction with current system context
        /// </summary>
        /// <remarks>
        /// Captures current system state snapshot and links it to conversation semantics
        /// This creates full context: what the user was doing when they asked
        /// </remarks>
        [HttpPost("log")]
        public IActionResult LogConversation([FromBody] LogConversationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserQuery) || string.IsNullOrEmpty(request.ClaudeResponse))
            {
                return BadRequest(new { error = "userQuery and claudeResponse are required" });
            }

            try
            {
                // Capture current system state
                var snapshot = _behaviorMonitor.CaptureSnapshot();

                // Log conversation linked to snapshot
                var entry = _conversationLogger.LogConversation(
                    request.UserQuery,
                    request.ClaudeResponse,
                    snapshot,
                    request.Topic ?? "General",
                    request.Intent ?? "Learn"
                );

                return Ok(new
                {
                    id = entry.Id,
                    snapshotId = entry.SnapshotId,
                    timestamp = entry.Timestamp,
                    topic = entry.Topic,
                    intent = entry.Intent,
                    category = entry.Category,
                    keywords = entry.Keywords,
                    researchAreas = entry.ResearchAreas,
                    problemsDiscussed = entry.ProblemsDiscussed,
                    solutionsSuggested = entry.SolutionsSuggested,
                    activeProcesses = entry.ActiveProcesses,
                    currentFocus = entry.CurrentFocus,
                    confidenceScore = entry.ConfidenceScore,
                    outcome = entry.Outcome
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error logging conversation: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get conversation history (recent conversations)
        /// </summary>
        [HttpGet("history")]
        public IActionResult GetHistory([FromQuery] int last = 100)
        {
            try
            {
                var conversations = _conversationLogger.GetConversationHistory(last);
                return Ok(new
                {
                    count = conversations.Count,
                    conversations = conversations.Select(c => new
                    {
                        id = c.Id,
                        snapshotId = c.SnapshotId,
                        timestamp = c.Timestamp,
                        topic = c.Topic,
                        intent = c.Intent,
                        category = c.Category,
                        keywords = c.Keywords,
                        outcome = c.Outcome,
                        confidenceScore = c.ConfidenceScore
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error retrieving history: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get conversations by topic
        /// </summary>
        [HttpGet("by-topic/{topic}")]
        public IActionResult GetByTopic(string topic)
        {
            try
            {
                var conversations = _conversationLogger.GetConversationsByTopic(topic);
                return Ok(new
                {
                    topic,
                    count = conversations.Count,
                    conversations = conversations.Select(c => new
                    {
                        id = c.Id,
                        snapshotId = c.SnapshotId,
                        timestamp = c.Timestamp,
                        keywords = c.Keywords,
                        outcome = c.Outcome
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error retrieving conversations: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get conversations by research area
        /// </summary>
        [HttpGet("by-area/{area}")]
        public IActionResult GetByResearchArea(string area)
        {
            try
            {
                var conversations = _conversationLogger.GetConversationsByResearchArea(area);
                return Ok(new
                {
                    area,
                    count = conversations.Count,
                    conversations = conversations.Select(c => new
                    {
                        id = c.Id,
                        snapshotId = c.SnapshotId,
                        timestamp = c.Timestamp,
                        keywords = c.Keywords,
                        researchAreas = c.ResearchAreas
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error retrieving conversations: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get conversation summary statistics
        /// </summary>
        [HttpGet("summary")]
        public IActionResult GetSummary([FromQuery] int last = 100)
        {
            try
            {
                var summary = _conversationLogger.GetConversationSummary(last);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error retrieving summary: {ex.Message}" });
            }
        }

        /// <summary>
        /// Link a conversation to an outcome (e.g., code implementation)
        /// </summary>
        [HttpPost("{conversationId}/outcome")]
        public IActionResult LinkOutcome(string conversationId, [FromBody] LinkOutcomeRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Outcome))
            {
                return BadRequest(new { error = "outcome is required" });
            }

            try
            {
                _conversationLogger.LinkOutcome(
                    conversationId,
                    request.Outcome,
                    request.ChangedFiles ?? new List<string>()
                );

                return Ok(new { message = $"Linked outcome '{request.Outcome}' to conversation {conversationId}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error linking outcome: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// Request model for logging conversations
    /// </summary>
    public class LogConversationRequest
    {
        public string UserQuery { get; set; } = string.Empty;
        public string ClaudeResponse { get; set; } = string.Empty;
        public string? Topic { get; set; } = "General";
        public string? Intent { get; set; } = "Learn";
    }

    /// <summary>
    /// Request model for linking outcomes
    /// </summary>
    public class LinkOutcomeRequest
    {
        public string Outcome { get; set; } = string.Empty;  // pending, implemented, researched, abandoned
        public List<string>? ChangedFiles { get; set; }
    }
}

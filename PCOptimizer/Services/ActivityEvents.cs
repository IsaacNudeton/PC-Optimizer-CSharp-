using System;
using System.Collections.Generic;

namespace PCOptimizer.Services
{
    /// <summary>
    /// User Activity Event - Tracks keyboard, mouse, and window focus patterns
    /// Captures human interaction metrics (not content, just patterns and timing)
    /// </summary>
    public class UserActivityEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Window focus tracking
        public string? ActiveWindowTitle { get; set; }
        public string? ActiveWindowProcess { get; set; }
        public TimeSpan FocusTimeOnCurrentWindow { get; set; }  // How long user focused on current window
        public int WindowFocusSwitches { get; set; }  // Number of window switches in period

        // Keyboard activity (pattern, no content)
        public int KeyPressesPerMinute { get; set; }  // Keystroke density
        public double AverageTimePerKeyPress { get; set; }  // Milliseconds
        public int BackspacesPerMinute { get; set; }  // Correction frequency
        public TimeSpan KeyboardIdleTime { get; set; }  // Idle between keystrokes

        // Mouse activity (movement and clicks)
        public int MouseClicksPerMinute { get; set; }  // Click frequency
        public double MouseMovementDistance { get; set; }  // Pixels moved
        public int MouseWheelScrolls { get; set; }  // Scroll events
        public TimeSpan MouseIdleTime { get; set; }  // Idle time since last activity

        // Overall attention
        public double AttentionScore { get; set; }  // 0-1, based on consistency and intensity
        public string AttentionLevel { get; set; } = "Normal";  // Focused, Normal, Distracted, Idle

        // Application context
        public List<string> RecentlyActiveApplications { get; set; } = new();  // Last 5 apps
        public string PrimaryActivityCategory { get; set; } = "Unknown";  // Gaming, Development, Browsing, etc.
    }

    /// <summary>
    /// AI Tool Event - Tracks interactions with AI models (Cursor, Claude, etc.)
    /// The "gold" data for training - captures how user interacts with AI assistance
    /// </summary>
    public class AIToolEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Tool identification
        public string ToolName { get; set; } = string.Empty;  // "Cursor", "Claude", "ChatGPT", etc.
        public string? ToolWindowTitle { get; set; }
        public string ToolVersion { get; set; } = string.Empty;

        // Cursor IDE specific
        public int? CursorCompletionsSuggested { get; set; }
        public int? CursorCompletionsAccepted { get; set; }  // How many AI suggestions user accepted
        public int? CursorCompletionsRejected { get; set; }  // How many user rejected
        public double? CursorAcceptanceRate { get; set; }  // 0-1, how often user accepts completions
        public int? CursorFileEditsWithAI { get; set; }  // Files edited with AI assistance
        public List<string>? CursorEditedFileTypes { get; set; }  // .cs, .tsx, .json, etc.

        // Claude / Terminal interactions
        public int? ClaudePromptsIssued { get; set; }  // Number of prompts sent
        public int? ClaudeResponsesReceived { get; set; }
        public double? ClaudeAverageResponseTime { get; set; }  // Seconds
        public int? ClaudeResponsesModifiedByUser { get; set; }  // Did user edit response before using?
        public bool? ClaudeDirectlyUsedResponse { get; set; }  // True if used without edits
        public List<string>? ClaudeTopics { get; set; }  // What was Claude asked about? (generalized, no secrets)

        // General AI metrics
        public int? WebSearchesPerformed { get; set; }  // User searches while interacting with AI
        public int? ExternalAPICallsMade { get; set; }  // To other models/services
        public double? AveragePromptLength { get; set; }  // Character count
        public List<string>? GeneralizedPromptPatterns { get; set; }  // "Asking for code review", "Debugging", "Learning", etc.

        // Satisfaction indicators
        public double SatisfactionScore { get; set; }  // 0-1, based on behavior
        public int? FollowupQuestions { get; set; }  // Did user ask follow-ups? (indicates satisfaction/clarification need)
        public TimeSpan? TimeToFirstEdit { get; set; }  // How quickly did user edit AI response?
        public bool? UserSeemedSatisfied { get; set; }  // Inferred from usage pattern
    }

    /// <summary>
    /// Inter-AI Event - Tracks how multiple AI tools work together
    /// Captures meta-learning: how does user orchestrate different AI models?
    /// </summary>
    public class InterAIEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Tool coordination
        public List<string> ActiveAITools { get; set; } = new();  // Which tools running simultaneously
        public int ActiveToolsCount { get; set; }  // How many AI tools active at once (capacity measure)
        public int ToolSwitchFrequency { get; set; }  // How often does user switch between tools per minute

        // Data flow between tools
        public List<string> ToolInteractionPattern { get; set; } = new();  // Sequence of tool usage: ["Cursor", "Claude", "WebSearch", ...]
        public bool DataFlowFromCursorToClaude { get; set; }  // Did user copy code from Cursor to Claude?
        public bool DataFlowFromClaudeToCursor { get; set; }  // Did user paste Claude response into Cursor?
        public Dictionary<string, int> ToolUsageForTask { get; set; } = new();  // {"Gaming": 5, "Development": 10}

        // Sequencing patterns
        public string PrimaryWorkflow { get; set; } = "Unknown";  // e.g., "Code→Claude→Review", "Search→Cursor→Implement"
        public int WorkflowCompletionCount { get; set; }  // How many times did user complete full workflow
        public TimeSpan AverageToolSwitchTime { get; set; }  // How quickly does user switch between tools?

        // Coordination efficiency
        public double CoordinationScore { get; set; }  // 0-1, how well tools are orchestrated
        public bool AllToolsUsedForSameTask { get; set; }  // Did all tools contribute to one goal?
        public List<string> ToolCombinations { get; set; } = new();  // ["Cursor+Claude", "Claude+WebSearch", ...]
    }

    /// <summary>
    /// Feedback Event - Tracks user satisfaction and validation signals
    /// Critical for training: tells AI whether its suggestions were helpful
    /// </summary>
    public class FeedbackEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Feedback type
        public string SourceTool { get; set; } = string.Empty;  // Which AI tool generated the suggestion
        public string SuggestionType { get; set; } = string.Empty;  // "CodeCompletion", "Optimization", "Explanation", etc.

        // User response
        public bool UserAccepted { get; set; }  // True if user accepted the suggestion as-is
        public bool UserRejected { get; set; }  // True if user explicitly rejected
        public bool UserModified { get; set; }  // True if user modified before using
        public int ModificationCount { get; set; }  // How many edits before user was satisfied
        public double? EditDistance { get; set; }  // Levenshtein or character count difference

        // Timing (indicates deliberation vs. rushed)
        public TimeSpan TimeToAcceptance { get; set; }  // How quickly did user make decision?
        public bool QuickAcceptance { get; set; }  // < 500ms = probably trusted suggestion
        public bool DeliberateDecision { get; set; }  // > 5000ms = user reviewed carefully

        // Quality indicators
        public int? SatisfactionRating { get; set; }  // 1-5 if user provided explicit rating
        public bool SuccessfulOutcome { get; set; }  // Inferred: did the code run, optimization work, etc.?
        public string? UserFeedbackComment { get; set; }  // Free-form feedback (rare but valuable)

        // Learning value
        public double ImportanceForTraining { get; set; }  // 0-1, high if user learned or changed behavior
        public bool RareOrNovelSituation { get; set; }  // Did this break the AI's typical pattern?
        public List<string> ContextTags { get; set; } = new();  // ["Gaming", "Optimization", "FirstTime", "Expert", "Beginner"]
    }

    /// <summary>
    /// Training Event - Metadata about when/how the AI model learned
    /// Links raw events to learning outcomes
    /// </summary>
    public class TrainingEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string EventType { get; set; } = string.Empty;  // "UserActivity", "AITool", "InterAI", "Feedback"
        public string SourceEventId { get; set; } = string.Empty;  // Reference to original event

        public string LearningPhase { get; set; } = string.Empty;  // "Imprinting", "Experience", "Consolidation"
        public double LearningIntensity { get; set; } = 0.5;  // 0-1, how important was this event

        public string RelatedMemoryType { get; set; } = string.Empty;  // "Semantic", "Episodic", "Causal"
        public Dictionary<string, object> ExtractedInsight { get; set; } = new();  // What the system learned

        public bool IsProcessedForTraining { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}

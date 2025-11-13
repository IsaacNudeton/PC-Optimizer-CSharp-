using System;

namespace PCOptimizer.Services
{
    /// <summary>
    /// Represents a labeled activity snapshot for ML training
    /// </summary>
    public class ActivityLabelResult
    {
        public string SnapshotId { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EmotionLabel { get; set; } = "";
        public string ActivityLabel { get; set; } = "";
        public string LearningModeLabel { get; set; } = "";
        public string CognitiveLoadLabel { get; set; } = "";
        public string IntentLabel { get; set; } = "";
        public string HealthStateLabel { get; set; } = "";
        public string Notes { get; set; } = "";
        public bool UserProvided { get; set; } = false;
        public double Confidence { get; set; } = 1.0;
        
        public ActivityLabelResult()
        {
        }
        
        /// <summary>
        /// Create label from auto-detected activity category
        /// </summary>
        public static ActivityLabelResult FromCategory(string category, double confidence = 0.8)
        {
            return new ActivityLabelResult
            {
                ActivityLabel = category,
                UserProvided = false,
                Confidence = confidence,
                Notes = $"Auto-detected from process/window analysis"
            };
        }
        
        /// <summary>
        /// Create label with user-provided emotion
        /// </summary>
        public static ActivityLabelResult WithEmotion(string emotion, string activity)
        {
            return new ActivityLabelResult
            {
                EmotionLabel = emotion,
                ActivityLabel = activity,
                UserProvided = true,
                Confidence = 1.0
            };
        }
    }
}

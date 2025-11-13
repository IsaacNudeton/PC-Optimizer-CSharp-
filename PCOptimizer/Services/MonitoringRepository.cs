using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace PCOptimizer.Services
{
    /// <summary>
    /// Persists activity snapshots to SQLite database
    /// Minimal footprint, no ORM overhead
    /// </summary>
    public class MonitoringRepository
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly object _lockObject = new();

        public MonitoringRepository()
        {
            // Store in AppData\Local\PCOptimizer\monitoring.db
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PCOptimizer");

            Directory.CreateDirectory(appDataPath);
            _dbPath = Path.Combine(appDataPath, "monitoring.db");
            _connectionString = $"Data Source={_dbPath};Cache=Shared";

            InitializeDatabase();
        }

        /// <summary>
        /// Initialize database schema if it doesn't exist
        /// </summary>
        private void InitializeDatabase()
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                CREATE TABLE IF NOT EXISTS ActivitySnapshots (
                                    SnapshotId TEXT PRIMARY KEY,
                                    Timestamp TEXT NOT NULL,
                                    Category TEXT NOT NULL,
                                    CPUUsageGlobal REAL NOT NULL,
                                    MemoryUsageGlobal REAL NOT NULL,
                                    GPUUsageGlobal REAL,
                                    GPUTempCelsius REAL,
                                    CPUTempCelsius REAL,
                                    ActiveWindowTitle TEXT,
                                    ActiveWindowProcess TEXT,
                                    ProcessesJson TEXT NOT NULL,
                                    BrowserActivityJson TEXT,
                                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                                );

                                CREATE INDEX IF NOT EXISTS idx_timestamp ON ActivitySnapshots(Timestamp);
                                CREATE INDEX IF NOT EXISTS idx_category ON ActivitySnapshots(Category);
                                CREATE INDEX IF NOT EXISTS idx_created ON ActivitySnapshots(CreatedAt);

                                CREATE TABLE IF NOT EXISTS UserActivityEvents (
                                    EventId TEXT PRIMARY KEY,
                                    Timestamp TEXT NOT NULL,
                                    ActiveWindowTitle TEXT,
                                    ActiveWindowProcess TEXT,
                                    FocusTimeSeconds INTEGER,
                                    WindowSwitches INTEGER,
                                    KeyPressesPerMinute INTEGER,
                                    MouseClicksPerMinute INTEGER,
                                    AttentionScore REAL,
                                    AttentionLevel TEXT,
                                    PrimaryActivityCategory TEXT,
                                    EventJson TEXT NOT NULL,
                                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                                );

                                CREATE INDEX IF NOT EXISTS idx_user_timestamp ON UserActivityEvents(Timestamp);
                                CREATE INDEX IF NOT EXISTS idx_user_window ON UserActivityEvents(ActiveWindowProcess);

                                CREATE TABLE IF NOT EXISTS AIToolEvents (
                                    EventId TEXT PRIMARY KEY,
                                    Timestamp TEXT NOT NULL,
                                    ToolName TEXT NOT NULL,
                                    CursorAcceptanceRate REAL,
                                    ClaudePromptsCount INTEGER,
                                    ClaudeDirectlyUsed INTEGER,
                                    SatisfactionScore REAL,
                                    FollowupQuestionsCount INTEGER,
                                    EventJson TEXT NOT NULL,
                                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                                );

                                CREATE INDEX IF NOT EXISTS idx_ai_timestamp ON AIToolEvents(Timestamp);
                                CREATE INDEX IF NOT EXISTS idx_ai_tool ON AIToolEvents(ToolName);

                                CREATE TABLE IF NOT EXISTS InterAIEvents (
                                    EventId TEXT PRIMARY KEY,
                                    Timestamp TEXT NOT NULL,
                                    ActiveToolsCount INTEGER,
                                    ToolSwitchFrequency INTEGER,
                                    PrimaryWorkflow TEXT,
                                    CoordinationScore REAL,
                                    EventJson TEXT NOT NULL,
                                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                                );

                                CREATE INDEX IF NOT EXISTS idx_interai_timestamp ON InterAIEvents(Timestamp);

                                CREATE TABLE IF NOT EXISTS FeedbackEvents (
                                    EventId TEXT PRIMARY KEY,
                                    Timestamp TEXT NOT NULL,
                                    SourceTool TEXT NOT NULL,
                                    SuggestionType TEXT,
                                    UserAccepted INTEGER,
                                    UserRejected INTEGER,
                                    UserModified INTEGER,
                                    TimeToAcceptanceMs INTEGER,
                                    SatisfactionRating INTEGER,
                                    SuccessfulOutcome INTEGER,
                                    ImportanceForTraining REAL,
                                    EventJson TEXT NOT NULL,
                                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                                );

                                CREATE INDEX IF NOT EXISTS idx_feedback_timestamp ON FeedbackEvents(Timestamp);
                                CREATE INDEX IF NOT EXISTS idx_feedback_tool ON FeedbackEvents(SourceTool);

                                CREATE TABLE IF NOT EXISTS ActivityLabels (
                                    LabelId TEXT PRIMARY KEY,
                                    SnapshotId TEXT NOT NULL,
                                    Timestamp TEXT NOT NULL,
                                    EmotionLabel TEXT,
                                    ActivityLabel TEXT,
                                    LearningModeLabel TEXT,
                                    CognitiveLoadLabel TEXT,
                                    IntentLabel TEXT,
                                    HealthStateLabel TEXT,
                                    Notes TEXT,
                                    UserProvided INTEGER DEFAULT 1,
                                    Confidence REAL,
                                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                                    FOREIGN KEY (SnapshotId) REFERENCES ActivitySnapshots(SnapshotId)
                                );

                                CREATE INDEX IF NOT EXISTS idx_labels_snapshot ON ActivityLabels(SnapshotId);
                                CREATE INDEX IF NOT EXISTS idx_labels_timestamp ON ActivityLabels(Timestamp);
                                CREATE INDEX IF NOT EXISTS idx_labels_emotion ON ActivityLabels(EmotionLabel);
                                CREATE INDEX IF NOT EXISTS idx_labels_activity ON ActivityLabels(ActivityLabel);
                            ";

                            command.ExecuteNonQuery();
                        }

                        System.Console.WriteLine($"[MonitoringRepository] Database initialized at {_dbPath}");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error initializing database: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Save activity snapshot to database
        /// </summary>
        public void SaveSnapshot(ActivitySnapshot snapshot)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                INSERT INTO ActivitySnapshots (
                                    SnapshotId, Timestamp, Category, CPUUsageGlobal, MemoryUsageGlobal,
                                    GPUUsageGlobal, GPUTempCelsius, CPUTempCelsius,
                                    ActiveWindowTitle, ActiveWindowProcess, ProcessesJson, BrowserActivityJson
                                ) VALUES (
                                    @snapshotId, @timestamp, @category, @cpuUsage, @memoryUsage,
                                    @gpuUsage, @gpuTemp, @cpuTemp,
                                    @windowTitle, @windowProcess, @processesJson, @browserActivityJson
                                )
                            ";

                            command.Parameters.AddWithValue("@snapshotId", snapshot.Id);
                            command.Parameters.AddWithValue("@timestamp", snapshot.Timestamp.ToString("O"));
                            command.Parameters.AddWithValue("@category", snapshot.Category);
                            command.Parameters.AddWithValue("@cpuUsage", snapshot.CPUUsageGlobal);
                            command.Parameters.AddWithValue("@memoryUsage", snapshot.MemoryUsageGlobal);
                            command.Parameters.AddWithValue("@gpuUsage", snapshot.GPUUsageGlobal ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@gpuTemp", snapshot.GPUTempCelsius ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@cpuTemp", snapshot.CPUTempCelsius ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@windowTitle", snapshot.ActiveWindow?.WindowTitle ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@windowProcess", snapshot.ActiveWindow?.ProcessName ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@processesJson",
                                JsonSerializer.Serialize(snapshot.RunningProcesses));
                            command.Parameters.AddWithValue("@browserActivityJson",
                                snapshot.ActiveBrowserContext != null ? JsonSerializer.Serialize(snapshot.ActiveBrowserContext) : (object)DBNull.Value);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error saving snapshot: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get snapshots within date range
        /// </summary>
        public List<ActivitySnapshot> GetSnapshotsByDateRange(DateTime startTime, DateTime endTime)
        {
            var snapshots = new List<ActivitySnapshot>();

            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT SnapshotId, Timestamp, Category, CPUUsageGlobal, MemoryUsageGlobal,
                                       GPUUsageGlobal, GPUTempCelsius, CPUTempCelsius,
                                       ActiveWindowTitle, ActiveWindowProcess, ProcessesJson, BrowserActivityJson
                                FROM ActivitySnapshots
                                WHERE Timestamp BETWEEN @start AND @end
                                ORDER BY Timestamp DESC
                            ";

                            command.Parameters.AddWithValue("@start", startTime.ToString("O"));
                            command.Parameters.AddWithValue("@end", endTime.ToString("O"));

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var snapshot = new ActivitySnapshot
                                    {
                                        Id = reader.GetString(0),
                                        Timestamp = DateTime.Parse(reader.GetString(1)),
                                        Category = reader.GetString(2),
                                        CPUUsageGlobal = reader.GetDouble(3),
                                        MemoryUsageGlobal = reader.GetDouble(4),
                                        GPUUsageGlobal = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                                        GPUTempCelsius = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                                        CPUTempCelsius = reader.IsDBNull(7) ? null : reader.GetDouble(7)
                                    };

                                    if (!reader.IsDBNull(8))
                                    {
                                        snapshot.ActiveWindow = new WindowActivity
                                        {
                                            WindowTitle = reader.GetString(8),
                                            ProcessName = reader.GetString(9),
                                            FocusedAt = snapshot.Timestamp
                                        };
                                    }

                                    if (!reader.IsDBNull(10))
                                    {
                                        var processesJson = reader.GetString(10);
                                        snapshot.RunningProcesses = JsonSerializer.Deserialize<List<ProcessActivity>>(processesJson) ?? new();
                                    }

                                    if (!reader.IsDBNull(11))
                                    {
                                        var browserJson = reader.GetString(11);
                                        snapshot.ActiveBrowserContext = JsonSerializer.Deserialize<BrowserActivity>(browserJson);
                                    }

                                    snapshots.Add(snapshot);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error retrieving snapshots: {ex.Message}");
                }
            }

            return snapshots;
        }

        /// <summary>
        /// Get latest N snapshots
        /// </summary>
        public List<ActivitySnapshot> GetLatestSnapshots(int count = 100)
        {
            return GetSnapshotsByDateRange(
                DateTime.Now.AddDays(-30),
                DateTime.Now
            ).Take(count).ToList();
        }

        /// <summary>
        /// Get activity statistics for date range
        /// </summary>
        public object GetStatistics(DateTime startTime, DateTime endTime)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT
                                    COUNT(*) as TotalSnapshots,
                                    AVG(CPUUsageGlobal) as AvgCPU,
                                    AVG(MemoryUsageGlobal) as AvgMemory,
                                    MAX(CPUUsageGlobal) as MaxCPU,
                                    MAX(MemoryUsageGlobal) as MaxMemory,
                                    AVG(CASE WHEN GPUTempCelsius IS NOT NULL THEN GPUTempCelsius END) as AvgGPUTemp
                                FROM ActivitySnapshots
                                WHERE Timestamp BETWEEN @start AND @end
                            ";

                            command.Parameters.AddWithValue("@start", startTime.ToString("O"));
                            command.Parameters.AddWithValue("@end", endTime.ToString("O"));

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return new
                                    {
                                        totalSnapshots = reader.GetInt64(0),
                                        averageCPU = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                                        averageMemory = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                                        maxCPU = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                                        maxMemory = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                                        averageGPUTemp = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                                        timeRange = new { start = startTime, end = endTime }
                                    };
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error retrieving statistics: {ex.Message}");
                }
            }

            return new { error = "Could not retrieve statistics" };
        }

        /// <summary>
        /// Get category breakdown for date range
        /// </summary>
        public object GetCategoryBreakdown(DateTime startTime, DateTime endTime)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT
                                    Category,
                                    COUNT(*) as Frequency,
                                    AVG(CPUUsageGlobal) as AvgCPU,
                                    AVG(MemoryUsageGlobal) as AvgMemory
                                FROM ActivitySnapshots
                                WHERE Timestamp BETWEEN @start AND @end
                                GROUP BY Category
                                ORDER BY Frequency DESC
                            ";

                            command.Parameters.AddWithValue("@start", startTime.ToString("O"));
                            command.Parameters.AddWithValue("@end", endTime.ToString("O"));

                            var breakdown = new Dictionary<string, object>();

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var category = reader.GetString(0);
                                    var frequency = reader.GetInt64(1);

                                    breakdown[category] = new
                                    {
                                        frequency,
                                        averageCPU = Math.Round(reader.GetDouble(2), 2),
                                        averageMemory = Math.Round(reader.GetDouble(3), 2)
                                    };
                                }
                            }

                            return breakdown;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error retrieving category breakdown: {ex.Message}");
                }
            }

            return new { error = "Could not retrieve category breakdown" };
        }

        /// <summary>
        /// Delete old snapshots (keep last N days)
        /// </summary>
        public void CleanupOldData(int daysToKeep = 30)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                DELETE FROM ActivitySnapshots
                                WHERE CreatedAt < datetime('now', '-' || @days || ' days')
                            ";

                            command.Parameters.AddWithValue("@days", daysToKeep);
                            var deletedRows = command.ExecuteNonQuery();

                            if (deletedRows > 0)
                            {
                                System.Console.WriteLine($"[MonitoringRepository] Cleaned up {deletedRows} old snapshots");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error cleaning up old data: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Save a user activity event
        /// </summary>
        public void SaveUserActivityEvent(UserActivityEvent evt)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                INSERT INTO UserActivityEvents (
                                    EventId, Timestamp, ActiveWindowTitle, ActiveWindowProcess, FocusTimeSeconds,
                                    WindowSwitches, KeyPressesPerMinute, MouseClicksPerMinute,
                                    AttentionScore, AttentionLevel, PrimaryActivityCategory, EventJson
                                ) VALUES (
                                    @eventId, @timestamp, @windowTitle, @windowProcess, @focusTime,
                                    @windowSwitches, @keyPresses, @mouseClicks,
                                    @attentionScore, @attentionLevel, @category, @eventJson
                                )
                            ";

                            command.Parameters.AddWithValue("@eventId", evt.Id);
                            command.Parameters.AddWithValue("@timestamp", evt.Timestamp.ToString("O"));
                            command.Parameters.AddWithValue("@windowTitle", evt.ActiveWindowTitle ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@windowProcess", evt.ActiveWindowProcess ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@focusTime", (int)evt.FocusTimeOnCurrentWindow.TotalSeconds);
                            command.Parameters.AddWithValue("@windowSwitches", evt.WindowFocusSwitches);
                            command.Parameters.AddWithValue("@keyPresses", evt.KeyPressesPerMinute);
                            command.Parameters.AddWithValue("@mouseClicks", evt.MouseClicksPerMinute);
                            command.Parameters.AddWithValue("@attentionScore", evt.AttentionScore);
                            command.Parameters.AddWithValue("@attentionLevel", evt.AttentionLevel);
                            command.Parameters.AddWithValue("@category", evt.PrimaryActivityCategory);
                            command.Parameters.AddWithValue("@eventJson", JsonSerializer.Serialize(evt));

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error saving user activity event: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Save an AI tool event
        /// </summary>
        public void SaveAIToolEvent(AIToolEvent evt)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                INSERT INTO AIToolEvents (
                                    EventId, Timestamp, ToolName, CursorAcceptanceRate,
                                    ClaudePromptsCount, ClaudeDirectlyUsed, SatisfactionScore,
                                    FollowupQuestionsCount, EventJson
                                ) VALUES (
                                    @eventId, @timestamp, @toolName, @acceptanceRate,
                                    @prompts, @directlyUsed, @satisfaction,
                                    @followups, @eventJson
                                )
                            ";

                            command.Parameters.AddWithValue("@eventId", evt.Id);
                            command.Parameters.AddWithValue("@timestamp", evt.Timestamp.ToString("O"));
                            command.Parameters.AddWithValue("@toolName", evt.ToolName);
                            command.Parameters.AddWithValue("@acceptanceRate", evt.CursorAcceptanceRate ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@prompts", evt.ClaudePromptsIssued ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@directlyUsed", evt.ClaudeDirectlyUsedResponse == true ? 1 : 0);
                            command.Parameters.AddWithValue("@satisfaction", evt.SatisfactionScore);
                            command.Parameters.AddWithValue("@followups", evt.FollowupQuestions ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@eventJson", JsonSerializer.Serialize(evt));

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error saving AI tool event: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Save an inter-AI event
        /// </summary>
        public void SaveInterAIEvent(InterAIEvent evt)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                INSERT INTO InterAIEvents (
                                    EventId, Timestamp, ActiveToolsCount, ToolSwitchFrequency,
                                    PrimaryWorkflow, CoordinationScore, EventJson
                                ) VALUES (
                                    @eventId, @timestamp, @toolsCount, @switchFreq,
                                    @workflow, @coordination, @eventJson
                                )
                            ";

                            command.Parameters.AddWithValue("@eventId", evt.Id);
                            command.Parameters.AddWithValue("@timestamp", evt.Timestamp.ToString("O"));
                            command.Parameters.AddWithValue("@toolsCount", evt.ActiveToolsCount);
                            command.Parameters.AddWithValue("@switchFreq", evt.ToolSwitchFrequency);
                            command.Parameters.AddWithValue("@workflow", evt.PrimaryWorkflow);
                            command.Parameters.AddWithValue("@coordination", evt.CoordinationScore);
                            command.Parameters.AddWithValue("@eventJson", JsonSerializer.Serialize(evt));

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error saving inter-AI event: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Save a feedback event
        /// </summary>
        public void SaveFeedbackEvent(FeedbackEvent evt)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                INSERT INTO FeedbackEvents (
                                    EventId, Timestamp, SourceTool, SuggestionType,
                                    UserAccepted, UserRejected, UserModified,
                                    TimeToAcceptanceMs, SatisfactionRating, SuccessfulOutcome,
                                    ImportanceForTraining, EventJson
                                ) VALUES (
                                    @eventId, @timestamp, @tool, @type,
                                    @accepted, @rejected, @modified,
                                    @timeMs, @rating, @success,
                                    @importance, @eventJson
                                )
                            ";

                            command.Parameters.AddWithValue("@eventId", evt.Id);
                            command.Parameters.AddWithValue("@timestamp", evt.Timestamp.ToString("O"));
                            command.Parameters.AddWithValue("@tool", evt.SourceTool);
                            command.Parameters.AddWithValue("@type", evt.SuggestionType ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@accepted", evt.UserAccepted ? 1 : 0);
                            command.Parameters.AddWithValue("@rejected", evt.UserRejected ? 1 : 0);
                            command.Parameters.AddWithValue("@modified", evt.UserModified ? 1 : 0);
                            command.Parameters.AddWithValue("@timeMs", (int)evt.TimeToAcceptance.TotalMilliseconds);
                            command.Parameters.AddWithValue("@rating", evt.SatisfactionRating ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@success", evt.SuccessfulOutcome ? 1 : 0);
                            command.Parameters.AddWithValue("@importance", evt.ImportanceForTraining);
                            command.Parameters.AddWithValue("@eventJson", JsonSerializer.Serialize(evt));

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error saving feedback event: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get AI tool events by tool name
        /// </summary>
        public List<AIToolEvent> GetAIToolEventsByTool(string toolName, int last = 100)
        {
            var events = new List<AIToolEvent>();

            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT EventJson FROM AIToolEvents
                                WHERE ToolName = @toolName
                                ORDER BY Timestamp DESC
                                LIMIT @limit
                            ";

                            command.Parameters.AddWithValue("@toolName", toolName);
                            command.Parameters.AddWithValue("@limit", last);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var json = reader.GetString(0);
                                    var evt = JsonSerializer.Deserialize<AIToolEvent>(json);
                                    if (evt != null) events.Add(evt);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error retrieving AI tool events: {ex.Message}");
                }
            }

            return events;
        }

        /// <summary>
        /// Get feedback events by tool
        /// </summary>
        public List<FeedbackEvent> GetFeedbackEventsByTool(string toolName, int last = 100)
        {
            var events = new List<FeedbackEvent>();

            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT EventJson FROM FeedbackEvents
                                WHERE SourceTool = @toolName
                                ORDER BY Timestamp DESC
                                LIMIT @limit
                            ";

                            command.Parameters.AddWithValue("@toolName", toolName);
                            command.Parameters.AddWithValue("@limit", last);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var json = reader.GetString(0);
                                    var evt = JsonSerializer.Deserialize<FeedbackEvent>(json);
                                    if (evt != null) events.Add(evt);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error retrieving feedback events: {ex.Message}");
                }
            }

            return events;
        }

        /// <summary>
        /// Get all events for training pipeline within a date range
        /// </summary>
        public object GetTrainingData(DateTime startTime, DateTime endTime)
        {
            var result = new Dictionary<string, object>();

            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        // Get activity snapshots
                        var snapshots = GetSnapshotsByDateRange(startTime, endTime);
                        result["ActivitySnapshots"] = snapshots.Count;

                        // Get user activity events
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT COUNT(*) FROM UserActivityEvents
                                WHERE Timestamp BETWEEN @start AND @end
                            ";
                            command.Parameters.AddWithValue("@start", startTime.ToString("O"));
                            command.Parameters.AddWithValue("@end", endTime.ToString("O"));
                            result["UserActivityEvents"] = command.ExecuteScalar();
                        }

                        // Get AI tool events
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT COUNT(*) FROM AIToolEvents
                                WHERE Timestamp BETWEEN @start AND @end
                            ";
                            command.Parameters.AddWithValue("@start", startTime.ToString("O"));
                            command.Parameters.AddWithValue("@end", endTime.ToString("O"));
                            result["AIToolEvents"] = command.ExecuteScalar();
                        }

                        // Get inter-AI events
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT COUNT(*) FROM InterAIEvents
                                WHERE Timestamp BETWEEN @start AND @end
                            ";
                            command.Parameters.AddWithValue("@start", startTime.ToString("O"));
                            command.Parameters.AddWithValue("@end", endTime.ToString("O"));
                            result["InterAIEvents"] = command.ExecuteScalar();
                        }

                        // Get feedback events
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT COUNT(*) FROM FeedbackEvents
                                WHERE Timestamp BETWEEN @start AND @end
                            ";
                            command.Parameters.AddWithValue("@start", startTime.ToString("O"));
                            command.Parameters.AddWithValue("@end", endTime.ToString("O"));
                            result["FeedbackEvents"] = command.ExecuteScalar();
                        }

                        result["TimeRange"] = new { Start = startTime, End = endTime };
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error retrieving training data: {ex.Message}");
                    result["Error"] = ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// Save user-provided activity label for training
        /// </summary>
        public void SaveActivityLabel(string snapshotId, ActivityLabelResult label)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                INSERT INTO ActivityLabels (
                                    LabelId, SnapshotId, Timestamp,
                                    EmotionLabel, ActivityLabel, LearningModeLabel,
                                    CognitiveLoadLabel, IntentLabel, Notes, UserProvided
                                ) VALUES (
                                    @labelId, @snapshotId, @timestamp,
                                    @emotion, @activity, @learningMode,
                                    @cognitiveLoad, @intent, @notes, @userProvided
                                )
                            ";

                            command.Parameters.AddWithValue("@labelId", Guid.NewGuid().ToString());
                            command.Parameters.AddWithValue("@snapshotId", snapshotId);
                            command.Parameters.AddWithValue("@timestamp", label.Timestamp.ToString("O"));
                            command.Parameters.AddWithValue("@emotion", label.EmotionLabel ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@activity", label.ActivityLabel ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@learningMode", label.LearningModeLabel ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@cognitiveLoad", label.CognitiveLoadLabel ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@intent", label.IntentLabel ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@notes", label.Notes ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@userProvided", label.UserProvided ? 1 : 0);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error saving activity label: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get labeled snapshots for ML training
        /// </summary>
        public List<(ActivitySnapshot snapshot, ActivityLabelResult label)> GetLabeledSnapshots(int limit = 1000)
        {
            var results = new List<(ActivitySnapshot, ActivityLabelResult)>();

            lock (_lockObject)
            {
                try
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT 
                                    s.SnapshotId, s.Timestamp, s.Category, s.CPUUsageGlobal, s.MemoryUsageGlobal,
                                    s.GPUUsageGlobal, s.GPUTempCelsius, s.CPUTempCelsius,
                                    s.ActiveWindowTitle, s.ActiveWindowProcess, s.ProcessesJson, s.BrowserActivityJson,
                                    l.EmotionLabel, l.ActivityLabel, l.LearningModeLabel, 
                                    l.CognitiveLoadLabel, l.IntentLabel, l.Notes
                                FROM ActivitySnapshots s
                                INNER JOIN ActivityLabels l ON s.SnapshotId = l.SnapshotId
                                WHERE l.UserProvided = 1
                                ORDER BY s.Timestamp DESC
                                LIMIT @limit
                            ";

                            command.Parameters.AddWithValue("@limit", limit);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var snapshot = new ActivitySnapshot
                                    {
                                        Id = reader.GetString(0),
                                        Timestamp = DateTime.Parse(reader.GetString(1)),
                                        Category = reader.GetString(2),
                                        CPUUsageGlobal = reader.GetDouble(3),
                                        MemoryUsageGlobal = reader.GetDouble(4),
                                        GPUUsageGlobal = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                                        GPUTempCelsius = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                                        CPUTempCelsius = reader.IsDBNull(7) ? null : reader.GetDouble(7)
                                    };

                                    var label = new ActivityLabelResult
                                    {
                                        SnapshotId = snapshot.Id,
                                        EmotionLabel = reader.IsDBNull(12) ? null : reader.GetString(12),
                                        ActivityLabel = reader.IsDBNull(13) ? null : reader.GetString(13),
                                        LearningModeLabel = reader.IsDBNull(14) ? null : reader.GetString(14),
                                        CognitiveLoadLabel = reader.IsDBNull(15) ? null : reader.GetString(15),
                                        IntentLabel = reader.IsDBNull(16) ? null : reader.GetString(16),
                                        Notes = reader.IsDBNull(17) ? null : reader.GetString(17)
                                    };

                                    results.Add((snapshot, label));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[MonitoringRepository] Error getting labeled snapshots: {ex.Message}");
                }
            }

            return results;
        }
    }
}

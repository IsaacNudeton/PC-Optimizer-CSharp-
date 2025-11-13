using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace PCOptimizer.Services
{
    /// <summary>
    /// Persists semantic conversation entries to SQLite
    /// Stores extracted context, not raw chat logs
    /// </summary>
    public class ConversationRepository
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly object _lockObject = new();

        public ConversationRepository()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PCOptimizer");

            Directory.CreateDirectory(appDataPath);
            _dbPath = Path.Combine(appDataPath, "conversations.db");
            _connectionString = $"Data Source={_dbPath};Cache=Shared";

            InitializeDatabase();
        }

        /// <summary>
        /// Initialize database schema
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
                                CREATE TABLE IF NOT EXISTS Conversations (
                                    Id TEXT PRIMARY KEY,
                                    Timestamp TEXT NOT NULL,
                                    SnapshotId TEXT,
                                    Topic TEXT NOT NULL,
                                    Intent TEXT NOT NULL,
                                    Category TEXT NOT NULL,
                                    Keywords TEXT NOT NULL,
                                    ResearchAreas TEXT NOT NULL,
                                    ProblemsDiscussed TEXT NOT NULL,
                                    SolutionsSuggested TEXT NOT NULL,
                                    ActiveProcesses TEXT NOT NULL,
                                    CurrentFocus TEXT,
                                    Outcome TEXT NOT NULL,
                                    CodeChangesRelated TEXT,
                                    OutcomeTimestamp TEXT,
                                    ConfidenceScore INTEGER NOT NULL,
                                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                                );

                                CREATE INDEX IF NOT EXISTS idx_timestamp ON Conversations(Timestamp);
                                CREATE INDEX IF NOT EXISTS idx_snapshot ON Conversations(SnapshotId);
                                CREATE INDEX IF NOT EXISTS idx_topic ON Conversations(Topic);
                                CREATE INDEX IF NOT EXISTS idx_outcome ON Conversations(Outcome);
                                CREATE INDEX IF NOT EXISTS idx_category ON Conversations(Category);
                                CREATE INDEX IF NOT EXISTS idx_confidence ON Conversations(ConfidenceScore);
                            ";

                            command.ExecuteNonQuery();
                        }

                        System.Console.WriteLine($"[ConversationRepository] Database initialized at {_dbPath}");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[ConversationRepository] Error initializing database: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Save conversation entry to database
        /// </summary>
        public void SaveConversation(ConversationEntry entry)
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
                                INSERT INTO Conversations (
                                    Id, Timestamp, SnapshotId, Topic, Intent, Category,
                                    Keywords, ResearchAreas, ProblemsDiscussed, SolutionsSuggested,
                                    ActiveProcesses, CurrentFocus, Outcome, CodeChangesRelated,
                                    OutcomeTimestamp, ConfidenceScore
                                ) VALUES (
                                    @id, @timestamp, @snapshotId, @topic, @intent, @category,
                                    @keywords, @researchAreas, @problems, @solutions,
                                    @processes, @focus, @outcome, @codeChanges,
                                    @outcomeTimestamp, @confidence
                                )
                            ";

                            command.Parameters.AddWithValue("@id", entry.Id);
                            command.Parameters.AddWithValue("@timestamp", entry.Timestamp.ToString("O"));
                            command.Parameters.AddWithValue("@snapshotId", entry.SnapshotId ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@topic", entry.Topic);
                            command.Parameters.AddWithValue("@intent", entry.Intent);
                            command.Parameters.AddWithValue("@category", entry.Category);
                            command.Parameters.AddWithValue("@keywords", JsonSerializer.Serialize(entry.Keywords));
                            command.Parameters.AddWithValue("@researchAreas", JsonSerializer.Serialize(entry.ResearchAreas));
                            command.Parameters.AddWithValue("@problems", JsonSerializer.Serialize(entry.ProblemsDiscussed));
                            command.Parameters.AddWithValue("@solutions", JsonSerializer.Serialize(entry.SolutionsSuggested));
                            command.Parameters.AddWithValue("@processes", JsonSerializer.Serialize(entry.ActiveProcesses));
                            command.Parameters.AddWithValue("@focus", entry.CurrentFocus);
                            command.Parameters.AddWithValue("@outcome", entry.Outcome);
                            command.Parameters.AddWithValue("@codeChanges", entry.CodeChangesRelated.Count > 0 ? JsonSerializer.Serialize(entry.CodeChangesRelated) : (object)DBNull.Value);
                            command.Parameters.AddWithValue("@outcomeTimestamp", entry.OutcomeTimestamp?.ToString("O") ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@confidence", entry.ConfidenceScore);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[ConversationRepository] Error saving conversation: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Update conversation outcome
        /// </summary>
        public void UpdateConversationOutcome(string conversationId, string outcome, List<string> changedFiles)
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
                                UPDATE Conversations
                                SET Outcome = @outcome,
                                    OutcomeTimestamp = @timestamp,
                                    CodeChangesRelated = @codeChanges
                                WHERE Id = @id
                            ";

                            command.Parameters.AddWithValue("@id", conversationId);
                            command.Parameters.AddWithValue("@outcome", outcome);
                            command.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("O"));
                            command.Parameters.AddWithValue("@codeChanges", JsonSerializer.Serialize(changedFiles));

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[ConversationRepository] Error updating outcome: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get conversations by date range
        /// </summary>
        public List<ConversationEntry> GetConversationsByDateRange(DateTime startTime, DateTime endTime)
        {
            var conversations = new List<ConversationEntry>();

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
                                SELECT * FROM Conversations
                                WHERE Timestamp BETWEEN @start AND @end
                                ORDER BY Timestamp DESC
                            ";

                            command.Parameters.AddWithValue("@start", startTime.ToString("O"));
                            command.Parameters.AddWithValue("@end", endTime.ToString("O"));

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    conversations.Add(ReaderToEntry(reader));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[ConversationRepository] Error retrieving conversations: {ex.Message}");
                }
            }

            return conversations;
        }

        /// <summary>
        /// Get conversations by topic
        /// </summary>
        public List<ConversationEntry> GetConversationsByTopic(string topic, int limit = 50)
        {
            var conversations = new List<ConversationEntry>();

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
                                SELECT * FROM Conversations
                                WHERE Topic = @topic
                                ORDER BY Timestamp DESC
                                LIMIT @limit
                            ";

                            command.Parameters.AddWithValue("@topic", topic);
                            command.Parameters.AddWithValue("@limit", limit);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    conversations.Add(ReaderToEntry(reader));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[ConversationRepository] Error retrieving conversations by topic: {ex.Message}");
                }
            }

            return conversations;
        }

        /// <summary>
        /// Get conversations with high confidence scores (for ML training)
        /// </summary>
        public List<ConversationEntry> GetHighConfidenceConversations(int minConfidence = 75, int limit = 100)
        {
            var conversations = new List<ConversationEntry>();

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
                                SELECT * FROM Conversations
                                WHERE ConfidenceScore >= @minConfidence
                                AND Outcome != 'pending'
                                ORDER BY ConfidenceScore DESC, Timestamp DESC
                                LIMIT @limit
                            ";

                            command.Parameters.AddWithValue("@minConfidence", minConfidence);
                            command.Parameters.AddWithValue("@limit", limit);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    conversations.Add(ReaderToEntry(reader));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[ConversationRepository] Error retrieving high-confidence conversations: {ex.Message}");
                }
            }

            return conversations;
        }

        /// <summary>
        /// Get statistics on conversations
        /// </summary>
        public object GetConversationStatistics(DateTime startTime, DateTime endTime)
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
                                    COUNT(*) as TotalConversations,
                                    AVG(ConfidenceScore) as AverageConfidence,
                                    COUNT(CASE WHEN Outcome = 'implemented' THEN 1 END) as ImplementedOutcomes,
                                    COUNT(CASE WHEN Outcome = 'researched' THEN 1 END) as ResearchedOutcomes,
                                    COUNT(CASE WHEN Outcome = 'abandoned' THEN 1 END) as AbandonedOutcomes
                                FROM Conversations
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
                                        totalConversations = reader.GetInt64(0),
                                        averageConfidence = reader.IsDBNull(1) ? 0 : Math.Round(reader.GetDouble(1), 2),
                                        implementedOutcomes = reader.GetInt64(2),
                                        researchedOutcomes = reader.GetInt64(3),
                                        abandonedOutcomes = reader.GetInt64(4),
                                        timeRange = new { start = startTime, end = endTime }
                                    };
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[ConversationRepository] Error retrieving statistics: {ex.Message}");
                }
            }

            return new { error = "Could not retrieve statistics" };
        }

        /// <summary>
        /// Convert database row to ConversationEntry
        /// </summary>
        private ConversationEntry ReaderToEntry(SqliteDataReader reader)
        {
            var entry = new ConversationEntry
            {
                Id = reader.GetString(0),
                Timestamp = DateTime.Parse(reader.GetString(1)),
                SnapshotId = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Topic = reader.GetString(3),
                Intent = reader.GetString(4),
                Category = reader.GetString(5),
                Keywords = JsonSerializer.Deserialize<List<string>>(reader.GetString(6)) ?? new(),
                ResearchAreas = JsonSerializer.Deserialize<List<string>>(reader.GetString(7)) ?? new(),
                ProblemsDiscussed = JsonSerializer.Deserialize<List<string>>(reader.GetString(8)) ?? new(),
                SolutionsSuggested = JsonSerializer.Deserialize<List<string>>(reader.GetString(9)) ?? new(),
                ActiveProcesses = JsonSerializer.Deserialize<List<string>>(reader.GetString(10)) ?? new(),
                CurrentFocus = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                Outcome = reader.GetString(12),
                CodeChangesRelated = reader.IsDBNull(13) ? new() : JsonSerializer.Deserialize<List<string>>(reader.GetString(13)) ?? new(),
                OutcomeTimestamp = reader.IsDBNull(14) ? null : DateTime.Parse(reader.GetString(14)),
                ConfidenceScore = reader.GetInt32(15)
            };

            return entry;
        }
    }
}

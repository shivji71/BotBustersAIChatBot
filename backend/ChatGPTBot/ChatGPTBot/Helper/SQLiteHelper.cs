using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using ChatGPTBot.Models;

namespace ChatGPTBot.Helper
{
    public class SQLiteHelper : ISQLiteHelperRepository
    {
        private readonly string dbPath;
        private readonly string connectionString;

        public SQLiteHelper()
        {
            //dbPath = ConfigurationManager.AppSettings["FAQDatabasePath"];
            dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "mydatabase.db");
            connectionString = $"Data Source={dbPath};Version=3;";
        }

        public async Task EnsureDatabaseAsync()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                Console.WriteLine($"✅ Database created at {dbPath}");
            }
            else
            {
                Console.WriteLine($"✅ Database exists at {dbPath}");
            }

            await EnsureTableAsync();
        }

        public async Task EnsureTableAsync()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                //        string tableCmd = @"
                //CREATE TABLE IF NOT EXISTS ChatbotFaqsEmbeddings(
                //    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                //    Role TEXT,
                //    Question TEXT,
                //    Answer TEXT,
                //    QuestionEmbedding BLOB,
                //    AnswerEmbedding BLOB
                //)";
                //        string tableCmd = @"
                //CREATE TABLE IF NOT EXISTS ChatHistory (
                //    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                //    UserId TEXT NOT NULL,
                //    UserMessage TEXT NOT NULL,
                //    BotResponse TEXT NOT NULL,
                //    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                //)";
                string tableCmd = @" CREATE TABLE IF NOT EXISTS Documents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    FileName TEXT NOT NULL,
    FileType TEXT NOT NULL,
    Content TEXT, -- extracted plain text from PDF
    Embedding BLOB, -- vector embedding
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
)";

                using (var cmd = new SQLiteCommand(tableCmd, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            Console.WriteLine("✅ ChatbotFaqsEmbeddings table ready with question & answer embeddings!");
        }
        

        public async Task<int> InsertFAQAsync(
string role,
string question,
string answer,
byte[] questionEmbedding,
byte[] answerEmbedding)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                string insertCmd = @"
            INSERT INTO ChatbotFaqsEmbeddings (Role, Question, Answer, QuestionEmbedding, AnswerEmbedding)
            VALUES (@role, @question, @answer, @questionEmbedding, @answerEmbedding);
            SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(insertCmd, connection))
                {
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@question", question);
                    cmd.Parameters.AddWithValue("@answer", answer);
                    cmd.Parameters.AddWithValue("@questionEmbedding", questionEmbedding ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@answerEmbedding", answerEmbedding ?? (object)DBNull.Value);

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<List<FAQItem>> GetFAQsAsync()
        {
            var faqs = new List<FAQItem>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Id, Role, Question, Answer, QuestionEmbedding, AnswerEmbedding FROM ChatbotFaqsEmbeddings";


                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        faqs.Add(new FAQItem
                        {
                            Id = reader.GetInt32(0),
                            Role = reader.GetString(1),
                            Question = reader.GetString(2),
                            Answer = reader.GetString(3),
                            QuestionEmbedding = reader["QuestionEmbedding"] as byte[] != null
                        ? Convert.ToBase64String((byte[])reader["QuestionEmbedding"])
                        : null,
                            AnswerEmbedding = reader["AnswerEmbedding"] as byte[] != null
                        ? Convert.ToBase64String((byte[])reader["AnswerEmbedding"])
                        : null
                        });
                    }
                }
            }

            return faqs;
        }

        public async Task<FAQItem> GetFAQByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Id, Role, Question, Answer, QuestionEmbedding, AnswerEmbedding FROM ChatbotFaqsEmbeddings WHERE Id = @id";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new FAQItem
                            {
                                Id = reader.GetInt32(0),
                                Role = reader.GetString(1),
                                Question = reader.GetString(2),
                                Answer = reader.GetString(3)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<int> SaveChatHistoryAsync(ChatHistoryItem chatHistory)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                string insertCmd = @"
            INSERT INTO ChatHistory (UserId, UserMessage, BotResponse, Timestamp)
            VALUES (@userId, @userMessage, @botResponse, @timestamp);
            SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(insertCmd, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", chatHistory.UserId ?? "default");
                    cmd.Parameters.AddWithValue("@userMessage", chatHistory.UserMessage);
                    cmd.Parameters.AddWithValue("@botResponse", chatHistory.BotResponse);
                    cmd.Parameters.AddWithValue("@timestamp",chatHistory.Timestamp == DateTime.MinValue ? DateTime.UtcNow : chatHistory.Timestamp
);

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<List<ChatHistoryItem>> GetLastChatsAsync(string userId, int limit = 5)
        {
            var history = new List<ChatHistoryItem>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
            SELECT Id, UserId, UserMessage, BotResponse, Timestamp
            FROM ChatHistory
            WHERE UserId = @userId
            ORDER BY Timestamp DESC
            LIMIT @limit;";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId ?? "default");
                    cmd.Parameters.AddWithValue("@limit", limit);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            history.Add(new ChatHistoryItem
                            {
                                Id = reader.GetInt32(0),
                                UserId = reader.GetString(1),
                                UserMessage = reader.GetString(2),
                                BotResponse = reader.GetString(3),
                                Timestamp = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }

            return history;
        }

        public async Task<int> SaveDocumentAsync(string userId, string fileName, string fileType, string content, byte[] embedding)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                string insertCmd = @"
            INSERT INTO Documents (UserId, FileName, FileType, Content, Embedding, Timestamp)
            VALUES (@userId, @fileName, @fileType, @content, @embedding, @timestamp);
            SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(insertCmd, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId ?? "default");
                    cmd.Parameters.AddWithValue("@fileName", fileName);
                    cmd.Parameters.AddWithValue("@fileType", fileType ?? "");
                    cmd.Parameters.AddWithValue("@content", content ?? "");
                    cmd.Parameters.AddWithValue("@embedding", embedding ?? new byte[0]);
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<List<DocumentItem>> GetDocumentsAsync(string userId)
        {
            var documents = new List<DocumentItem>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
            SELECT Id, UserId, FileName, FileType, Content, Embedding, Timestamp
            FROM Documents
            WHERE UserId = @userId
            ORDER BY Timestamp DESC;";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId ?? "default");

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            documents.Add(new DocumentItem
                            {
                                Id = reader.GetInt32(0),
                                UserId = reader.GetString(1),
                                FileName = reader.GetString(2),
                                FileType = reader.GetString(3),
                                Content = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                Embedding = reader.IsDBNull(5) ? new byte[0] : (byte[])reader["Embedding"],
                                Timestamp = reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }

            return documents;
        }
    }
}

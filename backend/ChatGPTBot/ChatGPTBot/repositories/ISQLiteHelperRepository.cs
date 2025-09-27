using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatGPTBot.Models;

namespace ChatGPTBot.Helper
{
    public interface ISQLiteHelperRepository
    {
        // ✅ Existing FAQ Methods
        Task EnsureDatabaseAsync();
        Task EnsureTableAsync();
        Task<int> InsertFAQAsync(string role, string question, string answer, byte[] questionEmbedding, byte[] answerEmbedding);
        Task<List<FAQItem>> GetFAQsAsync();
        Task<FAQItem> GetFAQByIdAsync(int id);

        // ✅ New Chat History Methods
        Task<int> SaveChatHistoryAsync(ChatHistoryItem chatHistory);
        Task<List<ChatHistoryItem>> GetLastChatsAsync(string userId, int limit = 5);

        // ✅ New Uploaded Document Methods (updated to match Documents table)
        Task<int> SaveDocumentAsync(string userId, string fileName, string fileType, string content, byte[] embedding);
        Task<List<DocumentItem>> GetDocumentsAsync(string userId);
    }
}

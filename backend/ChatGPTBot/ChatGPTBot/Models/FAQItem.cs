using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatGPTBot.Models
{
    public class FAQItem
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string QuestionEmbedding { get; set; }  // Base64 string
        public string AnswerEmbedding { get; set; }    // Base64 string
    }

    public class ChatHistoryItem
    {
        public int Id { get; set; }             // DB primary key
        public string UserId { get; set; }
        public string UserMessage { get; set; }
        public string BotResponse { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class UploadedDocument
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Content { get; set; }
        public byte[] Embedding { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DocumentItem
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Content { get; set; }   // extracted plain text
        public byte[] Embedding { get; set; } // vector
        public DateTime Timestamp { get; set; }
    }
}
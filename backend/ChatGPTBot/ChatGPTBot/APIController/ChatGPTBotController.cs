using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChatGPTBot.services;
using ChatGPTBot.Helper;
using ChatGPTBot.Models;

namespace ChatGPTBot.APIController
{
    public class ChatGPTBotController : ApiController
    {
        private readonly IChatBotRepository _iChatBotRepository;
        private readonly ISQLiteHelperRepository _iSQLiteHelperRepository;
        private readonly IAppLogger _logger; // optional for logging

        public ChatGPTBotController(
            IChatBotRepository iChatBotRepository,
            ISQLiteHelperRepository iSQLiteHelperRepository,
            IAppLogger logger = null)
        {
            _iChatBotRepository = iChatBotRepository;
            _iSQLiteHelperRepository = iSQLiteHelperRepository;
            _logger = logger;
        }

        [HttpPost]
        [Route("ChatGPTBotController/ask")]
        public async Task<IHttpActionResult> Ask([FromBody] AskRequest request)
        {
            // ✅ Validate input
            if (request == null || string.IsNullOrWhiteSpace(request.Question))
                return Content(HttpStatusCode.BadRequest, ApiResponse.Error("Please provide a valid question."));

            if (request.Question.Length < 3)
                return Content(HttpStatusCode.BadRequest, ApiResponse.Error("Question must be at least 3 characters long."));

            string userQuestion = request.Question.Trim();
            string userId = "demoUser"; // TODO: Replace with logged-in user ID

            try
            {
                // 🎯 Call repository for agent answer
                string agentAnswer = await _iChatBotRepository.AskAgentAsync(userQuestion);

                if (string.IsNullOrWhiteSpace(agentAnswer))
                {
                    agentAnswer = "I don’t have that information right now. You can reach out to our team at support@dyadtech.com for more details.";
                }

                return Ok(ApiResponse.Ok(new
                {
                    Question = userQuestion,
                    Answer = agentAnswer
                }));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"❌ Error in Ask(): {request.Question}");
                return Content(HttpStatusCode.InternalServerError, ApiResponse.Error("Something went wrong. Please try again later."));
            }
        }

        public class AskRequest
        {
            public string Question { get; set; }
        }

        #region Response Wrapper + Logger

        public class ApiResponse
        {
            public bool Success { get; private set; }
            public string Message { get; private set; }
            public object Data { get; private set; }

            public static ApiResponse Ok(object data, string message = "Request successful") =>
                new ApiResponse { Success = true, Message = message, Data = data };

            public static ApiResponse Error(string message) =>
                new ApiResponse { Success = false, Message = message, Data = null };
        }

        public interface IAppLogger
        {
            void LogError(Exception ex, string message);
        }

        #endregion
    }
}

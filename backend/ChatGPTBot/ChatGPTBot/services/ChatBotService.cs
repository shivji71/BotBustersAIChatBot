using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Amazon;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.Runtime;
using System.Configuration;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ChatGPTBot.services
{
    public class ChatBotService : IChatBotRepository
    {

        private readonly AmazonBedrockAgentRuntimeClient _agentClient;
        private readonly string _agentId;
        private readonly string _aliasId;


        public ChatBotService()
        {
            // Default: main appSettings
            string accessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string secretKey = ConfigurationManager.AppSettings["AWSSecretKey"];
            string region = ConfigurationManager.AppSettings["AWSRegion"];
            _agentId = ConfigurationManager.AppSettings["AWSAgentId"];
            _aliasId = ConfigurationManager.AppSettings["AWSAgentAliasId"];

            // Override with local file if present
            if (WebApiApplication.LocalAppSettings != null)
            {
                accessKey = WebApiApplication.LocalAppSettings["AWSAccessKey"]?.Value ?? accessKey;
                secretKey = WebApiApplication.LocalAppSettings["AWSSecretKey"]?.Value ?? secretKey;
                region = WebApiApplication.LocalAppSettings["AWSRegion"]?.Value ?? region;
                _agentId = WebApiApplication.LocalAppSettings["AWSAgentId"]?.Value ?? _agentId;
                _aliasId = WebApiApplication.LocalAppSettings["AWSAgentAliasId"]?.Value ?? _aliasId;
            }

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            _agentClient = new AmazonBedrockAgentRuntimeClient(credentials, RegionEndpoint.GetBySystemName(region));
        }



        public async Task<string> AskAgentAsync(string userInput)
        {
            
            
            var trimmedInput = userInput?.Trim().ToLower() ?? "";
            string greetingPattern = @"\b(hi|hii|hiii|hello|hey|hiya|howdy|good morning|good afternoon|good evening|greetings|what's up|whats up|how are you|how are you doing|how's it going|how's everything|how do you do|nice to meet you|pleased to meet you|howdy partner|yo|sup|morning|afternoon|evening|who are you)\b";

            if (Regex.IsMatch(trimmedInput, greetingPattern, RegexOptions.IgnoreCase))
            {
                // List of insurance chatbot greeting responses
                var greetingResponses = new List<string>
        {
            "Hello! How can I assist you with your insurance needs today?",
            "Hi there! I’m here to help with your insurance questions. What can I do for you?",
            "Greetings! How can I assist you with your insurance today?",
            "Hello! I’m here to make your insurance experience easy. What would you like to know?"
        };

                var random = new Random();
                return greetingResponses[random.Next(greetingResponses.Count)];

            }else
            {
                //string inputForService = userInput;
                if (!userInput.StartsWith("'") && !userInput.EndsWith("'"))
                {
                    userInput = $"'{userInput}'";
                }
            }

           

            var request = new InvokeAgentRequest
            {
                AgentId = _agentId,
                AgentAliasId = _aliasId,
                SessionId = Guid.NewGuid().ToString(),
                InputText = userInput
            };

            try
            {
                var response = await _agentClient.InvokeAgentAsync(request);

                if (response.Completion != null)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var chunk in response.Completion)
                    {
                        var payloadPart = chunk as Amazon.BedrockAgentRuntime.Model.PayloadPart;


                        if (payloadPart != null && payloadPart.Attribution?.Citations != null && payloadPart.Attribution.Citations.Count > 0)
                        {
                            var citation = payloadPart.Attribution.Citations[0];
                            var text = citation?.GeneratedResponsePart?.TextResponsePart?.Text;

                            if (!string.IsNullOrEmpty(text))
                            {
                                sb.Append(text);
                            }
                        }
                        //sb.Append(chunk);
                    }
                    return sb.ToString();
                }
                else
                {
                    return "No response received from agent.";
                }
                
            }
            catch (AmazonServiceException ex)
            {
                Console.WriteLine("AWS Error:");
                Console.WriteLine($"Status Code: {ex.StatusCode}");
                Console.WriteLine($"Error Code: {ex.ErrorCode}");
                Console.WriteLine($"Message: {ex.Message}");
                return ex.Message;
            }
        }

    }
}
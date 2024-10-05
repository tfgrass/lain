using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI;
using System;
using System.Threading.Tasks;

namespace Lain.Connectors
{
    public class LMStudio
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _streamingAgent;

        // LMStudio constructor with optional parameters
        public LMStudio(
            string apiUrl = "http://localhost:1234/v1/chat/completions",
            string model = "assistant",
            float temperature = 0.7f,
            string apiKey = "api-key")
        {
            var openaiClient = new OpenAIClient(apiKey, new OpenAIClientOptions
            {
                Endpoint = new Uri(apiUrl)
            });

            var chatClient = openaiClient.GetChatClient(model);

            // Create OpenAIChatAgent and wrap it with MiddlewareStreamingAgent
            var chatAgent = new OpenAIChatAgent(chatClient, name: model)
                .RegisterMessageConnector(); 

            // Wrap the chat agent with middleware streaming agent
            _streamingAgent = new MiddlewareStreamingAgent<OpenAIChatAgent>(chatAgent);
        }

        // Send method to send a message and handle responses or errors
        public async Task SendAsync(string userMessage, Action<string> onContent, Action<string>? onError = null)
        {
            try
            {
                var question = new TextMessage(Role.User, userMessage);

                // Stream responses using GenerateStreamingReplyAsync
                await foreach (var streamingReply in _streamingAgent.GenerateStreamingReplyAsync(new[] { question }))
                {
                    if (streamingReply is TextMessageUpdate textMessageUpdate)
                    {
                        onContent?.Invoke(textMessageUpdate.Content);  // Safely invoke the callback
           //             Console.WriteLine(textMessageUpdate.Content);
                    }
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Error: {ex.Message}");
            }
        }
    }
}

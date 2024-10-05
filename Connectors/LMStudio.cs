using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lain.Connectors
{
    public class LMStudio
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _streamingAgent;
        private readonly List<TextMessage> _chatHistory; // List to hold the chat history

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

            // Initialize the chat history list
            _chatHistory = new List<TextMessage>();
        }

        // Method to reset the chat history
        public void ResetChatHistory()
        {
            _chatHistory.Clear();
        }

        // Method to optionally send a message with or without chat history
        public async Task SendAsync(string userMessage, Action<string> onContent, Action<string>? onError = null, bool includeChatHistory = false)
        {
            try
            {
                var userTextMessage = new TextMessage(Role.User, userMessage);

                // Log the question
                Console.WriteLine($"Sending question: {userMessage}");

                // Add the current user message to the chat history if needed
                if (includeChatHistory)
                {
                    _chatHistory.Add(userTextMessage);
                }

                // Create the list of messages to send (either with or without chat history)
                var messagesToSend = includeChatHistory ? _chatHistory.ToArray() : new[] { userTextMessage };

                // Stream responses using GenerateStreamingReplyAsync
                await foreach (var streamingReply in _streamingAgent.GenerateStreamingReplyAsync(messagesToSend))
                {
                    // Log the content and trigger the onContent action if valid content is received
                    if (streamingReply is TextMessageUpdate textMessageUpdate)
                    {
                        if (onContent != null && !string.IsNullOrEmpty(textMessageUpdate.Content))
                        {
                            onContent(textMessageUpdate.Content);
                        }
                    }
                }
                Console.WriteLine("Finished streaming replies.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                onError?.Invoke($"Error: {ex.Message}");
            }
        }
    }
}

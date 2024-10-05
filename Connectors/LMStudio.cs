using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lain.Connectors
{
    public class LMStudio
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _streamingAgent;
        private readonly List<TextMessage> _chatHistory; // List to hold the chat history

        // Serilog logger instance
        private readonly ILogger _logger;

        // LMStudio constructor with optional parameters
        public LMStudio(
            string apiUrl = "http://localhost:1234/v1/chat/completions",
            string model = "assistant",
            float temperature = 0.7f,
            string apiKey = "api-key")
        {
            _logger = Log.ForContext<LMStudio>(); // Initialize logger

            _logger.Information("Initializing LMStudio with model {Model} and API URL {ApiUrl}", model, apiUrl);

            var openaiClient = new OpenAIClient(apiKey, new OpenAIClientOptions
            {
                Endpoint = new Uri(apiUrl)
            });

            var chatClient = openaiClient.GetChatClient(model);

            // Create OpenAIChatAgent and wrap it with MiddlewareStreamingAgent
            var chatAgent = new OpenAIChatAgent(chatClient, name: model)
                .RegisterMessageConnector();

            _streamingAgent = new MiddlewareStreamingAgent<OpenAIChatAgent>(chatAgent);

            _chatHistory = new List<TextMessage>();

            _logger.Information("LMStudio initialization complete.");
        }

        // Method to reset the chat history
        public void ResetChatHistory()
        {
            _chatHistory.Clear();
            _logger.Information("Chat history has been reset.");
        }

        // Method to optionally send a message with or without chat history
        public async Task SendAsync(string userMessage, Action<string> onContent, Action<string>? onError = null, bool includeChatHistory = false)
        {
            try
            {
                var userTextMessage = new TextMessage(Role.User, userMessage);

                // Log the message being sent
                _logger.Information("Sending user message: {UserMessage}", userMessage);

                // Add the current user message to the chat history if needed
                if (includeChatHistory)
                {
                    _chatHistory.Add(userTextMessage);
                    _logger.Information("User message added to chat history.");
                }

                // Create the list of messages to send (either with or without chat history)
                var messagesToSend = includeChatHistory ? _chatHistory.ToArray() : new[] { userTextMessage };

                // Stream responses using GenerateStreamingReplyAsync
                _logger.Information("Starting to stream replies...");
                await foreach (var streamingReply in _streamingAgent.GenerateStreamingReplyAsync(messagesToSend))
                {
                    if (streamingReply is TextMessageUpdate textMessageUpdate)
                    {
                        if (onContent != null && !string.IsNullOrEmpty(textMessageUpdate.Content))
                        {
                            _logger.Verbose("Received reply content: {ReplyContent}", textMessageUpdate.Content);
                            onContent(textMessageUpdate.Content);
                        }
                    }
                }

                _logger.Information("Finished streaming replies.");
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.Error(ex, "Exception occurred while sending message: {UserMessage}", userMessage);
                onError?.Invoke($"Error: {ex.Message}");
            }
        }
    }
}

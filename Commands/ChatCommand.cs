using System;
using OpenAI;
using System.Threading.Tasks;

namespace Lain.Commands
{
    public class ChatCommand
    {
        // Static variable to track if it's the first call in a session
        private static bool isFirstCall = true;

        // Chat history flag
        private static bool useChatHistory = true;

        public static string CommandName => "chat";

        public static void Execute(string[] args)
        {
            Console.WriteLine("Lain is in the chatroom. Type your messages. Type 'exit' to leave.");

            // Initialize the LMSConnector for the chat session
            var lmsConnector = new Lain.Connectors.LMStudio(
                apiUrl: "http://localhost:1234",  // Optionally customize the URL
                apiKey: "your-api-key-here"       // Optionally customize the API key
            );

            // Reset the first-call flag and chat history for a new session
            isFirstCall = true;

            while (true)
            {
                // Prompt user for input
                Console.Write("You> ");
                var question = Console.ReadLine();

                // Exit chat if user types 'exit'
                if (question.ToLower() == "exit")
                {
                    Console.WriteLine("Lain has left the chatroom.");
                    break;
                }

                // Handle empty input
                if (string.IsNullOrEmpty(question))
                {
                    Console.WriteLine("Please ask a question or type 'exit' to quit.");
                    continue;
                }

                // Send the question asynchronously with chat history
                lmsConnector.SendAsync(
                    question,
                    content => Console.Write(outputLain(content)),
                    error => Console.WriteLine($"Error: {error}"),
                    includeChatHistory: useChatHistory // Enable chat history
                ).Wait(); // Wait for the async task to complete
                
                Console.WriteLine(); // Add a new line after each response
            }
        }

        // Method to prefix the first response with "lain> "
        private static string outputLain(string content)
        {
            if (isFirstCall)
            {
                isFirstCall = false;
                return $"lain> {content}";
            }
            return content;
        }
    }
}

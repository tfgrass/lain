using System;
using OpenAI;

namespace Lain.Commands
{
    public class AskCommand
    {
        // Static variable to track if it's the first call
        private static bool isFirstCall = true;

        public static string CommandName => "ask";

        public static void Execute(string[] args)
        {
            var question = args.Length > 0 ? args[0] : string.Empty;

            if (string.IsNullOrEmpty(question))
            {
                Console.WriteLine("Lain is in the chatroom. Ask a question.");
                Console.WriteLine("Usage: lain ask <question>");
                return;
            }

            var lmsConnector = new Lain.Connectors.LMStudio(
                apiUrl: "http://localhost:1234",  // Optionally customize the URL
                apiKey: "your-api-key-here"       // Optionally customize the API key
            );

            lmsConnector.SendAsync(
                question,
                content => Console.Write(outputLain(content)),
                error => Console.WriteLine($"Error: {error}")
            ).Wait(); // Wait for the async task to complete
            Console.WriteLine(); // Add a new line after the response
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

using System;
using OpenAI;
namespace Lain.Commands
{
    public class AskCommand
    {
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
                content => Console.WriteLine($"Answer: {content}"),
                error => Console.WriteLine($"Error: {error}")
            ).Wait(); // Wait for the async task to complete
        }
    }
}

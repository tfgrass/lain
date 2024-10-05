using AutoGen.OpenAI;
using Lain.Connectors;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Initialize LMStudio connector with optional custom API URL or use defaults
        var lmStudio = new LMStudio(
            apiUrl: "http://localhost:1234",  // Optionally customize the URL
            apiKey: "your-api-key-here"       // Optionally customize the API key
        );

        // Use LMStudio to send a message and handle the content or errors
        await lmStudio.SendAsync(
            userMessage: "Write a piece of C# code to calculate the 100th Fibonacci number",
            onContent: content => Console.Write(content),
            onError: error => Console.WriteLine($"Error: {error}")
        );
    }
}

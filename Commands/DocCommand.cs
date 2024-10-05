using System;
using System.IO;
using Lain.Connectors;

namespace Lain.Commands
{
    public class DocCommand
    {
        public static string CommandName => "doc";

        public static void Execute(string[] args)
        {
            var sourceFile = args.Length > 0 ? args[0] : string.Empty;

            if (string.IsNullOrEmpty(sourceFile))
            {
                Console.WriteLine("Error: No source file provided.");
                Console.WriteLine("Usage: lain doc <source_file>");
                return;
            }

            var lmsConnector = new LMStudio(
                apiUrl: "http://localhost:1234",
                apiKey: "your-api-key-here"
            );

            GenerateDocumentation(lmsConnector, sourceFile);
        }

        private static void GenerateDocumentation(LMStudio lmsConnector, string sourceFile)
        {
            Console.WriteLine("Generating documentation...");

            if (!File.Exists(sourceFile))
            {
                Console.WriteLine($"Error: The file '{sourceFile}' does not exist.");
                return;
            }

            var codeContent = File.ReadAllText(sourceFile);

            // Define the system message
            var systemMessage = "You are a documentation generator. Given code, generate or update detailed markdown documentation for the code file. Summarize what the whole code does and then go into details on each function, class, and variable.";

            // Construct the user message
            var userMessage = $"Generate detailed markdown documentation for the following code:\n\n{codeContent}";

            // Create or clear the markdown file
            var mdFileName = Path.ChangeExtension(sourceFile, ".md");
            try
            {
                File.WriteAllText(mdFileName, string.Empty); // Clear the file content at the beginning
                Console.WriteLine($"Cleared content of '{mdFileName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing file: {ex.Message}");
                return;
            }

            try
            {
                lmsConnector.SendAsync(
                    $"{systemMessage}\n\n{userMessage}",
                    content => AppendContent(mdFileName, content),
                    error => Console.WriteLine($"Error: {error}")
                ).Wait(); // Wait for the async task to complete
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during documentation generation: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static void AppendContent(string mdFileName, string content)
        {
            try
            {
                // Append each chunk of content to the markdown file
                File.AppendAllText(mdFileName, content);
                Console.Write(content); // Optionally, you can print it to the console as well
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error appending to file: {ex.Message}");
            }
        }
    }
}

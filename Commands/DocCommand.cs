using System;
using System.IO;
using Lain.Connectors;
using Serilog;

namespace Lain.Commands
{
    public class DocCommand
    {
        private static readonly ILogger _logger = Log.ForContext<DocCommand>();

        public static string CommandName => "doc";

        public static void Execute(string[] args)
        {
            var sourceFile = args.Length > 0 ? args[0] : string.Empty;

            if (string.IsNullOrEmpty(sourceFile))
            {
                _logger.Error("No source file provided.");
                Console.WriteLine("Usage: lain doc <source_file>");
                return;
            }

            _logger.Information("Executing 'doc' command with source file: {SourceFile}", sourceFile);

            var lmsConnector = new LMStudio(
                apiUrl: "http://localhost:1234",
                apiKey: "your-api-key-here"
            );

            GenerateDocumentation(lmsConnector, sourceFile);
        }

        private static void GenerateDocumentation(LMStudio lmsConnector, string sourceFile)
        {
            _logger.Information("Starting documentation generation for file: {SourceFile}", sourceFile);

            if (!File.Exists(sourceFile))
            {
                _logger.Error("The file '{SourceFile}' does not exist.", sourceFile);
                Console.WriteLine($"Error: The file '{sourceFile}' does not exist.");
                return;
            }

            var codeContent = File.ReadAllText(sourceFile);
            _logger.Information("Read source file content: {SourceFile}", sourceFile);

            // Define the system message
            var systemMessage = "You are a documentation generator. Given code, generate or update detailed markdown documentation for the code file. Summarize what the whole code does and then go into details on each function, class, and variable.";

            // Construct the user message
            var userMessage = $"Generate detailed markdown documentation for the following code:\n\n{codeContent}";

            var mdFileName = Path.ChangeExtension(sourceFile, ".md");
            try
            {
                File.WriteAllText(mdFileName, string.Empty); // Clear the file content at the beginning
                _logger.Information("Cleared content of markdown file: {MarkdownFile}", mdFileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error clearing markdown file: {MarkdownFile}", mdFileName);
                Console.WriteLine($"Error clearing file: {ex.Message}");
                return;
            }

            try
            {
                lmsConnector.SendAsync(
                    $"{systemMessage}\n\n{userMessage}",
                    content => AppendContent(mdFileName, content),
                    error => 
                    {
                        _logger.Error("Error from LMS connector: {Error}", error);
                        Console.WriteLine($"Error: {error}");
                    }
                ).Wait(); // Wait for the async task to complete
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during documentation generation for file: {SourceFile}", sourceFile);
                Console.WriteLine($"Error during documentation generation: {ex.Message}");
            }
            _logger.Information("Documentation generation completed for file: {SourceFile}", sourceFile);
            Console.WriteLine();
        }

        private static void AppendContent(string mdFileName, string content)
        {
            try
            {
                // Append each chunk of content to the markdown file
                File.AppendAllText(mdFileName, content);
                _logger.Verbose("Appended content to markdown file: {MarkdownFile}", mdFileName);
                Console.Write(content); // Optionally, print to the console as well
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error appending to markdown file: {MarkdownFile}", mdFileName);
                Console.WriteLine($"Error appending to file: {ex.Message}");
            }
        }
    }
}

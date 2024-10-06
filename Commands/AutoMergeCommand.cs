using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lain.Commands
{
    public class AutoMergeCommand
    {
        public static string CommandName => "automerge";

        public static void Execute(string[] args)
        {
            Console.WriteLine("Starting auto-merge process...");

            // Open the current Git repository
            using (var repo = new Repository(Directory.GetCurrentDirectory()))
            {
                // Check if there are any merge conflicts
                if (!repo.Index.Conflicts.Any())
                {
                    Console.WriteLine("No merge conflicts detected.");
                    return;
                }

                Console.WriteLine("Merge conflicts detected. Attempting to resolve them using Lain...");

                // Initialize the LMSConnector for conflict resolution
                var lmsConnector = new Lain.Connectors.LMStudio(
                    apiUrl: "http://localhost:1234",  // Optionally customize the URL
                    apiKey: "your-api-key-here"       // Optionally customize the API key
                );

                // Loop through each conflict
                foreach (var conflict in repo.Index.Conflicts)
                {
                    Console.WriteLine($"Conflict detected in file: {conflict.Ours.Path}");

                    // Load the conflicting sections
                    var baseContent = GetFileContent(repo, conflict.Ancestor);
                    var oursContent = GetFileContent(repo, conflict.Ours);
                    var theirsContent = GetFileContent(repo, conflict.Theirs);

                    // Combine conflicting content
                    var conflictContent = $"BASE:\n{baseContent}\n\nOURS:\n{oursContent}\n\nTHEIRS:\n{theirsContent}";

                    // Send the conflict to Lain for resolution
                    lmsConnector.SendAsync(
                        conflictContent,
                        content => ApplyResolvedContent(repo, conflict.Ours.Path, content),
                        error => Console.WriteLine($"Error resolving conflict: {error}")
                    ).Wait(); // Wait for the async task to complete
                }

                Console.WriteLine("Auto-merge process complete.");
            }
        }

        // Helper method to get the content of a file from a conflict
        private static string GetFileContent(Repository repo, IndexEntry entry)
        {
            if (entry == null)
                return string.Empty;

            var blob = repo.Lookup<Blob>(entry.Id);
            using (var contentStream = new StreamReader(blob.GetContentStream()))
            {
                return contentStream.ReadToEnd();
            }
        }

        // Helper method to apply the resolved content back to the file
        private static void ApplyResolvedContent(Repository repo, string filePath, string resolvedContent)
        {
            // Replace the content of the conflicting file with the resolved content
            File.WriteAllText(filePath, resolvedContent);

            // Stage the resolved file for commit
            LibGit2Sharp.Commands.Stage(repo, filePath);

            Console.WriteLine($"Conflict in file '{filePath}' resolved and staged.");
        }
    }
}

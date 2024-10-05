using Lain.CLI;
using System;

class Program
{
    static void Main(string[] args)
    {
        var registry = new CommandRegistry();

        // Load commands from the Commands folder (assuming it is in the correct namespace)
        registry.LoadCommands("Commands");

        // Execute a command if provided
        if (args.Length > 0)
        {
            var commandName = args[0];
            var command = registry.GetCommand(commandName);
            if (command != null)
            {
                command(args[1..]); // Pass the remaining arguments to the command
            }
            else
            {
                Console.WriteLine($"Unknown command: {commandName}");
            }
        }
        else
        {
            // List all available commands if no command is provided
            Console.WriteLine("Available commands:");
            foreach (var commandName in registry.ListCommands())
            {
                Console.WriteLine(commandName);
            }

            Console.WriteLine("No command provided. Please provide a command.");
        }
    }
}

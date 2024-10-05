using Lain.CLI;
using Serilog;
using System;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // Set up Serilog configuration
        var logConfig = new LoggerConfiguration()
            .MinimumLevel.Warning() // Default log level
            .WriteTo.Console();

        // Adjust log level based on verbosity flags (-v, -vv, -vvv)
        if (args.Contains("-vvv"))
        {
            logConfig.MinimumLevel.Verbose();
        }
        else if (args.Contains("-vv"))
        {
            logConfig.MinimumLevel.Debug();
        }
        else if (args.Contains("-v"))
        {
            logConfig.MinimumLevel.Information();
        }

        // ToDo: sinnvolles file logging
//        logConfig.WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);

        // Create the logger
        Log.Logger = logConfig.CreateLogger();

        try
        {
            var registry = new CommandRegistry();

            // Load commands from the Commands folder
            registry.LoadCommands("Commands");

            // Execute a command if provided
            if (args.Length > 0)
            {
                var commandName = args[0];
                var command = registry.GetCommand(commandName);
                if (command != null)
                {
                    Log.Information($"Executing command: {commandName} with arguments: {string.Join(", ", args[1..])}");
                    command(args[1..]); // Pass the remaining arguments to the command
                }
                else
                {
                    Log.Warning($"Unknown command: {commandName}");
                }
            }
            else
            {
                // List all available commands if no command is provided
                Log.Information("Listing available commands.");
                Console.WriteLine("Available commands:");
                foreach (var commandName in registry.ListCommands())
                {
                    Console.WriteLine(commandName);
                }

                Console.WriteLine("No command provided. Please provide a command.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while running the command.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

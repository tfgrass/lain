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

        // ToDo: sinnvoll file logging
        // logConfig.WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);

        // Create the logger
        Log.Logger = logConfig.CreateLogger();

        try
        {
            var registry = new CommandRegistry();

            // Load commands (no need for a folder argument anymore)
            registry.LoadCommands();

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
                Console.WriteLine("No command provided. Please provide a command.");
                var command = registry.GetCommand("help");
                command?.Invoke(Array.Empty<string>()); // Pass an empty argument array
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

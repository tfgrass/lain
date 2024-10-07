using System;
using System.Collections.Generic;
using Serilog;

namespace Lain.CLI
{
    public class CommandRegistry
    {
        // Dictionary to store command names and their associated actions (delegates)
        private readonly Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();

        // Serilog logger
        private readonly ILogger _logger;

        // Constructor that initializes the logger and registers commands manually
        public CommandRegistry()
        {
            _logger = Log.ForContext<CommandRegistry>(); // Creates a logger specific to this class

            // Register basic commands: help and version
            RegisterCommand("help", HelpCommand);
            RegisterCommand("version", VersionCommand);

            // Register custom commands explicitly
            RegisterCommand("ask", Lain.Commands.AskCommand.Execute);
            RegisterCommand("chat", Lain.Commands.ChatCommand.Execute);
            RegisterCommand("doc", Lain.Commands.DocCommand.Execute);

            _logger.Information("All commands have been hardcoded and registered successfully.");
        }

        // Method to register a new command
        public void RegisterCommand(string name, Action<string[]> handler)
        {
            if (!_commands.ContainsKey(name))
            {
                _commands.Add(name, handler);
                _logger.Information("Command '{CommandName}' has been registered.", name);
            }
            else
            {
                _logger.Warning("Attempted to register command '{CommandName}' which is already registered.", name);
            }
        }

        // Method to get a command by name
        public Action<string[]>? GetCommand(string name)
        {
            if (_commands.TryGetValue(name, out var command))
            {
                _logger.Information("Command '{CommandName}' was found and will be executed.", name);
                return command;
            }
            _logger.Warning("Command '{CommandName}' was not found.", name);
            return null;  // Fixes CS8603
        }

        // Method to list all registered commands
        public IEnumerable<string> ListCommands()
        {
            _logger.Information("Listing all registered commands.");
            return _commands.Keys;
        }

        // Hardcoded Help command implementation
        private void HelpCommand(string[] args)
        {
            Console.WriteLine("Available commands:");
            foreach (var command in _commands.Keys)
            {
                Console.WriteLine($" - {command}");
            }
        }

        // Hardcoded Version command implementation
        private void VersionCommand(string[] args)
        {
            const string version = "Lain CLI version 0.1.1";
            Console.WriteLine(version);
            Console.WriteLine("by @tfgrass\nReleased under Artistic License 2.0");

            _logger.Information("Version command executed. Version: {Version}", version);
        }
    }
}

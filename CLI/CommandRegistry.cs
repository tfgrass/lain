using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace Lain.CLI
{
    public class CommandRegistry
    {
        // Dictionary to store command names and their associated actions (delegates)
        private readonly Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();

        // Serilog logger
        private readonly ILogger _logger;

        // Constructor that initializes the logger and registers basic commands
        public CommandRegistry()
        {
            _logger = Log.ForContext<CommandRegistry>();

            // Register basic commands: help and version
            RegisterCommand("help", HelpCommand);
            RegisterCommand("version", VersionCommand);

            // Load additional commands from the current assembly
            LoadCommands(); // Ensure LoadCommands is called in the constructor
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

        // Public method to load commands dynamically from the current assembly
        public void LoadCommands()
        {
            _logger.Information("Loading commands from the Lain.Commands namespace.");

            var commandTypes = LoadCommandModules();
            foreach (var commandType in commandTypes)
            {
                var commandNameProperty = commandType.GetProperty("CommandName", BindingFlags.Public | BindingFlags.Static);
                var executeMethod = commandType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);

                if (commandNameProperty != null && executeMethod != null)
                {
                    var commandName = (string)commandNameProperty.GetValue(null)!;  // Use null-forgiving operator (fixes CS8600)
                    
                    // Check if the command is already registered to avoid re-registering (fixes duplicate registration)
                    if (!_commands.ContainsKey(commandName))
                    {
                        Action<string[]> handler = (args) =>
                        {
                            // Correctly handle the Invoke method, as it should be a valid statement
                            executeMethod.Invoke(null, new object[] { args });
                        };
                        RegisterCommand(commandName, handler);
                        _logger.Information("Command '{CommandName}' from type '{CommandType}' was successfully loaded.", commandName, commandType.FullName);
                    }
                    else
                    {
                        _logger.Warning("Command '{CommandName}' is already registered, skipping registration.");
                    }
                }
                else
                {
                    _logger.Warning("Failed to load command from type '{CommandType}' because either CommandName or Execute method is missing.", commandType.FullName);
                }
            }
        }

        // Apply the DynamicallyAccessedMembers attribute to preserve the types loaded via reflection
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private IEnumerable<Type> LoadCommandModules()
        {
            List<Type> commandTypes = new List<Type>();

            var assembly = Assembly.GetExecutingAssembly();  // Load current assembly
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // Filter for the command types in the Lain.Commands namespace
                if (type.Namespace != null
                    && type.Namespace.StartsWith("Lain.Commands")
                    && type.IsClass
                    && !type.Name.Contains("<")) // This filters out the compiler-generated inner classes
                {
                    commandTypes.Add(type);
                    _logger.Information("Found command type: {CommandType}", type.FullName);
                }
            }

            if (commandTypes.Count == 0)
            {
                _logger.Error("No command types were found in the assembly. Please check if the 'Commands' namespace is correct.");
            }

            return commandTypes;
        }
    }
}

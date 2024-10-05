using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;

namespace Lain.CLI
{
    public class CommandRegistry
    {
        // Dictionary to store command names and their associated actions (delegates)
        private readonly Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();

        // Serilog logger
        private readonly ILogger _logger;

        // Constructor that accepts a logger
        public CommandRegistry()
        {
            _logger = Log.ForContext<CommandRegistry>(); // Creates a logger specific to this class
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
                throw new ArgumentException($"Command '{name}' is already registered.");
            }
        }

        // Method to get a command by name
        public Action<string[]> GetCommand(string name)
        {
            if (_commands.TryGetValue(name, out var command))
            {
                _logger.Information("Command '{CommandName}' was found and will be executed.", name);
                return command;
            }
            _logger.Warning("Command '{CommandName}' was not found.", name);
            return null;
        }

        // Method to list all registered commands
        public IEnumerable<string> ListCommands()
        {
            _logger.Information("Listing all registered commands.");
            return _commands.Keys;
        }

        // Method to load commands dynamically from the specified folder (e.g., Commands)
        public void LoadCommands(string folderPath)
        {
            _logger.Information("Loading commands from folder: {FolderPath}", folderPath);
            var commandTypes = LoadCommandModules(folderPath);
            foreach (var commandType in commandTypes)
            {
                var commandNameProperty = commandType.GetProperty("CommandName", BindingFlags.Public | BindingFlags.Static);
                var executeMethod = commandType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);

                if (commandNameProperty != null && executeMethod != null)
                {
                    var commandName = (string)commandNameProperty.GetValue(null);
                    Action<string[]> handler = (args) => executeMethod.Invoke(null, new object[] { args });
                    RegisterCommand(commandName, handler);
                    _logger.Information("Command '{CommandName}' from type '{CommandType}' was successfully loaded.", commandName, commandType.FullName);
                }
                else
                {
                    _logger.Warning("Failed to load command from type '{CommandType}' because either CommandName or Execute method is missing.", commandType.FullName);
                }
            }
        }

        // Helper method to load command types from the given folder
        private IEnumerable<Type> LoadCommandModules(string folderPath)
        {
            List<Type> commandTypes = new List<Type>();

            var assembly = Assembly.GetExecutingAssembly();  // Load current assembly
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // Filter out compiler-generated types (e.g., <>c or <>c__DisplayClass)
                if (type.Namespace != null
                    && type.Namespace.StartsWith("Lain.Commands")
                    && type.IsClass
                    && !type.Name.Contains("<")) // This filters out the compiler-generated inner classes
                {
                    commandTypes.Add(type);
                    _logger.Information("Found command type: {CommandType}", type.FullName);
                }
            }

            return commandTypes;
        }

    }
}

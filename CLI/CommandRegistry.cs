using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lain.CLI
{
    public class CommandRegistry
    {
        // Dictionary to store command names and their associated actions (delegates)
        private readonly Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();

        // Method to register a new command
        public void RegisterCommand(string name, Action<string[]> handler)
        {
            if (!_commands.ContainsKey(name))
            {
                _commands.Add(name, handler);
            }
            else
            {
                throw new ArgumentException($"Command '{name}' is already registered.");
            }
        }

        // Method to get a command by name
        public Action<string[]> GetCommand(string name)
        {
            if (_commands.TryGetValue(name, out var command))
            {
                return command;
            }
            return null;
        }

        // Method to list all registered commands
        public IEnumerable<string> ListCommands()
        {
            return _commands.Keys;
        }

        // Method to load commands dynamically from the specified folder (e.g., Commands)
        public void LoadCommands(string folderPath)
        {
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
                if (type.Namespace != null && type.Namespace.StartsWith("Lain.Commands") && type.IsClass)
                {
                    commandTypes.Add(type);
                }
            }

            return commandTypes;
        }
    }
}

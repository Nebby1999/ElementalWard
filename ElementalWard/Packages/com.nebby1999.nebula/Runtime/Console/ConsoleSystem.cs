using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace Nebula.Console
{
    public static class ConsoleSystem
    {
        private static readonly int maxLogCount = 30;
        private static List<Log> _logs = new List<Log>();
        public static event Action<ReadOnlyCollection<Log>> OnLogChanged;
        public delegate void ConsoleCommandDelegate(ConsoleCommandArgs args);
        private static Dictionary<string, ConsoleCommand> consoleCommands = new Dictionary<string, ConsoleCommand>(StringComparer.OrdinalIgnoreCase);
        private class ConsoleCommand
        {
            public ConsoleCommandDelegate action;
            public string helpText;
        }

        public static void Initialize()
        {
            Application.logMessageReceived -= HandleLog;
            Application.logMessageReceived += HandleLog;

            if (consoleCommands.Count > 0)
                return;

            List<ConsoleCommandAttribute> attributes = new List<ConsoleCommandAttribute>();
            if (!SearchableAttribute.TryGetInstances<ConsoleCommandAttribute>(attributes))
                return;
            attributes = attributes.OrderBy(x => x.commandName).ToList();
            foreach(ConsoleCommandAttribute attribute in attributes)
            {
                consoleCommands[attribute.commandName] = new ConsoleCommand
                {
                    action = (ConsoleCommandDelegate)Delegate.CreateDelegate(typeof(ConsoleCommandDelegate), attribute.Target as MethodInfo),
                    helpText = attribute.helpText
                };
            }
        }

        public static void ExecuteCommand(string commandName, string[] args)
        {
            if(!consoleCommands.TryGetValue(commandName, out var command))
            {
                Debug.LogError($"Command {commandName} is not a valid command. for a list of valid commands, use the command \"help\"");
                return;
            }

            Debug.Log($"Executing \"{commandName}\"");
            var conArgs = new ConsoleCommandArgs(commandName, args);
            command.action(conArgs);
        }

        private static void HandleLog(string message, string stackTrace, LogType type)
        {
            var log = new Log()
            {
                message = message,
                stackTrace = stackTrace,
                logType = type
            };
            _logs.Add(log);
            if(maxLogCount > 0)
            {
                while(_logs.Count > maxLogCount)
                {
                    _logs.RemoveAt(0);
                }
            }
            OnLogChanged?.Invoke(new(_logs));
        }

        [ConsoleCommand("help", "Displays all the commands, specifying a command as the second argument displays specific help for that command.")]
        private static void HelpCC(ConsoleCommandArgs args)
        {
            string specificCommand = args.TryGetArgString(0);
            if(specificCommand != null)
            {
                if (!consoleCommands.TryGetValue(specificCommand, out var command))
                {
                    Debug.Log($"Cannot display help text for command {specificCommand} as it doesnt exist.");
                    return;
                }
                Debug.Log($"Help for {specificCommand}: {command.helpText}");
                return;
            }

            StringBuilder builder = new StringBuilder();
            foreach(var (commandName, consoleCommand) in consoleCommands)
            {
                builder.AppendLine($"<color=green>{commandName}</color>: {consoleCommand.helpText}");
            }
            Debug.Log(builder.ToString());
        }
        public struct Log
        {
            public string message;
            public string stackTrace;
            public LogType logType;

            public override string ToString()
            {
                switch(logType)
                {
                    case LogType.Warning:
                        return string.Format("<color=yellow>{0}</color>", message);
                    case LogType.Error:
                        return string.Format("<color=red>{0}</color>", message);
                    default:
                        return message;
                }
            }
        }
    }
}

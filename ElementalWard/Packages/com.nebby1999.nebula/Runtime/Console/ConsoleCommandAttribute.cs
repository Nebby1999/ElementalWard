using System;

namespace Nebula.Console
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ConsoleCommandAttribute : SearchableAttribute
    {
        public string commandName;
        public string helpText;

        public ConsoleCommandAttribute(string commandName, string helpText)
        {
            this.commandName = commandName;
            this.helpText = helpText;
        }
    }
}
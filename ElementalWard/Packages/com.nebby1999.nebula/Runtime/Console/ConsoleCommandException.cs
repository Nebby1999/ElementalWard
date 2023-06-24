using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Nebula.Console
{
    [Serializable]
    public class ConsoleCommandException : Exception
    {
        public ConsoleCommandException()
        {
        }

        public ConsoleCommandException(string message)
            : base(message)
        {
        }

        public ConsoleCommandException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ConsoleCommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        public static void CheckArgumentCount(string[] args, int requiredArgCount)
        {
            if (args.Length < requiredArgCount)
            {
                throw new ConsoleCommandException($"{requiredArgCount} argument(s) required, {args.Length} argument(s) provided.");
            }
        }
    }
}
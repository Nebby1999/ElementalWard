using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nebula.Console
{
    public struct ConsoleCommandArgs
    {
        public string[] args;
        public string commandName;
        public string this[int i] => args[i];
        public int Count => args.Length;

        public void CheckArgCount(int count)
        {
            ConsoleCommandException.CheckArgumentCount(args, count);
        }

        public string TryGetArgString(int index)
        {
            if (index < args.Length)
                return args[index];
            return null;
        }

        public string GetArgString(int index)
        {
            return TryGetArgString(index) ?? throw new ConsoleCommandException($"Argument {index} must be a string.");
        }

        public ulong? TryGetArgULong(int index)
        {
            if(index < args.Length)
            {
                if (ulong.TryParse(args[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong result))
                    return result;
            }
            return null;
        }

        public ulong GetArgULong(int index)
        {
            return TryGetArgULong(index) ?? throw new ConsoleCommandException($"Argument {index} must be an unsigned integer.");
        }

        public int? TryGetArgInt(int index)
        {
            if (index < args.Length)
            {
                if (int.TryParse(args[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                    return result;
            }
            return null;
        }

        public int GetArgInt(int index)
        {
            return TryGetArgInt(index) ?? throw new ConsoleCommandException($"Argument {index} must be a signed integer.");
        }

        public bool? TryGetArgBool(int index)
        {
            if (index < args.Length)
            {
                if (bool.TryParse(args[index], out var result))
                    return result;
            }
            return null;
        }

        public bool GetArgBool(int index)
        {
            return TryGetArgBool(index) ?? throw new ConsoleCommandException($"Argument {index} must be a true or false value");
        }

        public float? TryGetArgFloat(int index)
        {
            if (index < args.Length)
            {
                if (float.TryParse(args[index], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return result;
            }
            return null;
        }

        public float GetArgFloat(int index)
        {
            return TryGetArgFloat(index) ?? throw new ConsoleCommandException($"Argument {index} must be a floating point number");
        }

        public double? TryGetArgDouble(int index)
        {
            if (index < args.Length)
            {
                if (double.TryParse(args[index], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return result;
            }
            return null;
        }

        public double GetArgDouble(int index)
        {
            return TryGetArgDouble(index) ?? throw new ConsoleCommandException($"Argument {index} must be a double prescicion floating point number.");
        }

        public T? TryGetArgEnum<T>(int index) where T : struct
        {
            if (index < args.Length && Enum.TryParse<T>(args[index], true, out var result))
                return result;
            return null;
        }

        public T GetArgEnum<T>(int index) where T : struct
        {
            return TryGetArgEnum<T>(index) ?? throw new ConsoleCommandException($"Argument {index} must be one of the values of {typeof(T).Name}");
        }

        public ConsoleCommandArgs(string commandName, IEnumerable<string> args)
        {
            this.args = args.ToArray();
            this.commandName = commandName;
        }
    }
}
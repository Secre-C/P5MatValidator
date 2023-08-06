using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P5MatValidator
{
    public class InputHandler
    {
        internal readonly string[] Args;
        internal readonly List<KeyValuePair<string, string>> Parameters = new();
        internal readonly List<string> Commands = new();
        public InputHandler(string[] input) 
        {
            Args = input;

            for (int i = 0; i < Args.Length;)
            {
                try
                {
                    if (Args[i].StartsWith('-'))
                    {
                        Parameters.Add(new KeyValuePair<string, string>(Args[i][1..], Args[i + 1]));
                        i += 2;
                    }
                    else if (Args[i].StartsWith('!'))
                    {
                        Commands.Add(Args[i][1..]);
                        i++;
                    }
                    else
                    {
                        i++;
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new Exception("Tried to read past program argument range. You may be missing an argument parameter.", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("An unhandled exception occured while parsing arguments", ex);
                }
            }


        }

        internal bool TryGetCommand(string command)
        {
            return Commands.Contains(command);
        }

        internal string GetParameterValue(string key)
        {
            foreach(var param in Parameters)
            {
                if (param.Key == key)
                    return param.Value;
            }

            throw new KeyNotFoundException($"Couldn't find arg key \"{key}\"");
        }

        internal bool TryGetParameterValue(string key, out string value)
        {
            try
            {
                value = GetParameterValue(key);
                return true;
            }
            catch (KeyNotFoundException)
            {
                Utils.DebugLog($"Could not find Key \"{key}\"");
                value = null;
                return false;
            }
        }
    }

    public class KeyNotFoundException : Exception 
    {
        public KeyNotFoundException()
        {
        }

        public KeyNotFoundException(string message)
            : base(message)
        {
        }

        public KeyNotFoundException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}

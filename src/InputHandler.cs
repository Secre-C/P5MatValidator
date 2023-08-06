using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P5MatValidator
{
    internal class InputHandler
    {
        internal readonly string[] Args;
        internal readonly Mode RunMode = 0;
        internal readonly List<KeyValuePair<string, string>> CommandParams = new();
        internal readonly List<string> Modifiers = new();
        internal InputHandler(string[] input) 
        {
            Args = input;

            for (int i = 0; i < Args.Length;)
            {
                try
                {
                    if (Args[i].StartsWith('-'))
                    {
                        CommandParams.Add(new KeyValuePair<string, string>(Args[i][1..], Args[i + 1]));
                        i += 2;
                    }
                    else if (Args[i].StartsWith('!'))
                    {
                        Modifiers.Add(Args[i]);
                        RunMode |= GetModeSet(Args[i]);
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

        internal string GetArgValue(string key)
        {
            foreach(var param in CommandParams)
            {
                if (param.Key == key)
                    return param.Value;
            }

            throw new KeyNotFoundException($"Couldn't find arg key \"{key}\"");
        }

        internal bool TryGetArgValue(string key, out string value)
        {
            try
            {
                value = GetArgValue(key);
                return true;
            }
            catch (KeyNotFoundException)
            {
                Utils.DebugLog($"Could not find Key \"{key}\"");
                value = null;
                return false;
            }
        }

        internal bool IsModeActive(Mode mode)
        {
            return (RunMode & mode) != 0;
        }

        private static Mode GetModeSet(string arg)
        {
            if (arg.Contains("validate")) return Mode.validate;
            if (arg.Contains("strict")) return Mode.strict;
            if (arg.Contains("convert")) return Mode.convert;
            if (arg.Contains("dump")) return Mode.dump;
            if (arg.Contains("search")) return Mode.search;

            return 0;
        }

        internal enum Mode
        {
            validate = 0x1,
            strict = 0x2,
            dump = 0x4,
            combine = 0x8,
            search = 0x10,
            convert = 0x20,
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

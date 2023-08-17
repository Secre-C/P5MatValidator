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
        internal readonly Dictionary<string, string> Parameters = new();
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
                        Parameters.Add(Args[i][1..].ToLower(), Args[i + 1].ToLower());
                        i += 2;
                    }
                    else if (Args[i].StartsWith('!'))
                    {
                        Commands.Add(Args[i][1..].ToLower());
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

        internal bool HasCommand(string command)
            => Commands.Contains(command.ToLower());

        internal string GetParameterValue(string key)
            => Parameters[key];

        internal bool TryGetParameterValue(string key, out string value)
            => Parameters.TryGetValue(key, out value);
    }
}

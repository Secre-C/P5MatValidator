namespace P5MatValidator;

internal static class Utils
{
    public static List<string> GetFiles(string path, string[] searchPatterns, SearchOption searchOption)
    {
        List<string> files = new();

        foreach (string pattern in searchPatterns)
        {
            files.AddRange(Directory.GetFiles(path, pattern, searchOption).ToList());
        }

        return files;
    }

    internal static void LogColor(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.White;
    }
    internal static void DebugLog(object message)
    {
#if DEBUG
        Console.WriteLine(message);
#endif
    }

    internal static void DebugLog(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        DebugLog(message);
        Console.ForegroundColor = ConsoleColor.White;
    }
}

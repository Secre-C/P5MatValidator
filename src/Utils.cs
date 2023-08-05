namespace P5MatValidator
{
    internal class Utils
    {
        internal static List<string> GetFiles(string path, string[] searchPatterns, SearchOption searchOption)
        {
            List<string> files = new();

            foreach (string pattern in searchPatterns)
            {
                files.AddRange(Directory.GetFiles(path, pattern, searchOption).ToList());
            }

            return files;
        }
    }
}

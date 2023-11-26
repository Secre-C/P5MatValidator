using System.Runtime.InteropServices;
using static GFDLibrary.Api.FlatApi;
using static P5MatValidator.Utils;

namespace P5MatValidator
{
    internal static class Dump
    {
        internal static void DumpMats(string resourceDir, string matOutputDir)
        {
            string[] fileExtensions = { "*.GFS", "*.GMD" };
            var gfsFileNames = GetFiles(resourceDir, fileExtensions, SearchOption.AllDirectories);
            var failedMaterialFiles = new List<string>();

            var asSpan = CollectionsMarshal.AsSpan(gfsFileNames);

            //dump all material dictionaries from all files
            foreach (string? file in asSpan)
            {
                try
                {
                    var gfsFile = LoadModel(file);

                    string savePath = Path.GetDirectoryName(Path.GetRelativePath(resourceDir, file)) + "\\";
                    Directory.CreateDirectory(matOutputDir + savePath);

                    gfsFile.Materials.Save($"{matOutputDir}{savePath}{Path.GetFileNameWithoutExtension(file)}.gmtd");
                }
                catch
                {
                    failedMaterialFiles.Add($"{Path.GetRelativePath(resourceDir, file)}");
                }
            }

            //Print files that failed to dump materials
            if (failedMaterialFiles.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nFailed Dumps:\n" +
                "=================================================");
                foreach (string material in failedMaterialFiles)
                {
                    Console.WriteLine($"{material}");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }

            return;
        }
    }
}

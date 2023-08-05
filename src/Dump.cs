using System.Runtime.InteropServices;
using static GFDLibrary.Api.FlatApi;
using static P5MatValidator.Program;
using static P5MatValidator.Utils;

namespace P5MatValidator
{
    internal class Dump
    {
        internal static void DumpMats(string[] args)
        {
            string modelDir = args[0];
            string matOutputDir = args[1];

            string[] fileExtensions = { "*.GFS", "*.GMD" };
            List<string> gfsFileNames = GetFiles(modelDir, fileExtensions, SearchOption.AllDirectories);

            var asSpan = CollectionsMarshal.AsSpan(gfsFileNames);

            //dump all material dictionaries from all files
            foreach (var file in asSpan)
            {
                try
                {
                    var gfsFile = LoadModel(file);

                    string savePath = Path.GetDirectoryName(Path.GetRelativePath(modelDir, file)) + "\\";
                    Directory.CreateDirectory(matOutputDir + savePath);

                    gfsFile.Materials.Save($"{matOutputDir}{savePath}{Path.GetFileNameWithoutExtension(file)}.gmtd");
                }
                catch
                {
                    FailedMaterialFiles.Add($"{Path.GetRelativePath(modelDir, file)}");
                }
            }

            //Print files that failed to dump materials
            if (FailedMaterialFiles.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nFailed Dumps:\n" +
                "=================================================");
                foreach (var material in FailedMaterialFiles)
                {
                    Console.WriteLine($"{material}");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }

            Program.Stopwatch.Stop();
            Console.WriteLine($"\nElapsed Time: {Program.Stopwatch.Elapsed}");

            return;
        }
    }
}

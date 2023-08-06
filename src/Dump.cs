using System.Runtime.InteropServices;
using static GFDLibrary.Api.FlatApi;
using static P5MatValidator.Program;
using static P5MatValidator.Utils;

namespace P5MatValidator
{
    internal class Dump
    {
        internal static void DumpMats(string resourceDir, string matOutputDir)
        {
            string[] fileExtensions = { "*.GFS", "*.GMD" };
            List<string> gfsFileNames = GetFiles(resourceDir, fileExtensions, SearchOption.AllDirectories);

            var asSpan = CollectionsMarshal.AsSpan(gfsFileNames);

            //dump all material dictionaries from all files
            foreach (var file in asSpan)
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
                    FailedMaterialFiles.Add($"{Path.GetRelativePath(resourceDir, file)}");
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

            return;
        }
    }
}

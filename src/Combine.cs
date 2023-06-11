using GFDLibrary.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P5MatValidator.Utils;
using static P5MatValidator.Program;

namespace P5MatValidator
{
    internal class Combine
    {
        internal static async void CreateCombinedMat(string modelsDir, string outputFilePath)
        {
            Console.WriteLine("Combining Materials...");

            var combinedMatDict = new MaterialDictionary();

            string[] pattern = { "*.GFS", "*.GMD", "*.gmt", "*.gmtd" };

            List<string> modelPaths = GetFiles(modelsDir, pattern, SearchOption.AllDirectories);

            foreach (string modelPath in modelPaths)
            {
                try
                {
                    var matList = await GenerateMaterialList(modelsDir, modelPath);
                    foreach (var mat in matList.materials)
                    {
                        if (mat.Version == matVersion || matVersion == null)
                            combinedMatDict.Add(mat);
                    }
                }
                catch 
                { 
                    FailedMaterialFiles.Add(Path.GetRelativePath(modelsDir, modelPath));    
                }
            }

            if (FailedMaterialFiles.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to Combine:\n" +
                "=================================================");
                foreach (var material in FailedMaterialFiles)
                {
                    Console.WriteLine($"{material}");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }

            combinedMatDict.Save(outputFilePath);
        }
    }
}

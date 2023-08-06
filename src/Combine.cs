using GFDLibrary.Materials;
using static P5MatValidator.Program;
using static P5MatValidator.Utils;

namespace P5MatValidator
{
    internal class Combine
    {
        internal static void CreateCombinedMat(MaterialResources materialResource, string outputFilePath, string materialVersion)
        {
            _ = UInt32.TryParse(materialVersion, out uint matVersion);
            Console.WriteLine("Combining Materials...");

            var combinedMatDict = new MaterialDictionary();

            foreach (var mat in materialResource.referenceMaterials.SelectMany(matDict => matDict.materials))
            {
                if (mat.Version == matVersion || matVersion == null)
                    combinedMatDict.Add(mat);
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

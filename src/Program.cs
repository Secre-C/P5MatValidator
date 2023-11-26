using GFDLibrary;
using GFDLibrary.Materials;
using System.Diagnostics;
using static P5MatValidator.Combine;
using static P5MatValidator.Converter;
using static P5MatValidator.Dump;
using static P5MatValidator.MaterialSearcher;

namespace P5MatValidator
{
    internal class Program
    {
        public static Stopwatch Stopwatch = new();

        static void ShowProgramUsage()
        {
            StreamReader reader = new("Usage.txt");
            Console.WriteLine(reader.ReadToEnd());
            Console.ReadKey();
            reader.Close();
            return;
        }

        static void Main(string[] args)
        {
            //Process Arguments and set modes
            InputHandler inputHandler = new(args);

            //timer for benchmarking
            Stopwatch.Start();

            //run commands
            if (inputHandler.HasCommand("validate") || inputHandler.HasCommand("convert"))
            {
                string fileInput = inputHandler.GetParameterValue("i");
                string materialReferenceDump = inputHandler.GetParameterValue("mats");

                MaterialResources materialResource = new(fileInput, materialReferenceDump);
                Validator validator = new(materialResource, inputHandler.HasCommand("strict"));

                validator.RunValidation();
                validator.PrintValidationResults();

                if (inputHandler.HasCommand("convert"))
                {
                    if (!inputHandler.TryGetParameterValue("o", out string modelOutputPath))
                        modelOutputPath = materialResource.InputFilePath;

                    ConvertAllInvalidMaterials(materialResource.Resource, inputHandler, validator, materialResource).Save(modelOutputPath);
                }
            }
            else if (inputHandler.TryGetParameterValue("combine", out string materialVersion))
            {
                string materialDumpPath = inputHandler.GetParameterValue("i");
                string outputFilePath = inputHandler.GetParameterValue("o");

                var materialResource = new MaterialResources(materialDumpPath);

                var combinedMat = CreateCombinedMat(materialResource, materialVersion);
                combinedMat.Save(outputFilePath);
            }
            else if (inputHandler.HasCommand("dump"))
            {
                string resourceInputDir = inputHandler.GetParameterValue("i");
                string outputDir = inputHandler.GetParameterValue("o");
                DumpMats(resourceInputDir, outputDir);
            }
            else if (inputHandler.HasCommand("search"))
            {
                string referenceMaterialPath = inputHandler.GetParameterValue("mats");
                var materialResource = new MaterialResources(referenceMaterialPath);
                var materialSearcher = new MaterialSearcher(inputHandler);
                materialSearcher.SearchForMaterial(materialResource);
                materialSearcher.PrintSearchResults();
            }
            else if (inputHandler.HasCommand("findsimilar"))
            {
                var inputMaterial = (Material)Resource.Load(inputHandler.GetParameterValue("i"));
                bool useStrictCompare = inputHandler.HasCommand("strict");
                int maximumPoints = int.Parse(inputHandler.GetParameterValue("points"));
                uint texcoordAccuracy = uint.Parse(inputHandler.GetParameterValue("accuracy"));
                string referenceMaterialPath = inputHandler.GetParameterValue("mats");
                var materialResource = new MaterialResources(referenceMaterialPath);

                bool matchesFound = TryFindReplacementMat(inputMaterial, materialResource, out var matches, useStrictCompare, maximumPoints, texcoordAccuracy);

                if (!matchesFound)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nNo matches");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    PrintFindReplacementResults(matches);
                }
            }
            else if (inputHandler.HasCommand("test"))
            {
                MaterialTester.TestMaterials(inputHandler);
            }
            else
            {
                ShowProgramUsage();
                return;
            }

            Stopwatch.Stop();
            Console.WriteLine($"\nElapsed Time: {Stopwatch.Elapsed}");

            return;
        }
    }
}
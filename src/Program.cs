using GFDLibrary;
using GFDLibrary.Materials;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static GFDLibrary.Api.FlatApi;
using static P5MatValidator.Combine;
using static P5MatValidator.Converter;
using static P5MatValidator.Dump;
using static P5MatValidator.MaterialSearcher;
using static P5MatValidator.Utils;
using static P5MatValidator.Validator;

namespace P5MatValidator
{
    internal class Program
    {
        public static Stopwatch Stopwatch = new ();
        public static List<string> FailedMaterialFiles = new();

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

            //run commands based on 
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

                    ConvertAllInvalidMaterials(materialResource.InputResource, inputHandler, validator, modelOutputPath, materialResource);
                }
            }
            else if (inputHandler.TryGetParameterValue("combine", out string materialVersion))
            {
                string materialDumpPath = inputHandler.GetParameterValue("i");
                string outputFilePath = inputHandler.GetParameterValue("o");

                var materialResource = new MaterialResources(materialDumpPath);

                CreateCombinedMat(materialResource, outputFilePath, materialVersion);
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
                Material inputMaterial = (Material)Resource.Load(inputHandler.GetParameterValue("i"));
                bool useStrictCompare = inputHandler.HasCommand("strict");
                int maximumPoints = int.Parse(inputHandler.GetParameterValue("points"));
                uint texcoordAccuracy = uint.Parse(inputHandler.GetParameterValue("accuracy"));
                string referenceMaterialPath = inputHandler.GetParameterValue("mats");
                var materialResource = new MaterialResources(referenceMaterialPath);

                bool matchesFound = TryFindReplacementMat(inputMaterial, materialResource, out List<MaterialComparer>? matches, useStrictCompare, maximumPoints, texcoordAccuracy);

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
                string resourceInput = inputHandler.GetParameterValue("i");
                string modOutputPath = inputHandler.GetParameterValue("o");
                string yamlPresetPath = inputHandler.GetParameterValue("preset");
                string cpkMakePath = inputHandler.GetParameterValue("cpkmakec");

                var resource = Resource.Load(resourceInput);

                if (inputHandler.TryGetParameterValue("batch", out string batchSizeStr))
                    MaterialTester.TestAllMaterials(resource, yamlPresetPath, resourceInput, cpkMakePath, modOutputPath, int.Parse(batchSizeStr));
                else
                    MaterialTester.TestAllMaterials(resource, yamlPresetPath, resourceInput, cpkMakePath, modOutputPath);
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
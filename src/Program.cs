using GFDLibrary;
using GFDLibrary.Materials;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
            if (inputHandler.TryGetCommand("validate") || inputHandler.TryGetCommand("convert"))
            {
                string fileInput = inputHandler.GetParameterValue("i");
                string materialReferenceDump = inputHandler.GetParameterValue("mats");

                MaterialResources materialResource = new(fileInput, materialReferenceDump);
                Validator validator = new(materialResource, inputHandler.TryGetCommand("strict"));

                validator.RunValidation();
                validator.PrintValidationResults();

                if (inputHandler.TryGetCommand("convert"))
                {
                    if (!inputHandler.TryGetParameterValue("o", out string modelOutputPath))
                        modelOutputPath = materialResource.inputFilePath;

                    if (materialResource.inputResource.ResourceType != ResourceType.ModelPack)
                    {
                        throw new Exception($"Expected to convert resource of type \"ModelPack\", got \"{materialResource.inputResource.ResourceType}\"");
                    }

                    _ = inputHandler.TryGetParameterValue("preset", out string presetYamlPath);

                    ConvertAllInvalidMaterials((ModelPack)materialResource.inputResource, modelOutputPath, presetYamlPath, validator);
                }
            }
            else if (inputHandler.TryGetParameterValue("combine", out string materialVersion))
            {
                string materialDumpPath = inputHandler.GetParameterValue("i");
                string outputFilePath = inputHandler.GetParameterValue("o");

                var materialResource = new MaterialResources(materialDumpPath);

                CreateCombinedMat(materialResource, outputFilePath, materialVersion);
            }
            else if (inputHandler.TryGetCommand("dump"))
            {
                string resourceInputDir = inputHandler.GetParameterValue("i");
                string outputDir = inputHandler.GetParameterValue("o");
                DumpMats(resourceInputDir, outputDir);
            }
            else if (inputHandler.TryGetCommand("search"))
            {
                string referenceMaterialPath = inputHandler.GetParameterValue("mats");
                var materialResource = new MaterialResources(referenceMaterialPath);
                var materialSearcher = new MaterialSearcher(inputHandler);
                materialSearcher.SearchForMaterial(materialResource);
                materialSearcher.PrintSearchResults();
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
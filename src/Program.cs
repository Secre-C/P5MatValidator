using GFDLibrary;
using GFDLibrary.Materials;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static GFDLibrary.Api.FlatApi;
using static P5MatValidator.Combine;
using static P5MatValidator.Converter;
using static P5MatValidator.Dump;
using static P5MatValidator.Search;
using static P5MatValidator.Utils;
using static P5MatValidator.Validator;

namespace P5MatValidator
{
    public struct ReferenceMaterial
    {
        internal List<Material> materials;
        internal string? fileName;
    }
    internal class Program
    {
        public static Mode mode = 0;
        public static int? matVersion = null;
        public static Stopwatch Stopwatch = new ();
        public static List<string> FailedMaterialFiles = new();

        internal enum Mode
        {
            validate = 0x1,
            strict = 0x2,
            dump = 0x4,
            combine = 0x8,
            search = 0x10,
            convert = 0x20,
        }
        static void ShowProgramUsage()
        {
            StreamReader reader = new("Usage.txt");
            Console.WriteLine(reader.ReadToEnd());
            reader.Close();
            Console.ReadKey();
            return;
        }

        static async Task Main(string[] args)
        {
            //Process Arguments and set modes
            InputHandler inputHandler = new(args);

            //return early if no mode is set
            if (inputHandler.RunMode == 0)
            {
                ShowProgramUsage();
                return;
            }

            //timer for benchmarking
            Stopwatch.Start();

            //run commands based on 
            if (inputHandler.IsModeActive(InputHandler.Mode.validate))
            {
                string fileInput = inputHandler.GetArgValue("i");
                string materialReferenceDump = inputHandler.GetArgValue("mats");

                MaterialResources materialResource = new(fileInput, materialReferenceDump);
                Validator validator = new(materialResource, inputHandler.IsModeActive(InputHandler.Mode.strict));

                validator.RunValidation();
                validator.PrintValidationResults();

                if (inputHandler.IsModeActive(InputHandler.Mode.convert))
                {
                    if (!inputHandler.TryGetArgValue("o", out string modelOutputPath))
                        modelOutputPath = materialResource.inputFilePath;

                    if (materialResource.inputResource.ResourceType != ResourceType.ModelPack)
                    {
                        throw new Exception($"Expected to convert resource of type \"ModelPack\", got \"{materialResource.inputResource.ResourceType}\"");
                    }

                    _ = inputHandler.TryGetArgValue("preset", out string presetYamlPath);
                    ConvertAllInvalidMaterials((ModelPack)materialResource.inputResource, modelOutputPath, presetYamlPath, validator);
                }
            }
            else if ((mode & Mode.combine) > 0)
            {
                CreateCombinedMat(args[0], args[1]);
            }
            else if ((mode & Mode.dump) > 0)
            {
                DumpMats(args);
            }
            else if ((mode & Mode.search) > 0)
            {
                PrintSearchResults(await SearchForMaterial(args));
            }

            Stopwatch.Stop();
            Console.WriteLine($"\nElapsed Time: {Stopwatch.Elapsed}");

            return;
        }
        internal static async Task<(ReferenceMaterial, List<ReferenceMaterial>)> PrepareMaterialLists(string compareModelDir, string referenceMaterialDir)
        {
            //Generate Compare Material List 
            Task<ReferenceMaterial> compareMaterials = GenerateMaterialList(compareModelDir, referenceMaterialDir);

            //Search all files in refMatDir for mats
            string[] fileExtenstions = { "*.gmtd", "*.gmt", "*.GFS", "*.GMD" };
            List<string> matFileNames = GetFiles($"{referenceMaterialDir}", fileExtenstions, SearchOption.AllDirectories);

            List<ReferenceMaterial> referenceMaterials = new();

            foreach (string matFile in matFileNames)
            {
                try
                {
                    referenceMaterials.Add(await GenerateMaterialList(matFile, referenceMaterialDir));
                }
                catch
                {
                    FailedMaterialFiles.Add(Path.GetRelativePath(referenceMaterialDir, matFile));
                }
            }

            return (await compareMaterials, referenceMaterials);
        }

        internal static async Task<ReferenceMaterial> GenerateMaterialList(string filePath, string referenceMaterialDir)
        {
            ReferenceMaterial materialInfo = new();

            //Add List of materials and filename to materialInfo struct
            if (Path.GetExtension(filePath).ToLower() == ".gfs" || Path.GetExtension(filePath).ToLower() == ".gmd")
            {
                materialInfo.materials = (List<Material>)LoadModel(filePath).Materials.Materials;
                materialInfo.fileName = Path.GetRelativePath(referenceMaterialDir, filePath);

                return materialInfo;
            }
            else if (Path.GetExtension(filePath).ToLower() == ".gmtd")
            {
                var matDict = (MaterialDictionary)Resource.Load(filePath);
                materialInfo.materials = (List<Material>)matDict.Materials;
                materialInfo.fileName = Path.GetRelativePath(referenceMaterialDir, filePath);

                return materialInfo;
            }
            else if (Path.GetExtension(filePath).ToLower() == ".gmt")
            {
                var mat = (Material)Resource.Load(filePath);

                materialInfo.materials = new List<Material>
                {
                    mat
                };

                materialInfo.fileName = Path.GetRelativePath(referenceMaterialDir, filePath);

                return materialInfo;
            }
            else
            {
                materialInfo.materials = new List<Material>();
                return materialInfo;
            }
        }
    }
}
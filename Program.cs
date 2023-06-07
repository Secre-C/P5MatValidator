using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using static GFDLibrary.Api.FlatApi;
using GFDLibrary.Materials;
using GFDLibrary;
using static P5MatValidator.Comparisons;
using static P5MatValidator.Validation;
using static P5MatValidator.Utils;
using static P5MatValidator.Combine;
using static P5MatValidator.Dump;

namespace P5MatValidator
{
    internal class Program
    {
        public static Mode mode = 0;
        public static int ?matVersion = null;
        public static Stopwatch Stopwatch = new Stopwatch();
        public static List<string> FailedMaterialFiles = new List<string>();

        internal enum Mode
        {
            validate = 1,
            strict = 2,
            dump = 4,
            combine = 8,
        }
        internal struct MaterialInfo
        {
            internal List<Material> materials;
            internal string? fileName;
        }
        static void GiveProgramInstructions()
        {
            Console.WriteLine("P5MatValidator By SecreC.\n\n" +
                    "Usage: <Input model or model directory> <dump directory or output> <Modes>\n" +
                    "Modes:\n" +
                    "\t-validate; compare each material in the first arg to the reference materials in the second and return which ones do and don't match\n" +
                    "\t-strict; values less likely to cause crashes will be compared\n" +
                    "\t-dump; dumps gmtds of all models in the first arg to the path in the second\n" +
                    "\t-combine <gfd version number (decimal)>; combines all materials in first arg subdirectories and outputs a single material to the output path");

            Console.ReadKey();
            return;
        }
        static void ProcessArgs(string[] args)
        {
            //print program usage if less than 3 args
            if (args.Length < 3)
            {
                GiveProgramInstructions();
                return;
            }

            //set mode based on args
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "-validate") mode |= Mode.validate;
                if (args[i] == "-strict") mode |= Mode.strict;
                if (args[i] == "-dump") mode |= Mode.dump;
                if (args[i] == "-combine")
                {
                    mode |= Mode.combine;

                    try
                    {
                        matVersion = Int32.Parse(args[i + 1]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("no version was found, version won't be filtered");
                    }
                    catch(FormatException)
                    {
                        Console.WriteLine("version number is not in the correct format. Correct format example: 17846528 (decimal)");
                        mode = 0;
                        return;
                    }
                    catch(OverflowException)
                    {
                        Console.WriteLine("version number is outside the bounds of an integer");
                        mode = 0;
                        return;
                    }
                }
            }
        }
        
        static void Main(string[] args)
        {
            //Process Arguments and set modes
            ProcessArgs(args);

            //return early if no mode is set
            if (mode == 0)
            {
                GiveProgramInstructions();
                return;
            }

            //timer for benchmarking
            Stopwatch.Start();

            //run commands based on 
            if ((mode & Mode.validate) > 0)
            {
                ValidateMaterials(args[0], args[1]);
                return;
            }
            else if ((mode & Mode.combine) > 0)
            {
                CreateCombinedMat(args[0], args[1]);
                return;
            }
            else if ((mode & Mode.dump) > 0)
            {
                DumpMats(args);
                return;
            }
        }
        internal static async Task<(MaterialInfo, List<MaterialInfo>)> PrepareMaterialLists(string compareModelDir, string referenceMaterialDir)
        {
            //Generate Compare Material List 
            Task<MaterialInfo> compareMaterials = GenerateMaterialList(compareModelDir, referenceMaterialDir);

            //Search all files in refMatDir for mats
            string[] fileExtenstions = {"*.gmtd", "*.gmt", "*.GFS", "*.GMD"};
            List<string> matFileNames = GetFiles($"{referenceMaterialDir}", fileExtenstions, SearchOption.AllDirectories);

            List<MaterialInfo> referenceMaterials = new();

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

            return(await compareMaterials, referenceMaterials);
        }

        internal static async Task<MaterialInfo> GenerateMaterialList(string filePath, string referenceMaterialDir)
        {
            MaterialInfo materialInfo = new();

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
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using static GFDLibrary.Api.FlatApi;
using GFDLibrary.Materials;
using GFDLibrary;

namespace P5MatValidator
{
    internal class Program
    {
        public static bool strictMode = false;
        public static bool dumpMode = false;
        public static bool combineMats = false;
        public static int ?matVersion = null;
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("P5MatValidator By SecreC. Compares materials with every material in Royal field GFS, and returns all that aren't identical\n" +
                    "Usage: <Input model or model directory> <dump directory or output> <Mode>\n" +
                    "Modes:\n" +
                    "\t-strict; values less likely to cause crashes will be compared\n" +
                    "\t-dump; dumps gmtds of all models in the first arg to the path in the second\n" +
                    "\t-combine <gfd version number (dec)>; combines all materials in first arg subdirectories and outputs a single material to the output path");

                Console.ReadKey();
                return;
            }

            if (args.Length > 2)
            {
                for (int i = 2; i <  args.Length; i++)
                {
                    if (args[i].ToLower() == "-strict") strictMode = true;
                    if (args[i].ToLower() == "-dump") dumpMode = true;
                    if (args[i].ToLower() == "-combine")
                    {
                        combineMats = true;
                        try
                        {
                            matVersion = Int32.Parse(args[i + 1]);
                        }
                        catch (IndexOutOfRangeException) { }
                    }
                }
            }

            //timer for benchmarking
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (combineMats)
            {
                await CreateCombinedMat(args[0], args[1]);
                return;
            }
            else if (dumpMode)
            {
                DumpMats(args, stopwatch);
                return;
            }

            //Get both material lists
            (List<Material> compareMaterials, List<Material> referenceMaterials) = await PrepareMaterialLists(args[0], args[1]);

            (List<string> invalidMats, List<string> validMats, List<string> sameNameMats) = await CompareAllMaterials(compareMaterials, referenceMaterials);

            PrintResults(args[0], stopwatch, invalidMats, validMats, sameNameMats);
        }
        static async Task<(List<Material>, List<Material>)> PrepareMaterialLists(string compareModelDir, string referenceMaterialDir)
        {
            //Generate Compare Material List 
            Task<List<Material>> compareMaterials = GenerateMaterialList(compareModelDir);

            string[] fileExtenstions = {"*.gmtd", "*.gmt", "*.GFS", "*.GMD"};
            List<string> matFileNames = GetFiles($"{referenceMaterialDir}", fileExtenstions, SearchOption.AllDirectories);

            List<Material> referenceMaterials = new();

            foreach (string matFile in matFileNames)
            {
                try
                {
                    referenceMaterials.AddRange(await GenerateMaterialList(matFile));
                }
                catch { }
            }

            return(await compareMaterials, referenceMaterials);
        }

        static async Task<List<Material>> GenerateMaterialList(string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() == ".gfs" || Path.GetExtension(filePath).ToLower() == ".gmd")
            {
                return (List<Material>)LoadModel(filePath).Materials.Materials;
            }
            else if (Path.GetExtension(filePath).ToLower() == ".gmtd")
            {
                var matDict = (MaterialDictionary)Resource.Load(filePath);
                return (List<Material>)matDict.Materials;
            }
            else if (Path.GetExtension(filePath).ToLower() == ".gmt")
            {
                var mat = (Material)Resource.Load(filePath);
                List<Material> materialList = new List<Material>
                {
                    mat
                };

                return materialList;
            }
            else
                return new List<Material>();
        }
        static async Task<(List<string>, List<string>, List<string>)> CompareAllMaterials(IList<Material> compareMaterials, List<Material> referenceMaterials)
        {
            List<string> invalidMats = new();
            List<string> validMats = new();
            List<string> sameNameMats = new();

            List<Task<(string compareMaterialName, byte isValid, string matchingMaterialName)>> compareTasks = new();

            foreach (var material in compareMaterials)
            {
                compareTasks.Add(CompareMaterial(material, referenceMaterials));
            }

            var results = await Task.WhenAll(compareTasks);

            foreach (var result in results)
            {
                if (result.isValid == 0) //invalid
                {
                    invalidMats.Add($"{result.compareMaterialName}");
                }
                else if (result.isValid == 2) //matching name
                {
                    sameNameMats.Add($"{result.compareMaterialName}");
                }
                else //valid
                {
                    validMats.Add($"{result.compareMaterialName} -> {result.matchingMaterialName}");
                }
            }

            return (invalidMats, validMats, sameNameMats);
        }

        static async Task<(string, byte, string?)> CompareMaterial(Material royalMaterial, List<Material> referenceMaterials)
        {
            byte validity = 0;
            string? matchingMat = null;

            foreach (var material in referenceMaterials)
            {
                if (material.Name == royalMaterial.Name)
                {
                    validity = 2;
                    matchingMat = material.Name;
                }
                if (!AreMatFlagsEqual(material.Flags, royalMaterial.Flags))
                    continue;
                if (!AreColorsEqual(material.AmbientColor, royalMaterial.AmbientColor) && strictMode)
                    continue;
                if (!AreColorsEqual(material.DiffuseColor, royalMaterial.DiffuseColor) && strictMode)
                    continue;
                if (!AreColorsEqual(material.SpecularColor, royalMaterial.SpecularColor) && strictMode)
                    continue;
                if (!AreColorsEqual(material.EmissiveColor, royalMaterial.EmissiveColor) && strictMode)
                    continue;
                if (!AreEqual(material.Field40, royalMaterial.Field40) && strictMode) //reflectivity
                    continue;
                if (!AreEqual(material.Field44, royalMaterial.Field44) && strictMode) //outline index
                    continue;
                if (!AreEqual((byte)material.DrawMethod, (byte)royalMaterial.DrawMethod))
                    continue;
                if (!AreEqual(material.Field49, royalMaterial.Field49))
                    continue;
                if (!AreEqual(material.Field4A, royalMaterial.Field4A) && strictMode)
                    continue;
                if (!AreEqual(material.Field4B, royalMaterial.Field4B))
                    continue;
                if (!AreEqual(material.Field4C, royalMaterial.Field4C) && strictMode)
                    continue;
                if (!AreEqual(material.Field4D, royalMaterial.Field4D)) //highlight blend mode
                    continue;
                if (!AreEqual(material.Field90, royalMaterial.Field90))
                    continue;
                if (!AreEqual(material.Field92, royalMaterial.Field92))
                    continue;
                if (!AreEqual(material.Field94, royalMaterial.Field94))
                    continue;
                if (!AreEqual(material.Field96, royalMaterial.Field96))
                    continue;
                if (!AreEqual(material.Field5C, royalMaterial.Field5C))
                    continue;
                if (!AreEqual(material.Field6C, royalMaterial.Field6C)) //texcoord1
                    continue;
                if (!AreEqual(material.Field70, royalMaterial.Field70)) //texcoord2
                    continue;
                if (!AreEqual(material.DisableBackfaceCulling, royalMaterial.DisableBackfaceCulling) && strictMode)
                    continue;
                if (!AreEqual(material.Field98, royalMaterial.Field98) && strictMode)
                    continue;
                if (!AreAttributesEqual(material, royalMaterial))
                    continue;

                validity = 1;
                matchingMat = material.Name;
                break;
            }

            return (royalMaterial.Name, validity, matchingMat);
        }
        static void PrintResults(string filePath, Stopwatch stopwatch, List<string> InvalidMats, List<string> validMats, List<string> sameNameMats)
        {
            Console.Clear();

            //Valid Mats
            Console.WriteLine("\n===============================================");
            Console.WriteLine($"{Path.GetFileName(filePath)}:");

            Console.WriteLine("===============================================");

            Console.WriteLine($"Valid Mats ({validMats.Count}):\n");

            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var mat in validMats)
                Console.WriteLine(mat);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("===============================================");

            //Matching Names
            if (!strictMode)
                Console.WriteLine($"Invalid Mats With Matching Names ({sameNameMats.Count}):\n");
            else
                Console.WriteLine($"Invalid Mats With Matching Names (Strict Mode) ({sameNameMats.Count}):\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var mat in sameNameMats)
                Console.WriteLine(mat);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("===============================================");

            //Invalid Mats
            if (!strictMode)
                Console.WriteLine($"Invalid Mats ({InvalidMats.Count}):\n");
            else
                Console.WriteLine($"Invalid Mats (Strict Mode) ({InvalidMats.Count}):\n");

            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var mat in InvalidMats)
                Console.WriteLine(mat);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("===============================================");

            stopwatch.Stop();
            Console.WriteLine($"\nElapsed Time: {stopwatch.Elapsed}");
        }

        static List<string> GetFiles(string path, string[] searchPatterns, SearchOption searchOption)
        {
            List<string> files = new List<string>();

            foreach (string pattern in searchPatterns)
            {
                files.AddRange(Directory.GetFiles(path, pattern, searchOption).ToList());
            }

            return files;
        }

        static void DumpMats(string[] args, Stopwatch stopwatch)
        {
            string modelDir = args[0];
            string matOutputDir = args[1];

            string[] fileExtensions = { "*.GFS", "*.GMD" };
            List<string> gfsFileNames = GetFiles(modelDir, fileExtensions, SearchOption.AllDirectories);

            var asSpan = CollectionsMarshal.AsSpan(gfsFileNames);

            List<Task> tasks = new();

            foreach (var file in asSpan)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var gfsFile = LoadModel(file);

                        string savePath = Path.GetDirectoryName(Path.GetRelativePath(modelDir, file)) + "\\";
                        Directory.CreateDirectory(matOutputDir + savePath);

                        gfsFile.Materials.Save($"{matOutputDir}{savePath}{Path.GetFileNameWithoutExtension(file)}.gmtd");
                    }
                    catch { }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            stopwatch.Stop();
            Console.WriteLine($"\nElapsed Time: {stopwatch.Elapsed}");

            return;
        }
        static async Task CreateCombinedMat(string modelsDir, string outputFilePath)
        {
            Console.WriteLine("Combining Materials...");

            var combinedMatDict = new MaterialDictionary();

            string[] pattern = { "*.GFS", "*.GMD", "*.gmt", "*.gmtd" };

            List<string> modelPaths = GetFiles(modelsDir, pattern, SearchOption.AllDirectories);

            foreach (string modelPath in modelPaths)
            {
                try
                {
                    var matList = await GenerateMaterialList(modelPath);
                    foreach (var mat in matList)
                    {
                        if (mat.Version == matVersion || matVersion == null)
                            combinedMatDict.Add(mat);
                    }
                }
                catch { }
            }

            combinedMatDict.Save(outputFilePath);
        }

        static bool AreEqual(object a, object b)
        {
            return Equals(a, b);
        }
        static bool AreColorsEqual(Vector4 a, Vector4 b)
        {
            if (!AreEqual(a.X, b.X)) return false;
            if (!AreEqual(a.Y, b.Y)) return false;
            if (!AreEqual(a.Z, b.Z)) return false;
            if (!AreEqual(a.W, b.W)) return false;

            return true;
        }

        static bool AreMatFlagsEqual(MaterialFlags a, MaterialFlags b)
        {
            if (!AreEqual((uint)a, (uint)b)) return false;

            return true;
        }

        static bool AreAttributesEqual(Material a, Material b)
        {
            if (!a.Flags.HasFlag(MaterialFlags.HasAttributes) && !b.Flags.HasFlag(MaterialFlags.HasAttributes))
                return true;
            else if (!a.Flags.HasFlag(MaterialFlags.HasAttributes) || !b.Flags.HasFlag(MaterialFlags.HasAttributes))
                return false;

            if (a.Attributes.Count != b.Attributes.Count)
                return false;

            bool typeMatchFound = false;

            foreach (var attr in a.Attributes)
            {
                foreach (var attr2 in b.Attributes)
                {
                    if (attr.AttributeType == attr2.AttributeType)
                    {
                        typeMatchFound = true;
                        break;
                    }
                }

                if (!typeMatchFound)
                    return false;

                typeMatchFound = false;
            }

            foreach (var attr in a.Attributes)
            {
                if (attr.AttributeType == MaterialAttributeType.Type1)
                {
                    foreach (var attr2 in b.Attributes)
                    {
                        if (attr2.AttributeType == MaterialAttributeType.Type1)
                        {
                            var type1a = (MaterialAttributeType1)attr;
                            var type1b = (MaterialAttributeType1)attr2;

                            if (type1a.RawFlags != type1b.RawFlags) return false;
                            if (((ushort)type1a.Type1Flags) != ((ushort)type1b.Type1Flags)) return false;
                            if (type1a.Field1C != type1b.Field1C && strictMode) return false;
                            if (type1a.Field20 != type1b.Field20 && strictMode) return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
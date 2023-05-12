﻿using System.Diagnostics;
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
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("P5MatValidator By SecreC. Compares materials with every material in Royal field GFS, and returns all that aren't identical\n" +
                    "Usage: <Path to file to compare> <Path to a gmtd/gmt dump> <modes>\n" +
                    "Modes:\n" +
                    "\t-strict; values less likely to cause crashes will be compared\n" +
                    "\t-dump; dumps gmtds of all models from the second arg to the path in the first\n");

                Console.ReadKey();
                return;
            }

            if (args.Length > 2)
            {
                for (int i = 2; i <  args.Length; i++)
                {
                    if (args[i].ToLower() == "-strict") strictMode = true;
                    if (args[i].ToLower() == "-dump") dumpMode = true;
                }
            }

            //timer for benchmarking
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (dumpMode)
            {
                DumpMats(args, stopwatch);
                return;
            }

            //Get both material lists
            (List<Material> compareMaterials, List<Material> referenceMaterials) = PrepareMaterialLists(args[0], args[1]);

            List<string> InvalidMats = new();
            List<string> validMats = new();

            foreach ( var material in compareMaterials )
            {
                var result = CompareMaterial(material, referenceMaterials);
                if (!result.Item1)
                {
                    InvalidMats.Add($"{material.Name}");
                }
                else
                {
                    validMats.Add($"{material.Name} -> {result.Item2}");
                }
            }

            PrintResults(args[0], stopwatch, InvalidMats, validMats);
        }
        static (List<Material>, List<Material>) PrepareMaterialLists(string compareModelDir, string referenceMaterialDir)
        {
            List<Material> referenceMaterials = new ();

            //Generate Compare Material List 
            List<Material> compareMaterials = GenerateMaterialList(compareModelDir).ToList();

            List<string> matFileNames = Directory.GetFiles($"{referenceMaterialDir}", $"*.gmtd", SearchOption.AllDirectories).ToList();
            matFileNames.AddRange(Directory.GetFiles($"{referenceMaterialDir}", $"*.gmt", SearchOption.AllDirectories).ToList());
            matFileNames.AddRange(Directory.GetFiles($"{referenceMaterialDir}", $"*.GFS", SearchOption.AllDirectories).ToList());
            matFileNames.AddRange(Directory.GetFiles($"{referenceMaterialDir}", $"*.GMD", SearchOption.AllDirectories).ToList());

            var asSpan = CollectionsMarshal.AsSpan(matFileNames);

            foreach (string matFile in asSpan)
            {
                try
                {
                    referenceMaterials.AddRange(GenerateMaterialList(matFile));
                }
                catch { }
            }

            return(compareMaterials, referenceMaterials);
        }
        static void PrintResults(string filePath, Stopwatch stopwatch, List<string> InvalidMats, List<string> validMats)
        {
            Console.Clear();

            Console.WriteLine("\n===============================================");
            Console.WriteLine($"{Path.GetFileName(filePath)}:");

            Console.WriteLine("===============================================");

            if (!strictMode)
                Console.WriteLine($"Invalid Mats ({InvalidMats.Count}):\n");
            else
                Console.WriteLine($"Invalid Mats (Strict Mode) ({InvalidMats.Count}):\n");

            foreach (var mat in InvalidMats)
                Console.WriteLine(mat);

            Console.WriteLine("===============================================");
            Console.WriteLine($"Valid Mats ({validMats.Count}):\n");

            foreach (var mat in validMats)
                Console.WriteLine(mat);


            Console.WriteLine("===============================================");

            stopwatch.Stop();
            Console.WriteLine($"\nElapsed Time: {stopwatch.Elapsed}");
        }

        static (bool, string) CompareMaterial(Material royalMaterial, List<Material> referenceMaterials)
        {
            foreach (var material in referenceMaterials)
            {
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

                return (true, material.Name);
            }

            return (false, null);
        }

        static IList<Material> GenerateMaterialList(string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() == ".gfs" || Path.GetExtension(filePath).ToLower() == ".gmd")
            {
                return LoadModel(filePath).Materials.Materials;
            }
            else if (Path.GetExtension(filePath).ToLower() == ".gmtd")
            {
                var matDict = (MaterialDictionary)Resource.Load(filePath);
                return matDict.Materials;
            }
            else if (Path.GetExtension(filePath).ToLower() == ".gmt")
            {
                var mat = (Material)Resource.Load(filePath);
                IList<Material> materialList = new List<Material>
                {
                    mat
                };

                return materialList;
            }
            else
                return new List<Material>();
        }

        static void DumpMats(string[] args, Stopwatch stopwatch)
        {
            string modelDir = args[1];
            string matOutputDir = args[0];

            List<string> gfsFileNames = Directory.GetFiles($"{modelDir}", $"*.GFS", SearchOption.AllDirectories).ToList();
            gfsFileNames.AddRange(Directory.GetFiles($"{modelDir}", $"*.GMD", SearchOption.AllDirectories).ToList());

            var asSpan = CollectionsMarshal.AsSpan(gfsFileNames);

            foreach (var file in asSpan)
            {
                try
                {
                    var gfsFile = LoadModel(file);

                    string savePath = Path.GetDirectoryName(Path.GetRelativePath(modelDir, file)) + "\\";
                    Directory.CreateDirectory(matOutputDir + savePath);

                    gfsFile.Materials.Save($"{matOutputDir}{savePath}{Path.GetFileNameWithoutExtension(file)}.gmtd");
                }
                catch { }
            }

            stopwatch.Stop();
            Console.WriteLine($"\nElapsed Time: {stopwatch.Elapsed}");

            return;
        }

        static bool AreEqual(object a, object b)
        {
            return Equals(a, b);
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
    }
}
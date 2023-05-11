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
                dumpMats(args, stopwatch);
                return;
            }

            var inputGFS = LoadModel(args[0]);
            var RoyalMaterials = inputGFS.Materials.Materials;

            List<string> InvalidMats = new();
            List<string> validMats = new();

            Console.Clear();

            if (!strictMode)
                Console.WriteLine("Scanning For Matching Materials:\n");
            else
                Console.WriteLine("Scanning For Matching Materials in Strict Mode:\n");

            foreach ( var material in RoyalMaterials )
            {
                Console.WriteLine(material.Name);

                var result = CompareMaterial(material, args[1]);
                if (!result.Item1)
                {
                    InvalidMats.Add($"{material.Name}");
                }
                else
                {
                    validMats.Add($"{material.Name} -> {result.Item2}");
                }
            }

            PrintResults(stopwatch, InvalidMats, validMats);
        }

        static void PrintResults(Stopwatch stopwatch, List<string> InvalidMats, List<string> validMats)
        {
            Console.Clear();

            Console.WriteLine("\n===============================================");

            if (!strictMode)
                Console.WriteLine("Invalid Mats:\n");
            else
                Console.WriteLine("Invalid Mats (Strict Mode):\n");

            foreach (var mat in InvalidMats)
                Console.WriteLine(mat);

            Console.WriteLine("===============================================");
            Console.WriteLine("Valid Mats:\n");
            foreach (var mat in validMats)
                Console.WriteLine(mat);


            Console.WriteLine("===============================================");

            stopwatch.Stop();
            Console.WriteLine("\nElapsed Time: " + stopwatch.Elapsed);
        }

        static (bool, string) CompareMaterial(Material royalMaterial, string vanillaModelDir)
        {
            List<string> matfileNames = Directory.GetFiles($"{vanillaModelDir}", $"*.gmtd", SearchOption.AllDirectories).ToList();
            matfileNames.AddRange(Directory.GetFiles($"{vanillaModelDir}", $"*.gmt", SearchOption.AllDirectories).ToList());

            var asSpan = CollectionsMarshal.AsSpan(matfileNames);

            foreach (string gfsFile in asSpan)
            {
                try
                {
                    var vanillaMats = GenerateMaterialList(gfsFile);

                    foreach (var material in vanillaMats)
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
                }
                catch
                {

                }
                
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

        static void dumpMats(string[] args, Stopwatch stopwatch)
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

                    gfsFile.Materials.Save(matOutputDir + savePath + Path.GetFileNameWithoutExtension(file) + ".gmtd");
                }
                catch { }
            }

            stopwatch.Stop();
            Console.WriteLine("\nElapsed Time: " + stopwatch.Elapsed);

            return;
        }

        //Assert.AreEqual(a.Name, b.Name);
        //Assert.AreEqual(a.Flags, b.Flags);
        //Assert.AreEqual(a.AmbientColor, b.AmbientColor);
        //Assert.AreEqual(a.DiffuseColor, b.DiffuseColor);
        //Assert.AreEqual(a.SpecularColor, b.SpecularColor);
        //Assert.AreEqual(a.EmissiveColor, b.EmissiveColor);
        //Assert.AreEqual(a.Field40, b.Field40);
        //Assert.AreEqual(a.Field44, b.Field44);
        //Assert.AreEqual(a.DrawMethod, b.DrawMethod);
        //Assert.AreEqual(a.Field49, b.Field49);
        //Assert.AreEqual(a.Field4A, b.Field4A);
        //Assert.AreEqual(a.Field4B, b.Field4B);
        //Assert.AreEqual(a.Field4C, b.Field4C);
        //Assert.AreEqual(a.Field4D, b.Field4D);
        //Assert.AreEqual(a.Field90, b.Field90);
        //Assert.AreEqual(a.Field92, b.Field92);
        //Assert.AreEqual(a.Field94, b.Field94);
        //Assert.AreEqual(a.Field96, b.Field96);
        //Assert.AreEqual(a.Field5C, b.Field5C);
        //Assert.AreEqual(a.Field6C, b.Field6C);
        //Assert.AreEqual(a.Field70, b.Field70);
        //Assert.AreEqual(a.DisableBackfaceCulling, b.DisableBackfaceCulling);
        //Assert.AreEqual(a.Field98, b.Field98);
        static bool AreEqual(object a, object b)
        {
            return Equals(a, b);
        }

        static bool AreAttributesEqual(Material a, Material b)
        {
            bool typeMatchFound = false;

            if (a.Flags.HasFlag(MaterialFlags.HasAttributes) && b.Flags.HasFlag(MaterialFlags.HasAttributes))
            {
                if (a.Attributes.Count != b.Attributes.Count) return false;

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

                    if (!typeMatchFound) return false;

                    typeMatchFound = false;
                }

                foreach (var attr in a.Attributes)
                {
                    if (attr.AttributeType == MaterialAttributeType.Type1)
                    {
                        foreach(var attr2 in b.Attributes)
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
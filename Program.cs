using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using static GFDLibrary.Api.FlatApi;
using GFDLibrary.Materials;

namespace P5MatValidator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
                Console.WriteLine("P5MatValidator By SecreC. Compares materials with every material in Royal field GFS, and returns all that aren't identical\n" +
                    "Usage: <Path to file to compare> <Path to the vanilla model folder> <modes>\n" +
                    "Modes: -strict; values less likely to cause crashes will be compared");

            bool useStrictMode = false;
            if (args.Length > 2) useStrictMode = args[2] == "-strict";

            //timer for benchmarking
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var inputGFS = LoadModel(args[0]);
            var RoyalMaterials = inputGFS.Materials.Materials;

            List<string> InvalidMats = new();
            List<string> validMats = new();

            foreach ( var material in RoyalMaterials )
            {
                var result = CompareMaterial(material, args[1], useStrictMode);
                if (!result.Item1)
                {
                    InvalidMats.Add($"{material.Name}");
                }
                else
                {
                    validMats.Add($"{material.Name} -> {result.Item2}");
                }
            }

            Console.Clear();

            Console.WriteLine("\nInvalid Mats:\n");
            foreach (var mat in InvalidMats)
                Console.WriteLine(mat);

            Console.WriteLine("\nValid Mats:\n");
            foreach (var mat in validMats)
                Console.WriteLine(mat);

            stopwatch.Stop();
            Console.WriteLine("\nElapsed Time: " + stopwatch.Elapsed);
        }

        static (bool, string) CompareMaterial(Material royalMaterial, string vanillaModelDir, bool useStrictMode)
        {
            List<string> gfsFileNames = Directory.GetFiles($"{vanillaModelDir}", $"*.GFS", SearchOption.AllDirectories).ToList();

            var asSpan = CollectionsMarshal.AsSpan(gfsFileNames);

            foreach (string gfsFile in asSpan)
            {
                var vanillaMats = LoadModel(gfsFile).Materials.Materials;

                foreach(var material in vanillaMats)
                {
                    if (!AreMatFlagsEqual(material.Flags, royalMaterial.Flags))
                        continue;
                    if (!AreColorsEqual(material.AmbientColor, royalMaterial.AmbientColor) && useStrictMode)
                        continue;
                    if (!AreColorsEqual(material.DiffuseColor, royalMaterial.DiffuseColor) && useStrictMode)
                        continue;
                    if (!AreColorsEqual(material.SpecularColor, royalMaterial.SpecularColor) && useStrictMode)
                        continue;
                    if (!AreColorsEqual(material.EmissiveColor, royalMaterial.EmissiveColor) && useStrictMode)
                        continue;
                    if (!AreEqual(material.Field40, royalMaterial.Field40) && useStrictMode) //reflectivity
                        continue;
                    if (!AreEqual(material.Field44, royalMaterial.Field44) && useStrictMode) //outline index
                        continue;
                    if (!AreEqual((byte)material.DrawMethod, (byte)royalMaterial.DrawMethod))
                        continue;
                    if (!AreEqual(material.Field49, royalMaterial.Field49))
                        continue;
                    if (!AreEqual(material.Field4A, royalMaterial.Field4A) && useStrictMode)
                        continue;
                    if (!AreEqual(material.Field4B, royalMaterial.Field4B))
                        continue;
                    if (!AreEqual(material.Field4C, royalMaterial.Field4C) && useStrictMode)
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
                    if (!AreEqual(material.DisableBackfaceCulling, royalMaterial.DisableBackfaceCulling) && useStrictMode)
                        continue;
                    if (!AreEqual(material.Field98, royalMaterial.Field98) && useStrictMode)
                        continue;

                    return (true, material.Name);
                }
            }

            return (false, null);
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
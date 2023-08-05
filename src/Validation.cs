using GFDLibrary.Materials;
using static P5MatValidator.Comparisons;
using static P5MatValidator.Program;

namespace P5MatValidator
{
    internal class Validation
    {
        internal static async Task<(List<string> invalidMats, List<string> validMats, List<string> sameNameMats)> ValidateMaterials(string inputModelPath, string referenceMaterialPath)
        {
            //Get both material lists
            (MaterialInfo compareMaterials, List<MaterialInfo> referenceMaterials) = await PrepareMaterialLists(inputModelPath, referenceMaterialPath);

            //compare all materials
            (List<string> invalidMats, List<string> validMats, List<string> sameNameMats) = await CompareAllMaterials(compareMaterials, referenceMaterials);

            //print results
            PrintResults(inputModelPath, invalidMats, validMats, sameNameMats);

            return (invalidMats, validMats, sameNameMats);
        }
        internal static void PrintResults(string filePath, List<string> InvalidMats, List<string> validMats, List<string> sameNameMats)
        {
            Console.Clear();

            if (validMats.Count > 0)
            {
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
            }

            //Matching Names
            if (sameNameMats.Count > 0)
            {
                if ((mode & Mode.strict) <= 0)
                    Console.WriteLine($"Invalid Mats With Matching Names ({sameNameMats.Count}):\n");
                else
                    Console.WriteLine($"Invalid Mats With Matching Names (Strict Mode) ({sameNameMats.Count}):\n");

                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var mat in sameNameMats)
                    Console.WriteLine(mat);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("===============================================");
            }

            //Invalid Mats
            if (InvalidMats.Count > 0)
            {
                if ((mode & Mode.strict) <= 0)
                    Console.WriteLine($"Invalid Mats ({InvalidMats.Count}):\n");
                else
                    Console.WriteLine($"Invalid Mats (Strict Mode) ({InvalidMats.Count}):\n");

                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var mat in InvalidMats)
                    Console.WriteLine(mat);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("===============================================");
            }

            Stopwatch.Stop();
            Console.WriteLine($"\nElapsed Time: {Stopwatch.Elapsed}");
        }
        internal static async Task<(List<string>, List<string>, List<string>)> CompareAllMaterials(MaterialInfo compareMaterials, List<MaterialInfo> referenceMaterials)
        {
            List<string> invalidMats = new();
            List<string> validMats = new();
            List<string> sameNameMats = new();

            List<Task<(string compareMaterialName, byte isValid, string matchingMaterialName)>> compareTasks = new();

            foreach (var material in compareMaterials.materials)
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
                    sameNameMats.Add($"{result.compareMaterialName} -> {result.matchingMaterialName}");
                }
                else //valid
                {
                    validMats.Add($"{result.compareMaterialName} -> {result.matchingMaterialName}");
                }
            }

            return (invalidMats, validMats, sameNameMats);
        }

        internal static async Task<(string, byte, string?)> CompareMaterial(Material royalMaterial, List<MaterialInfo> referenceMaterials)
        {
            byte validity = 0;
            string? matchingMat = null;

            foreach (var materialInfo in referenceMaterials)
            {
                foreach (var material in materialInfo.materials)
                {
                    if (material.Name == royalMaterial.Name)
                    {
                        validity = 2;
                        matchingMat = materialInfo.fileName;
                    }
                    if ((mode & Mode.strict) > 0) //check these in strict mode
                    {
                        if (!AreColorsEqual(material.AmbientColor, royalMaterial.AmbientColor))
                            continue;
                        if (!AreColorsEqual(material.DiffuseColor, royalMaterial.DiffuseColor))
                            continue;
                        if (!AreColorsEqual(material.SpecularColor, royalMaterial.SpecularColor))
                            continue;
                        if (!AreColorsEqual(material.EmissiveColor, royalMaterial.EmissiveColor))
                            continue;
                        if (!AreEqual(material.Field40, royalMaterial.Field40)) //reflectivity
                            continue;
                        if (!AreEqual(material.Field44, royalMaterial.Field44)) //outline index
                            continue;
                        if (!AreEqual(material.Field4A, royalMaterial.Field4A))
                            continue;
                        if (!AreEqual(material.Field4C, royalMaterial.Field4C))
                            continue;
                        if (!AreEqual(material.DisableBackfaceCulling, royalMaterial.DisableBackfaceCulling))
                            continue;
                        if (!AreEqual(material.Field98, royalMaterial.Field98))
                            continue;
                    }
                    if (!AreMatFlagsEqual(material.Flags, royalMaterial.Flags))
                        continue;
                    if (!AreEqual((byte)material.DrawMethod, (byte)royalMaterial.DrawMethod))
                        continue;
                    if (!AreEqual(material.Field49, royalMaterial.Field49))
                        continue;
                    if (!AreEqual(material.Field4B, royalMaterial.Field4B))
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
                    if (!AreAttributesEqual(material, royalMaterial))
                        continue;

                    validity = 1;
                    matchingMat = $"{material.Name} ({materialInfo.fileName})";
                    break;
                }

                if (validity == 1)
                    break;
            }

            return (royalMaterial.Name, validity, matchingMat);
        }
    }
}

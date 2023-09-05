using GFDLibrary;
using GFDLibrary.Materials;
using System.Diagnostics;
using static P5MatValidator.Converter;

namespace P5MatValidator
{
    internal static class MaterialTester
    {
        internal static void TestAllMaterials(Resource resource, string yamlPresetPath, string resourceOutputPath, string cpkMakePath, string modOutputPath, int batchSize = 1)
        {
            MaterialDictionary inputMatDict;
            List<string> crashyMats = new();

            if (resource.ResourceType == ResourceType.ModelPack)
                inputMatDict = ((ModelPack)resource).Materials;
            else if (resource.ResourceType == ResourceType.MaterialDictionary)
                inputMatDict = (MaterialDictionary)resource;
            else
                throw new Exception($"Expected resource of type \"ModelPack\" or \"MaterialDictionary\". Got type \"{resource.ResourceType}\"");

            //populate newMatDict with preset mats
            MaterialDictionary newMatDict = ConvertAllToPreset(inputMatDict, yamlPresetPath);

            for(int i = 0; i < inputMatDict.Materials.Count; i += batchSize)
            {
                for (int j = i; j < i + batchSize; j++)
                {
                    if (j >= inputMatDict.Materials.Count)
                    {
                        newMatDict.Add(inputMatDict.Materials[j]);
                    }
                }

                SaveAndBuild(resource, newMatDict, resourceOutputPath, cpkMakePath, modOutputPath);

                Console.Write($"\nTest {inputMatDict.Materials[i].Name}. ({i + 1}/{inputMatDict.Materials.Count}) Does it work? (Y/N): ");
                var keyPress = Console.ReadKey();
                Console.WriteLine();

                while (char.ToLower(keyPress.KeyChar) != 'y' && char.ToLower(keyPress.KeyChar) != 'n')
                {
                    Console.Write("\nInvalid Key Press, please press either Y or N: ");
                    keyPress = Console.ReadKey();
                }

                if (char.ToLower(keyPress.KeyChar) == 'n')
                {
                    for (int j = i; j < i + batchSize; j++)
                    {
                        if (j >= inputMatDict.Materials.Count)
                        {
                            newMatDict.Add(GetPresetMaterial(inputMatDict.Materials[j], yamlPresetPath));
                            crashyMats.Add(inputMatDict.Materials[j].Name);
                        }
                    }
                }
            }

            SaveAndBuild(resource, newMatDict, resourceOutputPath, cpkMakePath, modOutputPath);

            Console.WriteLine("Crashy Mats:");

            crashyMats.ForEach(Console.WriteLine);
        }

        internal static void SaveAndBuild(Resource resource, MaterialDictionary matDict, string resourceOutputPath, string cpkMakePath, string modOutputPath)
        {
            if (resource.ResourceType == ResourceType.ModelPack)
            {
                ((ModelPack)resource).Materials = matDict;
                resource.Save(resourceOutputPath);
            }
            else if (resource.ResourceType == ResourceType.MaterialDictionary)
            {
                matDict.Save(resourceOutputPath);
            }
            else
                throw new Exception($"Expected resource of type \"ModelPack\" or \"MaterialDictionary\". Got type \"{resource.ResourceType}\"");

            var cpkmakeproc = new Process();

            cpkmakeproc.StartInfo.FileName = cpkMakePath;
            cpkmakeproc.StartInfo.Arguments = $"{modOutputPath} {modOutputPath}.cpk -mode=FILENAME -crc";

            cpkmakeproc.Start();
            cpkmakeproc.WaitForExit();
        }
    }
}

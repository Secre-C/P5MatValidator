using GFDLibrary;
using GFDLibrary.Materials;
using System.Diagnostics;
using static P5MatValidator.Converter;

namespace P5MatValidator
{
    internal static class MaterialTester
    {
        internal static void TestAllMaterials(Resource resource, string yamlPresetPath, string resourceOutputPath, string cpkMakePath, string modOutputPath)
        {
            MaterialDictionary inputMatDict;

            if (resource.ResourceType == ResourceType.ModelPack)
                inputMatDict = ((ModelPack)resource).Materials;
            else if (resource.ResourceType == ResourceType.MaterialDictionary)
                inputMatDict = (MaterialDictionary)resource;
            else
                throw new Exception($"Expected resource of type \"ModelPack\" or \"MaterialDictionary\". Got type \"{resource.ResourceType}\"");

            //populate newMatDict with preset mats
            MaterialDictionary newMatDict = ConvertAllToPreset(inputMatDict, yamlPresetPath);

            for(int i = 0; i < inputMatDict.Materials.Count; i++)
            {
                //put mattest and overwrite thing here
                newMatDict.Add(inputMatDict.Materials[i]);
                SaveAndBuild(resource, newMatDict, resourceOutputPath, cpkMakePath, modOutputPath);

                Console.Write($"\nTest {inputMatDict.Materials[i].Name}. Does it work? (Y/N) if it's preset just skip lmao: ");
                var keyPress = Console.ReadKey();

                while (char.ToLower(keyPress.KeyChar) != 'y' && char.ToLower(keyPress.KeyChar) != 'n')
                {
                    Console.Write("\nInvalid Key Press, please press either Y or N: ");
                    keyPress = Console.ReadKey();
                }

                if (char.ToLower(keyPress.KeyChar) == 'n')
                {
                    newMatDict.Add(GetPresetMaterial(inputMatDict.Materials[i], yamlPresetPath));
                }
            }

            SaveAndBuild(resource, newMatDict, resourceOutputPath, cpkMakePath, modOutputPath);
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

using GFDLibrary;
using GFDLibrary.Materials;
using System.Diagnostics;
using static P5MatValidator.Converter;

namespace P5MatValidator
{
    internal static class MaterialTester
    {
        internal static void TestMaterials(InputHandler inputHandler)
        {
            string resourceInput = inputHandler.GetParameterValue("i");
            string modOutputPath = inputHandler.GetParameterValue("o");
            string yamlPresetPath = inputHandler.GetParameterValue("preset");
            string cpkMakePath = inputHandler.GetParameterValue("cpkmakec");
            inputHandler.TryGetParameterValue("mats", out string referenceMatPath);
            bool onlyTestInvalid = inputHandler.HasCommand("onlyinvalid");
            bool strict = inputHandler.HasCommand("strict");
            int batchSize = inputHandler.TryGetParameterValue("batch", out string s_batchSize) ? int.TryParse(s_batchSize, out int i_batchSize) ? i_batchSize : 1 : 1;

            if (onlyTestInvalid)
                TestInvalidMaterials(resourceInput, modOutputPath, yamlPresetPath, cpkMakePath, referenceMatPath, strict);
            else
                TestAllMaterials(resourceInput, modOutputPath, yamlPresetPath, cpkMakePath, batchSize);
        }
        internal static void TestAllMaterials
            (string resourceInput, string modOutputPath, string yamlPresetPath, string cpkMakePath, int batchSize)
        {
            var resource = Resource.Load(resourceInput);
            MaterialDictionary inputMatDict;
            List<string> crashyMats = new();

            if (resource.ResourceType == ResourceType.ModelPack)
                inputMatDict = ((ModelPack)resource).Materials;
            else if (resource.ResourceType == ResourceType.MaterialDictionary)
                inputMatDict = (MaterialDictionary)resource;
            else
                throw new Exception($"Expected resource of type \"ModelPack\" or \"MaterialDictionary\". Got type \"{resource.ResourceType}\"");

            //populate newMatDict with preset mats
            var newMatDict = ConvertAllToPreset(inputMatDict, yamlPresetPath);

            for (int i = 0; i < inputMatDict.Materials.Count; i += batchSize)
            {
                for (int j = i; j < i + batchSize; j++)
                {
                    if (j >= inputMatDict.Materials.Count)
                    {
                        newMatDict.Add(inputMatDict.Materials[j]);
                    }
                }

                SaveAndBuild(resource, newMatDict, resourceInput, cpkMakePath, modOutputPath);

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

            SaveAndBuild(resource, newMatDict, resourceInput, cpkMakePath, modOutputPath);

            Console.WriteLine("Crashy Mats:");

            crashyMats.ForEach(Console.WriteLine);
        }

        internal static void TestInvalidMaterials
            (string resourceInput, string modOutputPath, string yamlPresetPath, string cpkMakePath, string referenceMatPath, bool strict = false)
        {
            var resources = new MaterialResources(resourceInput, referenceMatPath);
            var inputMatDict = resources.MaterialDictionary;
            List<string> crashyMats = new();

            var validator = new Validator(resources, strict);
            validator.RunValidation();
            var invalidMats = validator.materialValidationResults.Where(i => i.validity != Validator.MaterialValidity.Valid).ToList();
            Console.WriteLine($"invalid - {invalidMats.Count}");

            var newMatDict = ConvertInvalidToPreset(inputMatDict, yamlPresetPath, invalidMats);

            for (int i = 0; i < invalidMats.Count; i++)
            {
                newMatDict[invalidMats[i].material.Name] = inputMatDict[invalidMats[i].material.Name];

                SaveAndBuild(resources.Resource, newMatDict, resourceInput, cpkMakePath, modOutputPath);

                Console.Write($"\nTest {invalidMats[i].material.Name}. ({i + 1}/{invalidMats.Count}) Does it work? (Y/N): ");
                var keyPress = Console.ReadKey();
                Console.WriteLine();

                while (char.ToLower(keyPress.KeyChar) != 'y' && char.ToLower(keyPress.KeyChar) != 'n')
                {
                    Console.Write("\nInvalid Key Press, please press either Y or N: ");
                    keyPress = Console.ReadKey();
                }

                if (char.ToLower(keyPress.KeyChar) == 'n')
                {
                    newMatDict.Add(GetPresetMaterial(inputMatDict[invalidMats[i].material.Name], yamlPresetPath));
                    crashyMats.Add(invalidMats[i].material.Name);
                }
            }

            SaveAndBuild(resources.Resource, newMatDict, resourceInput, cpkMakePath, modOutputPath);

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
            cpkmakeproc.StartInfo.Arguments = $"\"{modOutputPath}\" \"{modOutputPath}\".cpk -mode=FILENAME -crc";

            cpkmakeproc.Start();
            cpkmakeproc.WaitForExit();
        }
    }
}

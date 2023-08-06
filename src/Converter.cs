using GFDLibrary;
using GFDLibrary.Materials;
using static GFDLibrary.Api.FlatApi;

namespace P5MatValidator
{
    internal class Converter
    {
        internal static void ConvertAllInvalidMaterials(ModelPack modelFile, string modelOutputPath, string presetYamlPath, Validator validatorResults)
        {
            bool usePresets = presetYamlPath != null;
            var newDict = new MaterialDictionary();

            for (int i = 0; i < modelFile.Materials.Materials.Count; i++)
            {
                if (validatorResults.IsMaterialValid(modelFile.Materials.Materials[i].Name))
                {
                    newDict.Add(ConvertMaterial(modelFile.Materials.Materials[i], presetYamlPath, usePresets));
                }
                else
                {
                    newDict.Add(modelFile.Materials.Materials[i]);
                }
            }

            modelFile.Materials = newDict;
            modelFile.Save(modelOutputPath);
            
            Console.WriteLine(modelOutputPath);
        }

        internal static Material ConvertMaterial(Material inputMaterial, string presetYamlPath, bool usePresets)
        {
            if (!usePresets && TryFindReplacementMat(inputMaterial, out Material outputMaterial))
                return outputMaterial;
            else
                return GetPresetMaterial(inputMaterial, presetYamlPath);
        }

        internal static bool TryFindReplacementMat(Material inputMaterial, out Material outputMaterial) 
        {
            throw new NotImplementedException($"If you see this, it's a bug!");
            outputMaterial = null;
            return false;
        }
        internal static Material GetPresetMaterial(Material inputMaterial, string presetYamlPath)
        {
            Material newMaterial;
            string name = inputMaterial.Name;

            if (inputMaterial.Flags.HasFlag(MaterialFlags.HasDiffuseMap))
            {
                string diffuseMapName = inputMaterial.DiffuseMap.Name;
                newMaterial = YamlSerializer.LoadYamlFile<Material>(presetYamlPath);
                newMaterial.Name = name;
                newMaterial.DiffuseMap.Name = diffuseMapName;
            }
            else
            {
                name = inputMaterial.Name;
                var defaultYamlPath = Path.GetDirectoryName(presetYamlPath) + "\\1_gfdDefaultMat0.yml";
                newMaterial = YamlSerializer.LoadYamlFile<Material>(defaultYamlPath);
            }

            newMaterial.Name = name;

            return newMaterial;
        }
    }
}

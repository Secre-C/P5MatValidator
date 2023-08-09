using GFDLibrary;
using GFDLibrary.Materials;
using static GFDLibrary.Api.FlatApi;
using static P5MatValidator.MaterialComparer;
using static P5MatValidator.MaterialSearcher;

namespace P5MatValidator
{
    internal class Converter
    {
        internal static void ConvertAllInvalidMaterials(Resource resource, InputHandler inputHandler, Validator validatorResults, string outputPath, MaterialResources? materialResources = null)
        {
            MaterialDictionary matDict;

            if (resource.ResourceType == ResourceType.ModelPack)
                matDict = ((ModelPack)resource).Materials;
            else if (resource.ResourceType == ResourceType.MaterialDictionary)
                matDict = (MaterialDictionary)resource;
            else
                throw new Exception($"Expected resource of type \"ModelPack\" or \"MaterialDictionary\". Got type \"{resource.ResourceType}\"");

            MaterialDictionary newDict = new();

            for (int i = 0; i < matDict.Materials.Count; i++)
            {
                if (validatorResults.IsMaterialValid(matDict.Materials[i].Name))
                {
                    newDict.Add(ConvertMaterial(matDict.Materials[i], inputHandler, materialResources));
                }
                else
                {
                    newDict.Add(matDict.Materials[i]);
                }
            }

            matDict = newDict;

            if (resource.ResourceType == ResourceType.ModelPack)
            {
                ((ModelPack)resource).Materials = matDict;
                resource.Save(outputPath);
            }
            else
            {
                matDict.Save(outputPath);
            }
        }

        internal static Material ConvertMaterial(Material inputMaterial, InputHandler inputHandler, MaterialResources? materialResource = null)
        {
            bool useOnlyPreset = inputHandler.TryGetCommand("onlypreset");
            string presetYamlPath = inputHandler.GetParameterValue("preset");
            bool useStrictMode = inputHandler.TryGetCommand("strict");
            int maximumPoints = int.Parse(inputHandler.TryGetParameterValue("points", out string maxPoints) ? maxPoints : "-1");
            uint texcoordAccuracy = uint.Parse(inputHandler.TryGetParameterValue("accuracy", out string texAccuracy) ? texAccuracy : "0");

            if (!useOnlyPreset && TryFindReplacementMat(inputMaterial, materialResource, out List<MaterialComparer>? outputMaterial, useStrictMode, maximumPoints, texcoordAccuracy) 
                && outputMaterial != null)
                return outputMaterial[0].material;
            else
                return GetPresetMaterial(inputMaterial, presetYamlPath);
        }
        internal static void OverWriteMaterialEssentials(Material referenceMaterial, Material inputMaterial)
        {
            referenceMaterial.Name = inputMaterial.Name;

            referenceMaterial.AmbientColor = inputMaterial.AmbientColor;
            referenceMaterial.DiffuseColor = inputMaterial.DiffuseColor;
            referenceMaterial.SpecularColor = inputMaterial.SpecularColor;
            referenceMaterial.EmissiveColor = inputMaterial.EmissiveColor;

            referenceMaterial.DiffuseMap = inputMaterial.DiffuseMap;
            referenceMaterial.NormalMap = inputMaterial.NormalMap;
            referenceMaterial.SpecularMap = inputMaterial.SpecularMap;
            referenceMaterial.ReflectionMap = inputMaterial.ReflectionMap;
            referenceMaterial.HighlightMap = inputMaterial.HighlightMap;
            referenceMaterial.GlowMap = inputMaterial.GlowMap;
            referenceMaterial.NightMap = inputMaterial.NightMap;
            referenceMaterial.DetailMap = inputMaterial.DetailMap;
            referenceMaterial.ShadowMap = inputMaterial.ShadowMap;
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

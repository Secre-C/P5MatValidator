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

            Console.WriteLine('\n');
            for (int i = 0; i < matDict.Materials.Count; i++)
            {
                if (!validatorResults.IsMaterialValid(matDict.Materials[i].Name))
                {
                    newDict.Add(ConvertMaterial(matDict.Materials[i], inputHandler, materialResources));
                }
                else
                {
                    newDict.Add(matDict.Materials[i]);
                }
            }

            if (resource.ResourceType == ResourceType.ModelPack)
            {
                ((ModelPack)resource).Materials = newDict;
                resource.Save(outputPath);
            }
            else
            {
                newDict.Save(outputPath);
            }
        }

        internal static Material ConvertMaterial(Material inputMaterial, InputHandler inputHandler, MaterialResources? materialResource = null)
        {
            bool useOnlyPreset = inputHandler.TryGetCommand("onlypreset");
            string presetYamlPath = inputHandler.GetParameterValue("preset");
            bool useStrictMode = inputHandler.TryGetCommand("strict");
            int maximumPoints = int.Parse(inputHandler.TryGetParameterValue("points", out string maxPoints) ? maxPoints : "20");
            uint texcoordAccuracy = uint.Parse(inputHandler.TryGetParameterValue("accuracy", out string texAccuracy) ? texAccuracy : "1");

            if (!useOnlyPreset && TryFindReplacementMat(inputMaterial, materialResource, out List<MaterialComparer>? outputMaterial, useStrictMode, maximumPoints, texcoordAccuracy) 
                && outputMaterial != null)
            {
                Console.WriteLine($"replacing {inputMaterial.Name} with {outputMaterial[0].material.Name} from {outputMaterial[0].materialFilename}");
                UpdateMaterialValues(outputMaterial[0].material, inputMaterial);
                return inputMaterial;
            }
            else
                return GetPresetMaterial(inputMaterial, presetYamlPath);
        }
        internal static void UpdateMaterialValues(Material referenceMaterial, Material inputMaterial)
        {
            //referenceMaterial.Name = inputMaterial.Name;
            //
            //inputMaterial.AmbientColor = referenceMaterial.AmbientColor;
            //inputMaterial.DiffuseColor = referenceMaterial.DiffuseColor;
            //inputMaterial.SpecularColor = referenceMaterial.SpecularColor;
            //inputMaterial.EmissiveColor = referenceMaterial.EmissiveColor;

            inputMaterial.Flags = referenceMaterial.Flags;
            inputMaterial.Field40 = referenceMaterial.Field40;
            inputMaterial.Field44 = referenceMaterial.Field44;
            inputMaterial.DrawMethod = referenceMaterial.DrawMethod;
            inputMaterial.Field49 = referenceMaterial.Field49;
            inputMaterial.Field4A = referenceMaterial.Field4A;
            inputMaterial.Field4B = referenceMaterial.Field4B;
            inputMaterial.Field4C = referenceMaterial.Field4C;
            inputMaterial.Field4D = referenceMaterial.Field4D;
            inputMaterial.Field90 = referenceMaterial.Field90;
            inputMaterial.Field94 = referenceMaterial.Field94;
            inputMaterial.Field96 = referenceMaterial.Field96;
            inputMaterial.Field5C = referenceMaterial.Field5C;
            inputMaterial.Field6C = referenceMaterial.Field6C;
            inputMaterial.Field70 = referenceMaterial.Field70;
            inputMaterial.DisableBackfaceCulling = referenceMaterial.DisableBackfaceCulling;
            inputMaterial.Field98 = referenceMaterial.Field98;

            inputMaterial.Attributes = referenceMaterial.Attributes;

            if (referenceMaterial.DiffuseMap  == null)
                inputMaterial.DiffuseMap = null;
            if (referenceMaterial.NormalMap == null)
                inputMaterial.NormalMap = null;
            if (referenceMaterial.SpecularMap == null)
                inputMaterial.SpecularMap = null;
            if (referenceMaterial.ReflectionMap == null)
                inputMaterial.ReflectionMap = null;
            if (referenceMaterial.HighlightMap == null)
                inputMaterial.HighlightMap = null;
            if (referenceMaterial.GlowMap == null)
                inputMaterial.GlowMap = null;
            if (referenceMaterial.NightMap == null)
                inputMaterial.NightMap = null;
            if (referenceMaterial.DetailMap == null)
                inputMaterial.DetailMap = null;
            if (referenceMaterial.ShadowMap == null)
                inputMaterial.ShadowMap = null;
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

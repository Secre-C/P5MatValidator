using GFDLibrary;
using GFDLibrary.Materials;
using static P5MatValidator.MaterialComparer;
using static P5MatValidator.MaterialSearcher;

namespace P5MatValidator
{
    internal static class Converter
    {
        internal static Resource ConvertAllInvalidMaterials(Resource resource, InputHandler inputHandler, Validator validatorResults, MaterialResources materialResources)
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
                return resource;
            }
            else
            {
                return newDict;
            }
        }

        internal static Material ConvertMaterial(Material inputMaterial, InputHandler inputHandler, MaterialResources? materialResource = null)
        {
            bool useOnlyPreset = inputHandler.HasCommand("onlypreset");
            string presetYamlPath = inputHandler.GetParameterValue("preset");
            int maximumPoints = int.Parse(inputHandler.TryGetParameterValue("points", out string maxPoints) ? maxPoints : "20");
            uint texcoordAccuracy = uint.Parse(inputHandler.TryGetParameterValue("accuracy", out string texAccuracy) ? texAccuracy : "2");

            if (!useOnlyPreset && TryFindReplacementMat(inputMaterial, materialResource, out var outputMaterial, false, maximumPoints, texcoordAccuracy)
                && outputMaterial != null)
            {
                Console.WriteLine($"replacing {inputMaterial.Name} with {outputMaterial[0].material.Name} from {outputMaterial[0].materialFilename}");
                CopyMaterialValues(outputMaterial[0].material, inputMaterial);
                return inputMaterial;
            }
            else
            {
                if (!useOnlyPreset)
                    Console.WriteLine($"Failed to Convert {inputMaterial.Name}");

                return GetPresetMaterial(inputMaterial, presetYamlPath);
            }
        }
        internal static void CopyMaterialValues(Material referenceMaterial, Material inputMaterial)
        {
            //referenceMaterial.Name = inputMaterial.Name;
            //
            //inputMaterial.AmbientColor = referenceMaterial.AmbientColor;
            //inputMaterial.DiffuseColor = referenceMaterial.DiffuseColor;
            //inputMaterial.SpecularColor = referenceMaterial.SpecularColor;
            //inputMaterial.EmissiveColor = referenceMaterial.EmissiveColor;

            inputMaterial.Flags = referenceMaterial.Flags;

            if ((inputMaterial.Field40 == 1) != (referenceMaterial.Field40 == 1))
                inputMaterial.Field40 = referenceMaterial.Field40;

            if ((inputMaterial.Field44 == 0) != (referenceMaterial.Field44 == 0))
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
            //inputMaterial.DisableBackfaceCulling = referenceMaterial.DisableBackfaceCulling;
            inputMaterial.Field98 = referenceMaterial.Field98;

            //copy material attributes other than the flag
            CopyAttributes(referenceMaterial, inputMaterial);

            if (referenceMaterial.DiffuseMap == null)
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
        internal static void CopyAttributes(Material referenceMaterial, Material inputMaterial)
        {

            for (int i = 0; i < 8; i++)
            {
                if (HasAttributeOfType(inputMaterial, i, out int inputAttrIndex) && HasAttributeOfType(referenceMaterial, i, out int refAttrIndex))
                {
                    if (i == 0)
                    {
                        var inputAttr = (MaterialAttributeType0)inputMaterial.Attributes[inputAttrIndex];
                        var refAttr = (MaterialAttributeType0)referenceMaterial.Attributes[refAttrIndex];
                        inputAttr.Type0Flags = refAttr.Type0Flags;
                    }
                    else if (i == 1)
                    {
                        var inputAttr = (MaterialAttributeType1)inputMaterial.Attributes[inputAttrIndex];
                        var refAttr = (MaterialAttributeType1)referenceMaterial.Attributes[refAttrIndex];
                        inputAttr.Type1Flags = refAttr.Type1Flags;
                    }
                    else if (i == 4)
                    {
                        var inputAttr = (MaterialAttributeType4)inputMaterial.Attributes[inputAttrIndex];
                        var refAttr = (MaterialAttributeType4)referenceMaterial.Attributes[refAttrIndex];
                        inputAttr.Field5C = refAttr.Field5C;
                    }
                    else
                        inputMaterial.Attributes[inputAttrIndex] = referenceMaterial.Attributes[refAttrIndex];
                }
            }
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
                string defaultYamlPath = Path.GetDirectoryName(presetYamlPath) + "\\1_gfdDefaultMat0.yml";
                newMaterial = YamlSerializer.LoadYamlFile<Material>(defaultYamlPath);
            }

            newMaterial.Name = name;

            return newMaterial;
        }

        internal static MaterialDictionary ConvertAllToPreset(MaterialDictionary inputDict, string yamlPresetPath)
        {
            MaterialDictionary newMaterialDict = new();

            foreach (var mat in inputDict.Materials)
            {
                newMaterialDict.Add(mat.Name, GetPresetMaterial(mat, yamlPresetPath));
            }

            return newMaterialDict;
        }

        internal static MaterialDictionary ConvertInvalidToPreset(MaterialDictionary inputDict, string yamlPresetPath, List<Validator.MaterialValidationResult> invalidMats)
        {
            MaterialDictionary newMaterialDict = new();

            foreach (var mat in inputDict.Materials)
            {
                newMaterialDict.Add(inputDict[mat.Name]);
            }

            foreach (var mat in invalidMats)
            {
                newMaterialDict[mat.material.Name] = GetPresetMaterial(inputDict[mat.material.Name], yamlPresetPath);
            }

            return newMaterialDict;
        }
    }
}

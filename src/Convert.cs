using GFDLibrary.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P5MatValidator.Program;
using static GFDLibrary.Api.FlatApi;
using GFDLibrary;
using System.Drawing;
using System.Xml.Linq;

namespace P5MatValidator
{
    internal class Convert
    {
        internal static void ConvertInvalidMaterialsToPreset(string modelPath, string presetYamlPath, List<string> invalidMats, List<string> sameNameMats)
        {
            List<string> fixedInvalidMats = FixMatNames(invalidMats);
            List<string> fixedSameNameMats = FixMatNames(sameNameMats);

            List<string> failedMats = new List<string>();

            var newDict = new MaterialDictionary();

            var modelFile = LoadModel(modelPath);

            for (int i = 0; i < modelFile.Materials.Materials.Count; i++)
            {
                if (fixedInvalidMats.Contains(modelFile.Materials.Materials[i].Name) || fixedSameNameMats.Contains(modelFile.Materials.Materials[i].Name))
                {
                    Material newMaterial;
                    string name = modelFile.Materials.Materials[i].Name;

                    if (modelFile.Materials.Materials[i].Flags.HasFlag(MaterialFlags.HasDiffuseMap))
                    {
                        string diffuseMapName = modelFile.Materials.Materials[i].DiffuseMap.Name;
                        newMaterial = YamlSerializer.LoadYamlFile<Material>(presetYamlPath);
                        newMaterial.Name = name;
                        newMaterial.DiffuseMap.Name = diffuseMapName;
                    }
                    else
                    {
                        name = modelFile.Materials.Materials[i].Name;
                        var defaultYamlPath = Path.GetDirectoryName(presetYamlPath) + "\\1_gfdDefaultMat0.yml";
                        newMaterial = YamlSerializer.LoadYamlFile<Material>(defaultYamlPath);
                    }
                    newMaterial.Name = name;
                    newDict.Add(newMaterial);
                }
                else
                {
                    newDict.Add(modelFile.Materials.Materials[i]);
                }
            }

            modelFile.Materials = newDict;
            modelFile.Save(modelPath);
        }

        private static List<string> FixMatNames(List<string> mats)
        {
            for (int i = 0; i < mats.Count; i++)
            {
                mats[i] = mats[i].Split(" ->")[0];
            }

            return mats;
        }
    }
}

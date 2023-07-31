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

            string diffuseMapName = "pls replace";

            for (int i = 0; i < modelFile.Materials.Materials.Count; i++)
            {
                if (fixedInvalidMats.Contains(modelFile.Materials.Materials[i].Name) || fixedSameNameMats.Contains(modelFile.Materials.Materials[i].Name))
                {
                    if (modelFile.Materials.Materials[i].Flags.HasFlag(MaterialFlags.HasDiffuseMap))
                    {
                        string name = modelFile.Materials.Materials[i].Name;

                        diffuseMapName = modelFile.Materials.Materials[i].DiffuseMap.Name;
                        var newMaterial = YamlSerializer.LoadYamlFile<Material>(presetYamlPath);
                        newMaterial.Name = name;
                        newMaterial.DiffuseMap.Name = diffuseMapName;
                        newDict.Add(newMaterial);
                    }
                    else
                    {
                        failedMats.Add(modelFile.Materials.Materials[i].Name);
                        newDict.Add(modelFile.Materials.Materials[i]);
                    }
                }
                else
                {
                    newDict.Add(modelFile.Materials.Materials[i]);
                }
            }

            modelFile.Materials = newDict;
            modelFile.Save(modelPath);

            if (failedMats.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to automatically convert the following mats, probably because of no diffuse map:\n");

                foreach (var mat in failedMats)
                {
                    Console.WriteLine(mat);
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
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

﻿using GFDLibrary.Materials;
using static P5MatValidator.MaterialComparer;

namespace P5MatValidator
{
    internal class Validator
    {
        internal List<MaterialValidationResult> materialValidationResults = new();
        private readonly MaterialResources materialResources;
        private readonly bool useStrictCompare = false;

        internal Validator(MaterialResources materialResources, bool useStrictCompare)
            : this(materialResources)
        {
            this.useStrictCompare = useStrictCompare;
        }

        internal Validator(MaterialResources materialResources)
        {
            this.materialResources = materialResources;
        }

        internal void RunValidation()
        {
            CompareAllMaterials();
        }

        internal void PrintValidationResults()
        {
            List<string> validMats = new();
            List<string> invalidMats = new();
            List<string> sameNameMats = new();

            foreach (var material in materialValidationResults)
            {
                if (material.validity == MaterialValidity.Valid)
                {
                    validMats.Add($"{material.material.Name} -> {material.material.Name}");
                }
                else if (material.validity == MaterialValidity.Invalid)
                {
                    invalidMats.Add($"{material.material.Name}");
                }
                else if (material.validity == MaterialValidity.SameName)
                {
                    sameNameMats.Add($"{material.material.Name} -> {material.material.Name}");
                }
            }

            if (validMats.Count > 0)
            {
                //Valid Mats
                Console.WriteLine("\n===============================================");
                Console.WriteLine($"{Path.GetFileName(materialResources.InputFilePath)}:");

                Console.WriteLine("===============================================");

                Console.WriteLine($"Valid Mats ({validMats.Count}):\n");

                Console.ForegroundColor = ConsoleColor.Green;
                foreach (string mat in validMats)
                    Console.WriteLine(mat);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("===============================================");
            }

            //Matching Names
            if (sameNameMats.Count > 0)
            {
                if (!useStrictCompare)
                    Console.WriteLine($"Invalid Mats With Matching Names ({sameNameMats.Count}):\n");
                else
                    Console.WriteLine($"Invalid Mats With Matching Names (Strict Mode) ({sameNameMats.Count}):\n");

                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (string mat in sameNameMats)
                    Console.WriteLine(mat);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("===============================================");
            }

            //Invalid Mats
            if (invalidMats.Count > 0)
            {
                if (!useStrictCompare)
                    Console.WriteLine($"Invalid Mats ({invalidMats.Count}):\n");
                else
                    Console.WriteLine($"Invalid Mats (Strict Mode) ({invalidMats.Count}):\n");

                Console.ForegroundColor = ConsoleColor.Red;
                foreach (string mat in invalidMats)
                    Console.WriteLine(mat);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("===============================================");
            }
        }
        private void CompareAllMaterials()
        {
            List<Task<MaterialValidationResult>> compareTasks = new();
            var materialPoints = MaterialPoints.GetMaterialPoints();

            foreach (var royalMaterialDict in materialResources.MaterialDictionary.Materials)
            {
                Task<MaterialValidationResult> compareTask = new(() => CompareMaterials(royalMaterialDict, materialPoints));
                compareTask.Start();
                compareTasks.Add(compareTask);
            }

            var result = Task.WhenAll(compareTasks).Result;
            materialValidationResults = result.ToList();
        }

        internal MaterialValidationResult CompareMaterials(Material royalMaterial, MaterialPoints materialPoints)
        {
            var validity = MaterialValidity.Invalid;
            string matchingMat = "";

            for (int i = 0; i < materialResources.ReferenceMaterials.Count; i++)
            {
                var referenceMaterial = materialResources.ReferenceMaterials[i];
                for (int j = 0; j < referenceMaterial.materials.Count; j++)
                {
                    var material = referenceMaterial.materials[j];
                    if (material.Name == royalMaterial.Name)
                    {
                        validity = MaterialValidity.SameName;
                        matchingMat = referenceMaterial.fileName;
                    }

                    validity = CompareMaterial(material, royalMaterial, materialPoints, useStrictCompare) == 0 ? MaterialValidity.Valid : validity;

                    if (validity == MaterialValidity.Valid)
                    {
                        matchingMat = $"{material.Name} ({referenceMaterial.fileName})";
                        break;
                    }
                }

                if (validity == MaterialValidity.Valid)
                    break;
            }

            return new MaterialValidationResult
            {
                material = royalMaterial,
                validity = validity,
                matchingMaterialPath = matchingMat
            };
        }

        internal bool IsMaterialValid(string materialName)
        {
            return !materialValidationResults.Any(validatorResult =>
            validatorResult.material.Name == materialName &&
            (validatorResult.validity == MaterialValidity.Invalid || validatorResult.validity == MaterialValidity.SameName));
        }

        internal static bool IsMaterialValid(MaterialValidationResult result)
        {
            return result.validity == MaterialValidity.Valid;
        }

        internal struct MaterialValidationResult
        {
            internal Material material;
            internal MaterialValidity validity;
            internal string matchingMaterialPath;
        }

        internal enum MaterialValidity : int
        {
            Valid,
            Invalid,
            SameName
        }
    }
}

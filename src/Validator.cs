using GFDLibrary.Materials;
using static P5MatValidator.Comparisons;

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

            foreach ( var material in materialValidationResults ) 
            {
                if (material.materialValidity == MaterialValididty.Valid)
                {
                    validMats.Add($"{material.materialName} -> {material.matchingMaterialPath}");
                }
                else if (material.materialValidity == MaterialValididty.Invalid)
                {
                    invalidMats.Add($"{material.materialName}");
                }
                else if (material.materialValidity == MaterialValididty.SameName)
                {
                    sameNameMats.Add($"{material.materialName} -> {material.matchingMaterialPath}");
                }
            }

            if (validMats.Count > 0)
            {
                //Valid Mats
                Console.WriteLine("\n===============================================");
                Console.WriteLine($"{Path.GetFileName(materialResources.inputFilePath)}:");

                Console.WriteLine("===============================================");

                Console.WriteLine($"Valid Mats ({validMats.Count}):\n");

                Console.ForegroundColor = ConsoleColor.Green;
                foreach (var mat in validMats)
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
                foreach (var mat in sameNameMats)
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
                foreach (var mat in invalidMats)
                    Console.WriteLine(mat);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("===============================================");
            }
        }
        private void CompareAllMaterials()
        {
            List<Task<MaterialValidationResult>> compareTasks = new();

            foreach (var material in materialResources.inputMaterials)
            {
                compareTasks.Add(CompareMaterial(material));
            }

            var result = Task.WhenAll(compareTasks).Result;
            materialValidationResults = result.ToList();
        }

        internal async Task<MaterialValidationResult> CompareMaterial(Material royalMaterial)
        {
            MaterialValididty validity = MaterialValididty.Invalid;
            string matchingMat = "";

            foreach (var referenceMaterial in materialResources.referenceMaterials)
            {
                foreach (var material in referenceMaterial.materials)
                {
                    if (material.Name == royalMaterial.Name)
                    {
                        validity = MaterialValididty.SameName;
                        matchingMat = referenceMaterial.fileName;
                    }
                    if (useStrictCompare) //check these in strict mode
                    {
                        if (!AreColorsEqual(material.AmbientColor, royalMaterial.AmbientColor))
                            continue;
                        if (!AreColorsEqual(material.DiffuseColor, royalMaterial.DiffuseColor))
                            continue;
                        if (!AreColorsEqual(material.SpecularColor, royalMaterial.SpecularColor))
                            continue;
                        if (!AreColorsEqual(material.EmissiveColor, royalMaterial.EmissiveColor))
                            continue;
                        if (!AreEqual(material.Field40, royalMaterial.Field40)) //reflectivity
                            continue;
                        if (!AreEqual(material.Field44, royalMaterial.Field44)) //outline index
                            continue;
                        if (!AreEqual(material.Field4A, royalMaterial.Field4A))
                            continue;
                        if (!AreEqual(material.Field4C, royalMaterial.Field4C))
                            continue;
                        if (!AreEqual(material.DisableBackfaceCulling, royalMaterial.DisableBackfaceCulling))
                            continue;
                        if (!AreEqual(material.Field98, royalMaterial.Field98))
                            continue;
                    }
                    if (!AreMatFlagsEqual(material.Flags, royalMaterial.Flags))
                        continue;
                    if (!AreEqual((byte)material.DrawMethod, (byte)royalMaterial.DrawMethod))
                        continue;
                    if (!AreEqual(material.Field49, royalMaterial.Field49))
                        continue;
                    if (!AreEqual(material.Field4B, royalMaterial.Field4B))
                        continue;
                    if (!AreEqual(material.Field4D, royalMaterial.Field4D)) //highlight blend mode
                        continue;
                    if (!AreEqual(material.Field90, royalMaterial.Field90))
                        continue;
                    if (!AreEqual(material.Field92, royalMaterial.Field92))
                        continue;
                    if (!AreEqual(material.Field94, royalMaterial.Field94))
                        continue;
                    if (!AreEqual(material.Field96, royalMaterial.Field96))
                        continue;
                    if (!AreEqual(material.Field5C, royalMaterial.Field5C))
                        continue;
                    if (!AreEqual(material.Field6C, royalMaterial.Field6C)) //texcoord1
                        continue;
                    if (!AreEqual(material.Field70, royalMaterial.Field70)) //texcoord2
                        continue;
                    if (!AreAttributesEqual(material, royalMaterial, useStrictCompare))
                        continue;

                    validity = MaterialValididty.Valid;
                    matchingMat = $"{material.Name} ({referenceMaterial.fileName})";
                    break;
                }

                if (validity == MaterialValididty.Valid)
                    break;
            }

            return new MaterialValidationResult
            {
                materialName = royalMaterial.Name,
                materialValidity = validity,
                matchingMaterialPath = matchingMat
            };
        }

        internal bool IsMaterialValid(string materialName)
        {
            return materialValidationResults.Any(validatorResult => validatorResult.materialName == materialName &&
                (validatorResult.materialValidity == MaterialValididty.Invalid || validatorResult.materialValidity == MaterialValididty.SameName));
        }

        internal bool IsMaterialValid(MaterialValidationResult result)
        {
            return result.materialValidity == MaterialValididty.Valid;
        }

        internal struct MaterialValidationResult
        {
            internal string materialName;
            internal MaterialValididty materialValidity;
            internal string matchingMaterialPath;
        }

        internal enum MaterialValididty : int
        {
            Valid,
            Invalid,
            SameName
        }
    }
}

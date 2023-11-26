using GFDLibrary;
using GFDLibrary.Materials;
using static P5MatValidator.Utils;

namespace P5MatValidator
{
    public class MaterialResources
    {
        internal Resource Resource { get; private set; } = new ModelPack();
        internal string InputFilePath { get; private set; } = string.Empty;
        internal string ReferenceMaterialPath { get; private set; } = string.Empty;
        internal MaterialDictionary MaterialDictionary { get; private set; }
        internal List<ReferenceMaterial> ReferenceMaterials { get; private set; } = new();

        public MaterialResources(string referenceMaterialPath)
        {
            ReferenceMaterialPath = referenceMaterialPath;
            GetReferenceMaterials();
        }
        public MaterialResources(string inputFilePath, string referenceMaterialPath)
        {
            InputFilePath = inputFilePath;
            ReferenceMaterialPath = referenceMaterialPath;

            Resource = Resource.Load(inputFilePath);

            MaterialDictionary = GetMaterialDictionary(Resource);

            GetReferenceMaterials();
        }

        private static MaterialDictionary GetMaterialDictionary(Resource resource)
        {
            //Add List of materials and filename to materialInfo struct
            if (resource.ResourceType == ResourceType.ModelPack)
            {
                var modelPack = (ModelPack)resource;
                return modelPack.Materials;
            }
            else if (resource.ResourceType == ResourceType.MaterialDictionary)
            {
                return (MaterialDictionary)resource;
            }
            else if (resource.ResourceType == ResourceType.Material)
            {
                var mat = (Material)resource;

                var matDict = new MaterialDictionary
                {
                    mat
                };

                return matDict;
            }
            else
            {
                return new MaterialDictionary();
            }
        }

        private void GetReferenceMaterials()
        {
            string[] fileExtensions = { "*.gmtd", "*.gmt", "*.GFS", "*.GMD" };
            var referenceMaterialFiles = GetFiles(ReferenceMaterialPath, fileExtensions, SearchOption.AllDirectories);

            foreach (string referenceMaterialFile in referenceMaterialFiles)
            {
                try
                {
                    ReferenceMaterials.Add(GenerateMaterialList(referenceMaterialFile));
                }
                catch (Exception)
                {
                    //throw new Exception($"Unhandled Exception when Generating Material List for \"{referenceMaterialFile}\" {ex}");
                }
            }
        }

        internal ReferenceMaterial GenerateMaterialList(string filePath)
        {
            return GenerateMaterialList(Resource.Load(filePath), filePath);
        }

        internal ReferenceMaterial GenerateMaterialList(Resource resource, string filePath)
        {
            ReferenceMaterial materialInfo = new();

            //Add List of materials and filename to materialInfo struct
            if (resource.ResourceType == ResourceType.ModelPack)
            {
                var model = (ModelPack)resource;
                materialInfo.materials = (List<Material>)model.Materials.Materials;
                materialInfo.fileName = Path.GetRelativePath(ReferenceMaterialPath, filePath);

                return materialInfo;
            }
            else if (resource.ResourceType == ResourceType.MaterialDictionary)
            {
                var matDict = (MaterialDictionary)resource;
                materialInfo.materials = (List<Material>)matDict.Materials;
                materialInfo.fileName = Path.GetRelativePath(ReferenceMaterialPath, filePath);

                return materialInfo;
            }
            else if (resource.ResourceType == ResourceType.Material)
            {
                var mat = (Material)resource;

                materialInfo.materials = new List<Material>
                {
                    mat
                };

                materialInfo.fileName = Path.GetRelativePath(ReferenceMaterialPath, filePath);

                return materialInfo;
            }
            else
            {
                materialInfo.materials = new List<Material>();
                return materialInfo;
            }
        }

        public struct ReferenceMaterial
        {
            internal List<Material> materials;
            internal string fileName;
        }
    }
}

using GFDLibrary;
using GFDLibrary.Materials;
using static GFDLibrary.Api.FlatApi;
using static P5MatValidator.Utils;

namespace P5MatValidator
{
    public class MaterialResources
    {
        internal Resource InputResource { get; private set; } = new ModelPack();
        internal string InputFilePath { get; private set; } = string.Empty;
        internal string ReferenceMaterialPath { get; private set; } = string.Empty;
        internal List<Material> InputMaterials { get; private set; } = new();
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

            InputResource = Resource.Load(inputFilePath);
            Task<ReferenceMaterial> getInputFileMaterials = new(() => GenerateMaterialList(InputResource, inputFilePath));
            getInputFileMaterials.Start();

            GetReferenceMaterials();

            InputMaterials = getInputFileMaterials.Result.materials;
        }

        private void GetReferenceMaterials()
        {
            string[] fileExtensions = { "*.gmtd", "*.gmt", "*.GFS", "*.GMD" };
            List<string> referenceMaterialFiles = GetFiles(ReferenceMaterialPath, fileExtensions, SearchOption.AllDirectories);

            foreach (var referenceMaterialFile in referenceMaterialFiles)
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

        internal ReferenceMaterial GenerateMaterialList(Resource gfdResource, string filePath)
        {
            ReferenceMaterial materialInfo = new();

            //Add List of materials and filename to materialInfo struct
            if (gfdResource.ResourceType == ResourceType.ModelPack)
            {
                var model = (ModelPack)gfdResource;
                materialInfo.materials = (List<Material>)model.Materials.Materials;
                materialInfo.fileName = Path.GetRelativePath(ReferenceMaterialPath, filePath);

                return materialInfo;
            }
            else if (gfdResource.ResourceType == ResourceType.MaterialDictionary)
            {
                var matDict = (MaterialDictionary)gfdResource;
                materialInfo.materials = (List<Material>)matDict.Materials;
                materialInfo.fileName = Path.GetRelativePath(ReferenceMaterialPath, filePath);

                return materialInfo;
            }
            else if (gfdResource.ResourceType == ResourceType.Material)
            {
                Material mat = (Material)gfdResource;

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

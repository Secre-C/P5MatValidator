﻿using GFDLibrary;
using GFDLibrary.Materials;
using static GFDLibrary.Api.FlatApi;
using static P5MatValidator.Utils;

namespace P5MatValidator
{
    public class MaterialResources
    {
        internal Resource? inputResource { get; private set; }
        internal string? inputFilePath { get; private set; }
        internal string? referenceMaterialPath { get; private set; }
        internal List<Material>? inputMaterials { get; private set; } = new();
        internal List<ReferenceMaterial>? referenceMaterials { get; private set; } = new();

        public MaterialResources(string referenceMaterialPath)
        {
            this.referenceMaterialPath = referenceMaterialPath;
            GetReferenceMaterials();
        }
        public MaterialResources(string inputFilePath, string referenceMaterialPath)
        {
            this.inputFilePath = inputFilePath;
            this.referenceMaterialPath = referenceMaterialPath;

            inputResource = Resource.Load(inputFilePath);
            var getInputFileMaterials = GenerateMaterialList(inputResource, inputFilePath);

            GetReferenceMaterials();

            inputMaterials = getInputFileMaterials.Result.materials;
        }

        private void GetReferenceMaterials()
        {
            string[] fileExtensions = { "*.gmtd", "*.gmt", "*.GFS", "*.GMD" };
            List<string> referenceMaterialFiles = GetFiles(referenceMaterialPath, fileExtensions, SearchOption.AllDirectories);

            foreach (var referenceMaterialFile in referenceMaterialFiles)
            {
                try
                {
                    referenceMaterials.Add(GenerateMaterialList(referenceMaterialFile).Result);
                }
                catch (Exception ex)
                {
                    //throw new Exception($"Unhandled Exception when Generating Material List for \"{referenceMaterialFile}\" {ex}");
                }
            }
        }

        internal async Task<ReferenceMaterial> GenerateMaterialList(string filePath)
        {
            return await GenerateMaterialList(Resource.Load(filePath), filePath);
        }

        internal async Task<ReferenceMaterial> GenerateMaterialList(Resource gfdResource, string filePath)
        {
            ReferenceMaterial materialInfo = new();

            //Add List of materials and filename to materialInfo struct
            if (gfdResource.ResourceType == ResourceType.ModelPack)
            {
                var model = (ModelPack)gfdResource;
                materialInfo.materials = (List<Material>)model.Materials.Materials;
                materialInfo.fileName = Path.GetRelativePath(referenceMaterialPath, filePath);

                return materialInfo;
            }
            else if (gfdResource.ResourceType == ResourceType.MaterialDictionary)
            {
                var matDict = (MaterialDictionary)gfdResource;
                materialInfo.materials = (List<Material>)matDict.Materials;
                materialInfo.fileName = Path.GetRelativePath(referenceMaterialPath, filePath);

                return materialInfo;
            }
            else if (gfdResource.ResourceType == ResourceType.Material)
            {
                var mat = (Material)gfdResource;

                materialInfo.materials = new List<Material>
                {
                    mat
                };

                materialInfo.fileName = Path.GetRelativePath(referenceMaterialPath, filePath);

                return materialInfo;
            }
            else
            {
                materialInfo.materials = new List<Material>();
                return materialInfo;
            }
        }

        internal struct ReferenceMaterial
        {
            internal List<Material> materials;
            internal string fileName;
        }
    }
}

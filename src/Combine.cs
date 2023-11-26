using GFDLibrary.Materials;

namespace P5MatValidator
{
    internal static class Combine
    {
        internal static MaterialDictionary CreateCombinedMat(MaterialResources materialResource, string materialVersion)
        {
            if (!uint.TryParse(materialVersion, out uint matVersion))
                throw new Exception("Couldn't parse mat version");

            Console.WriteLine("Combining Materials...");

            var combinedMatDict = new MaterialDictionary();

            foreach (var mat in materialResource.ReferenceMaterials.SelectMany(matDict => matDict.materials))
            {
                if (mat.Version == matVersion || matVersion == 0)
                    combinedMatDict.Add(mat);
            }

            return combinedMatDict;
        }
    }
}

using GFDLibrary.Materials;
using static P5MatValidator.MaterialComparer;
using static P5MatValidator.MaterialPoints;

namespace P5MatValidator
{
    public class MaterialSearcher
    {
        public List<string> Matches { get; private set; } = new();
        private readonly List<SearchParameter> SearchParameters = new();
        private struct SearchParameter
        {
            public string materialKey;
            public string materialValue;
            public byte texcoordMatchCount;
        }

        public MaterialSearcher(InputHandler inputHandler)
        {
            string[] args = inputHandler.Args;

            for (int i = 0; i < args.Length; i++)
            {
                bool parsable = false;
                byte texcoordSearch = 0;

                //Force ? prefix for material search
                if (!args[i].StartsWith('?'))
                {
                    continue;
                }

                if (args.Length < i + 2)
                    parsable = byte.TryParse(args[i + 2], out texcoordSearch);

                if (parsable)
                {
                    SearchParameters.Add(new SearchParameter
                    {
                        materialKey = args[i][1..],
                        materialValue = args[i + 1],
                        texcoordMatchCount = texcoordSearch
                    });
                    i += 2;
                }
                else
                {
                    SearchParameters.Add(new SearchParameter
                    {
                        materialKey = args[i][1..],
                        materialValue = args[i + 1],
                        texcoordMatchCount = texcoordSearch
                    });
                    i++;
                }
            }
        }
        public void SearchForMaterial(MaterialResources materialResource)
        {
            foreach (var materials in materialResource.ReferenceMaterials)
            {
                bool doesMaterialHaveMatchingMember = false;

                foreach (var material in materials.materials)
                {
                    foreach (var searchParameter in SearchParameters)
                    {
                        doesMaterialHaveMatchingMember = CompareMaterialMemberValue(searchParameter, material);

                        if (!doesMaterialHaveMatchingMember) break;
                    }

                    if (doesMaterialHaveMatchingMember)
                    {
                        try
                        {
                            Matches.Add($"{materials.fileName} -> {material.Name}");
                        }
                        catch (NullReferenceException e)
                        {
                            Console.WriteLine($"Skipping Material: {e}");
                        }
                    }
                }
            }
        }

        public static bool TryFindReplacementMat(Material inputMaterial, MaterialResources? materialResource, out List<MaterialComparer>? outputMaterials, bool useStrictCompare, int maximumPoints, uint texcoordAccuracy)
        {
            if (materialResource == null)
                throw new NullReferenceException(nameof(materialResource));

            outputMaterials = new();
            var materialPoints = GetMaterialPoints();

            foreach (var referenceDict in materialResource.ReferenceMaterials)
            {
                foreach (var reference in referenceDict.materials)
                {
                    int compareResult = CompareMaterial(reference, inputMaterial, materialPoints, useStrictCompare, texcoordAccuracy);

                    if (compareResult != -1 && compareResult <= maximumPoints)
                        outputMaterials.Add(new MaterialComparer(reference, compareResult, referenceDict.fileName));
                }
            }

            if (outputMaterials.Count == 0)
            {
                return false;
            }

            outputMaterials.Sort();

            return true;
        }

        public static void PrintFindReplacementResults(List<MaterialComparer>? matches)
        {
            if (matches == null)
                throw new NullReferenceException();

            Console.WriteLine($"\nFound {matches.Count} matches:\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var match in matches)
            {
                Console.WriteLine($"{match.points:D2} points || {match.material.Name} -> {match.materialFilename}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        public void PrintSearchResults()
        {
            if (Matches.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nMatches Found ({Matches.Count})");
                Console.WriteLine("=====================================");
                foreach (string match in Matches)
                {
                    Console.WriteLine(match);
                }
                Console.WriteLine("=====================================");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nNo Results Found.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static bool CompareMaterialMemberValue(SearchParameter searchParameter, Material material)
        {
            //Material Flags
            if (searchParameter.materialKey.ToLower() == "name")
                return material.Name.ToLower() == searchParameter.materialValue.ToLower();
            if (searchParameter.materialKey.ToLower() == "bit0")
                return material.Flags.HasFlag(MaterialFlags.Bit0) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit1")
                return material.Flags.HasFlag(MaterialFlags.Bit1) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit2")
                return material.Flags.HasFlag(MaterialFlags.Bit2) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit3")
                return material.Flags.HasFlag(MaterialFlags.Bit3) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "enablevertcolors" || searchParameter.materialKey.ToLower() == "bit4")
                return material.Flags.HasFlag(MaterialFlags.EnableVertColors) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "opaquealpha1" || searchParameter.materialKey.ToLower() == "bit5")
                return material.Flags.HasFlag(MaterialFlags.OpaqueAlpha1) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit6")
                return material.Flags.HasFlag(MaterialFlags.Bit6) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "enablelight" || searchParameter.materialKey.ToLower() == "bit7")
                return material.Flags.HasFlag(MaterialFlags.EnableLight) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit8")
                return material.Flags.HasFlag(MaterialFlags.Bit8) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit9")
                return material.Flags.HasFlag(MaterialFlags.Bit9) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit10")
                return material.Flags.HasFlag(MaterialFlags.Bit10) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "enablelight2" || searchParameter.materialKey.ToLower() == "bit11")
                return material.Flags.HasFlag(MaterialFlags.EnableLight2) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "purplewireframe" || searchParameter.materialKey.ToLower() == "bit12")
                return material.Flags.HasFlag(MaterialFlags.PurpleWireframe) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "opaquealpha2" || searchParameter.materialKey.ToLower() == "bit13")
                return material.Flags.HasFlag(MaterialFlags.OpaqueAlpha2) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "receiveshadow" || searchParameter.materialKey.ToLower() == "bit14")
                return material.Flags.HasFlag(MaterialFlags.ReceiveShadow) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "castshadow" || searchParameter.materialKey.ToLower() == "bit15")
                return material.Flags.HasFlag(MaterialFlags.CastShadow) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasattributes" || searchParameter.materialKey.ToLower() == "bit16")
                return HasAttributeOfType(material, int.Parse(searchParameter.materialValue), out _);
            if (searchParameter.materialKey.ToLower() == "hasoutline" || searchParameter.materialKey.ToLower() == "bit17")
                return material.Flags.HasFlag(MaterialFlags.HasOutline) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit18")
                return material.Flags.HasFlag(MaterialFlags.Bit18) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "disablebloom" || searchParameter.materialKey.ToLower() == "bit19")
                return material.Flags.HasFlag(MaterialFlags.DisableBloom) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasdiffusemap" || searchParameter.materialKey.ToLower() == "bit20")
                return material.Flags.HasFlag(MaterialFlags.HasDiffuseMap) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasnormalmap" || searchParameter.materialKey.ToLower() == "bit21")
                return material.Flags.HasFlag(MaterialFlags.HasNormalMap) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasspecularmap" || searchParameter.materialKey.ToLower() == "bit22")
                return material.Flags.HasFlag(MaterialFlags.HasSpecularMap) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasreflectionmap" || searchParameter.materialKey.ToLower() == "bit23")
                return material.Flags.HasFlag(MaterialFlags.HasReflectionMap) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hashighlightmap" || searchParameter.materialKey.ToLower() == "bit24")
                return material.Flags.HasFlag(MaterialFlags.HasHighlightMap) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasglowmap" || searchParameter.materialKey.ToLower() == "bit25")
                return material.Flags.HasFlag(MaterialFlags.HasGlowMap) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasnightmap" || searchParameter.materialKey.ToLower() == "bit26")
                return material.Flags.HasFlag(MaterialFlags.HasNightMap) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasdetailmap" || searchParameter.materialKey.ToLower() == "bit27")
                return material.Flags.HasFlag(MaterialFlags.HasDetailMap) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasshadowmap" || searchParameter.materialKey.ToLower() == "bit28")
                return material.Flags.HasFlag(MaterialFlags.HasShadowMap) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit29")
                return material.Flags.HasFlag(MaterialFlags.Bit29) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit30")
                return material.Flags.HasFlag(MaterialFlags.Bit30) == (int.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit31")
                return material.Flags.HasFlag(MaterialFlags.Bit31) == (int.Parse(searchParameter.materialValue) != 0);

            //other values
            if (searchParameter.materialKey.ToLower() == "reflectivity" || searchParameter.materialKey.ToLower() == "field40")
                return material.Field40 == float.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "outlineindex" || searchParameter.materialKey.ToLower() == "field44")
                return material.Field44 == float.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "drawmethod" || searchParameter.materialKey.ToLower() == "field48")
                return material.DrawMethod == (MaterialDrawMethod)uint.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field49")
                return material.Field49 == uint .Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field4a")
                return material.Field4A == uint.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field4b")
                return material.Field4B == uint.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field4c")
                return material.Field4C == uint.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "hightlightmapblendmode" || searchParameter.materialKey.ToLower() == "field4d")
                return material.Field4D == uint             .Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field90")
                return material.Field90 ==  uint.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field94")
                return material.Field94 == ParseTexcoord(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field96")
                return material.Field96 == uint.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field5c")
                return material.Field5C == uint.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "texcoord1" || searchParameter.materialKey.ToLower() == "field6c")
            {
                if (searchParameter.texcoordMatchCount != 0)
                {
                    return FindReducedTexcoord(material.Field6C, ParseTexcoord(searchParameter.materialValue), searchParameter.texcoordMatchCount) != -1;
                }
                else
                {
                    return material.Field6C == ParseTexcoord(searchParameter.materialValue);
                }
            }
            if (searchParameter.materialKey.ToLower() == "texcoord2" || searchParameter.materialKey.ToLower() == "field70")
            {
                if (searchParameter.texcoordMatchCount != 0)
                {
                    return FindReducedTexcoord(material.Field70, ParseTexcoord(searchParameter.materialValue), searchParameter.texcoordMatchCount) != -1;
                }
                else
                {
                    return material.Field70 == ParseTexcoord(searchParameter.materialValue);
                }
            }
            if (searchParameter.materialKey.ToLower() == "disablebackfaceculling")
                return material.DisableBackfaceCulling == uint.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field98")
                return material.Field98 == uint.Parse(searchParameter.materialValue);

            Console.WriteLine($"Material member \"{searchParameter.materialKey}\" is not a valid member");
            return false;
        }

        static uint ParseTexcoord(string hexString)
        {
            if (hexString.StartsWith("0x"))
            {
                hexString = hexString[2..];
                return uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                return uint.Parse(hexString);
            }
        }
    }

    class Texcoord
    {
        internal uint Raw { get; private set; }
        internal int Diffuse { get; private set; }
        internal int Normal { get; private set; }
        internal int Specular { get; private set; }
        internal int Reflection { get; private set; }
        internal int Highlight { get; private set; }
        internal int Glow { get; private set; }
        internal int Night { get; private set; }
        internal int Detail { get; private set; }
        internal int Shadow { get; private set; }

        internal Texcoord(uint value)
        {
            Raw = value;
            Diffuse = (byte)(value & 0x7);
            Normal = (byte)((value >> 3) & 0x7);
            Specular = (byte)((value >> 6) & 0x7);
            Reflection = (byte)((value >> 9) & 0x7);
            Highlight = (byte)((value >> 12) & 0x7);
            Glow = (byte)((value >> 15) & 0x7);
            Night = (byte)((value >> 18) & 0x7);
            Detail = (byte)((value >> 21) & 0x7);
            Shadow = (byte)((value >> 24) & 0x7);
        }

        internal int TestTexcoord(Texcoord inputTexcoord, uint accuracy)
        {
            int differingCoordCount = 0;

            static int texCompare(int referenceCoord, int inputCoord)
            {
                if (referenceCoord == inputCoord)
                    return 0;
                else if (referenceCoord == 7)
                    return 1;
                else
                    return -1;
            }

            int cDiffuse = texCompare(this.Diffuse, inputTexcoord.Diffuse);
            if (cDiffuse == -1)
                return -1;
            differingCoordCount += cDiffuse;

            int cNormal = texCompare(this.Normal, inputTexcoord.Normal);
            if (cNormal == -1)
                return -1;
            differingCoordCount += cNormal;

            int cSpecular = texCompare(this.Specular, inputTexcoord.Specular);
            if (cSpecular == -1)
                return -1;
            differingCoordCount += cSpecular;

            int cReflection = texCompare(this.Reflection, inputTexcoord.Reflection);
            if (cReflection == -1)
                return -1;
            differingCoordCount += cReflection;

            int cHighlight = texCompare(this.Highlight, inputTexcoord.Highlight);
            if (cHighlight == -1)
                return -1;
            differingCoordCount += cHighlight;

            int cGlow = texCompare(this.Glow, inputTexcoord.Glow);
            if (cGlow == -1)
                return -1;
            differingCoordCount += cGlow;

            int cNight = texCompare(this.Night, inputTexcoord.Night);
            if (cNight == -1)
                return -1;
            differingCoordCount += cNight;

            int cDetail = texCompare(this.Detail, inputTexcoord.Detail);
            if (cDetail == -1)
                return -1;
            differingCoordCount += cDetail;

            int cShadow = texCompare(this.Shadow, inputTexcoord.Shadow);
            if (cShadow == -1)
                return -1;
            differingCoordCount += cShadow;

            return differingCoordCount <= accuracy ? differingCoordCount : -1;
        }
        internal int GetEnabledTexcoordCount()
        {
            int count = 0;

            count += (Diffuse != 7) ? 1 : 0;
            count += (Normal != 7) ? 1 : 0;
            count += (Specular != 7) ? 1 : 0;
            count += (Reflection != 7) ? 1 : 0;
            count += (Highlight != 7) ? 1 : 0;
            count += (Glow != 7) ? 1 : 0;
            count += (Night != 7) ? 1 : 0;
            count += (Detail != 7) ? 1 : 0;
            count += (Shadow != 7) ? 1 : 0;

            return count;
        }

        internal int TestTexcoord(uint value, uint accuracy)
        {
            Texcoord inputTexcoord = new(value);
            return TestTexcoord(inputTexcoord, accuracy);
        }
    }
}

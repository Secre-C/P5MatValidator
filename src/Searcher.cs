using GFDLibrary.Materials;
using static P5MatValidator.Utils;
using static P5MatValidator.MaterialResources;
using static P5MatValidator.MaterialComparer;
using System.Text.RegularExpressions;

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

                try
                {
                    parsable = Byte.TryParse(args[i + 2], out texcoordSearch);
                }
                catch (IndexOutOfRangeException) { }

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
            foreach (ReferenceMaterial materials in materialResource.ReferenceMaterials)
            {
                bool doesMaterialHaveMatchingMember = false;

                foreach (Material material in materials.materials)
                {
                    foreach (SearchParameter searchParameter in SearchParameters)
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

            foreach (ReferenceMaterial referenceDict in materialResource.ReferenceMaterials)
            {
                foreach (Material reference in referenceDict.materials)
                {
                    int compareResult = CompareMaterial(reference, inputMaterial, useStrictCompare, texcoordAccuracy);

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
            foreach (MaterialComparer match in matches)
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
                return material.Flags.HasFlag(MaterialFlags.Bit0) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit1")
                return material.Flags.HasFlag(MaterialFlags.Bit1) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit2")
                return material.Flags.HasFlag(MaterialFlags.Bit2) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit3")
                return material.Flags.HasFlag(MaterialFlags.Bit3) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "enablevertcolors" || searchParameter.materialKey.ToLower() == "bit4")
                return material.Flags.HasFlag(MaterialFlags.EnableVertColors) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "opaquealpha1" || searchParameter.materialKey.ToLower() == "bit5")
                return material.Flags.HasFlag(MaterialFlags.OpaqueAlpha1) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit6")
                return material.Flags.HasFlag(MaterialFlags.Bit6) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "enablelight" || searchParameter.materialKey.ToLower() == "bit7")
                return material.Flags.HasFlag(MaterialFlags.EnableLight) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit8")
                return material.Flags.HasFlag(MaterialFlags.Bit8) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit9")
                return material.Flags.HasFlag(MaterialFlags.Bit9) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit10")
                return material.Flags.HasFlag(MaterialFlags.Bit10) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "enablelight2" || searchParameter.materialKey.ToLower() == "bit11")
                return material.Flags.HasFlag(MaterialFlags.EnableLight2) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "purplewireframe" || searchParameter.materialKey.ToLower() == "bit12")
                return material.Flags.HasFlag(MaterialFlags.PurpleWireframe) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "opaquealpha2" || searchParameter.materialKey.ToLower() == "bit13")
                return material.Flags.HasFlag(MaterialFlags.OpaqueAlpha2) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "receiveshadow" || searchParameter.materialKey.ToLower() == "bit14")
                return material.Flags.HasFlag(MaterialFlags.ReceiveShadow) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "castshadow" || searchParameter.materialKey.ToLower() == "bit15")
                return material.Flags.HasFlag(MaterialFlags.CastShadow) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasattributes" || searchParameter.materialKey.ToLower() == "bit16")
                return CheckAttributeTypes(material, Int32.Parse(searchParameter.materialValue));
            if (searchParameter.materialKey.ToLower() == "hasoutline" || searchParameter.materialKey.ToLower() == "bit17")
                return material.Flags.HasFlag(MaterialFlags.HasOutline) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit18")
                return material.Flags.HasFlag(MaterialFlags.Bit18) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "disablebloom" || searchParameter.materialKey.ToLower() == "bit19")
                return material.Flags.HasFlag(MaterialFlags.DisableBloom) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasdiffusemap" || searchParameter.materialKey.ToLower() == "bit20")
                return material.Flags.HasFlag(MaterialFlags.HasDiffuseMap) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasnormalmap" || searchParameter.materialKey.ToLower() == "bit21")
                return material.Flags.HasFlag(MaterialFlags.HasNormalMap) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasspecularmap" || searchParameter.materialKey.ToLower() == "bit22")
                return material.Flags.HasFlag(MaterialFlags.HasSpecularMap) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasreflectionmap" || searchParameter.materialKey.ToLower() == "bit23")
                return material.Flags.HasFlag(MaterialFlags.HasReflectionMap) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hashighlightmap" || searchParameter.materialKey.ToLower() == "bit24")
                return material.Flags.HasFlag(MaterialFlags.HasHighlightMap) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasglowmap" || searchParameter.materialKey.ToLower() == "bit25")
                return material.Flags.HasFlag(MaterialFlags.HasGlowMap) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasnightmap" || searchParameter.materialKey.ToLower() == "bit26")
                return material.Flags.HasFlag(MaterialFlags.HasNightMap) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasdetailmap" || searchParameter.materialKey.ToLower() == "bit27")
                return material.Flags.HasFlag(MaterialFlags.HasDetailMap) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "hasshadowmap" || searchParameter.materialKey.ToLower() == "bit28")
                return material.Flags.HasFlag(MaterialFlags.HasShadowMap) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit29")
                return material.Flags.HasFlag(MaterialFlags.Bit29) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit30")
                return material.Flags.HasFlag(MaterialFlags.Bit30) == (Int32.Parse(searchParameter.materialValue) != 0);
            if (searchParameter.materialKey.ToLower() == "bit31")
                return material.Flags.HasFlag(MaterialFlags.Bit31) == (Int32.Parse(searchParameter.materialValue) != 0);

            //other values
            if (searchParameter.materialKey.ToLower() == "reflectivity" || searchParameter.materialKey.ToLower() == "field40")
                return material.Field40 == float.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "outlineindex" || searchParameter.materialKey.ToLower() == "field44")
                return material.Field44 == float.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "drawmethod" || searchParameter.materialKey.ToLower() == "field48")
                return material.DrawMethod == (MaterialDrawMethod)UInt32.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field49")
                return material.Field49 == UInt32.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field4a")
                return material.Field4A == UInt32.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field4b")
                return material.Field4B == UInt32.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field4c")
                return material.Field4C == UInt32.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "hightlightmapblendmode" || searchParameter.materialKey.ToLower() == "field4d")
                return material.Field4D == UInt32.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field90")
                return material.Field90 == UInt32.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field94")
                return material.Field94 == ParseTexcoord(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field96")
                return material.Field96 == UInt32.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field5c")
                return material.Field5C == UInt32.Parse(searchParameter.materialValue);
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
                return material.DisableBackfaceCulling == UInt32.Parse(searchParameter.materialValue);
            if (searchParameter.materialKey.ToLower() == "field98")
                return material.Field98 == UInt32.Parse(searchParameter.materialValue);

            Console.WriteLine($"Material member \"{searchParameter.materialKey}\" is not a valid member");
            return false;
        }

        static UInt32 ParseTexcoord(string hexString)
        {
            if (hexString.StartsWith("0x"))
            {
                hexString = hexString[2..];
                return UInt32.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                return UInt32.Parse(hexString);
            }
        }
    }

    class Texcoord
    {
        internal uint Raw { get; }
        private int Diffuse { get; }
        private int Normal { get; }
        private int Specular { get; }
        private int Reflection { get; }
        private int Highlight { get; }
        private int Glow { get; }
        private int Night { get; }
        private int Detail { get; }
        private int Shadow { get; }

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
            int matchingCoords = 0;

            static int texCompare(int op1, int op2)
            {
                if (op1 == op2 && op2 != 7)
                    return 1;
                else if (op1 == 7)
                    return 0;
                else
                    return 2;
            }

            var cDiffuse = texCompare(this.Diffuse, inputTexcoord.Diffuse);
            if (cDiffuse == 2)
                return -1;
            matchingCoords += cDiffuse;

            var cNormal = texCompare(this.Normal, inputTexcoord.Normal);
            if (cNormal == 2)
                return -1;
            matchingCoords += cNormal;

            var cSpecular = texCompare(this.Specular, inputTexcoord.Specular);
            if (cSpecular == 2)
                return -1;
            matchingCoords += cSpecular;

            var cReflection = texCompare(this.Reflection, inputTexcoord.Reflection);
            if (cReflection == 2)
                return -1;
            matchingCoords += cReflection;

            var cHighlight = texCompare(this.Highlight, inputTexcoord.Highlight);
            if (cHighlight == 2)
                return -1;
            matchingCoords += cHighlight;

            var cGlow = texCompare(this.Glow, inputTexcoord.Glow);
            if (cGlow == 2)
                return -1;
            matchingCoords += cGlow;

            var cNight = texCompare(this.Night, inputTexcoord.Night);
            if (cNight == 2)
                return -1;
            matchingCoords += cNight;

            var cDetail = texCompare(this.Detail, inputTexcoord.Detail);
            if (cDetail == 2)
                return -1;
            matchingCoords += cDetail;

            var cShadow = texCompare(this.Shadow, inputTexcoord.Shadow);
            if (cShadow == 2)
                return -1;
            matchingCoords += cShadow;

            int texDiff = inputTexcoord.GetEnabledTexcoordCount() - matchingCoords;
            return texDiff <= accuracy ? texDiff : -1;
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

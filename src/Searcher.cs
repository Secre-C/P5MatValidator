using GFDLibrary.Materials;
using static P5MatValidator.Utils;
using static P5MatValidator.MaterialResources;

namespace P5MatValidator
{
    public class MaterialSearcher
    {
        public List<string> matches { get; private set; } = new();
        private List<SearchParameter> searchParameters = new();
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
                    searchParameters.Add(new SearchParameter
                    {
                        materialKey = args[i][1..],
                        materialValue = args[i + 1],
                        texcoordMatchCount = texcoordSearch
                    });
                    i += 2;
                }
                else
                {
                    searchParameters.Add(new SearchParameter
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
            foreach (ReferenceMaterial materials in materialResource.referenceMaterials)
            {
                bool doesMaterialHaveMatchingMember = false;

                foreach (Material material in materials.materials)
                {
                    foreach (SearchParameter searchParameter in searchParameters)
                    {
                        doesMaterialHaveMatchingMember = CompareMaterialMemberValue(searchParameter, material);

                        if (!doesMaterialHaveMatchingMember) break;
                    }

                    if (doesMaterialHaveMatchingMember)
                    {
                        try
                        {
                            matches.Add($"{materials.fileName} -> {material.Name}");
                        }
                        catch (NullReferenceException e)
                        {
                            Console.WriteLine($"Skipping Material: {e}");
                        }
                    }
                }
            }
        }

        public void PrintSearchResults()
        {
            if (matches.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nMatches Found ({matches.Count})");
                Console.WriteLine("=====================================");
                foreach (string match in matches)
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
                return CheckAttribute(material, Int32.Parse(searchParameter.materialValue));
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
                    return FindReducedTexcoord(material.Field6C, ParseTexcoord(searchParameter.materialValue), searchParameter.texcoordMatchCount);
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
                    return FindReducedTexcoord(material.Field70, ParseTexcoord(searchParameter.materialValue), searchParameter.texcoordMatchCount);
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

        static bool FindReducedTexcoord(uint compareValue, uint inputValue, uint keepMinimum)
        {
            if (compareValue == inputValue)
                return true;

            var compareTexcoord = new Texcoord(compareValue);

            if (compareTexcoord.TestTexcoord(inputValue, keepMinimum))
                return true;

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

        static bool CheckAttribute(Material material, Int32 type)
        {
            if (!material.Flags.HasFlag(MaterialFlags.HasAttributes))
            {
                if (type == -1)
                    return true;

                return false;
            }


            foreach (var attr in material.Attributes)
            {
                if (type == 0 && attr.AttributeType == MaterialAttributeType.Type0)
                    return true;
                if (type == 1 && attr.AttributeType == MaterialAttributeType.Type1)
                    return true;
                if (type == 2 && attr.AttributeType == MaterialAttributeType.Type2)
                    return true;
                if (type == 3 && attr.AttributeType == MaterialAttributeType.Type3)
                    return true;
                if (type == 4 && attr.AttributeType == MaterialAttributeType.Type4)
                    return true;
                if (type == 5 && attr.AttributeType == MaterialAttributeType.Type5)
                    return true;
                if (type == 6 && attr.AttributeType == MaterialAttributeType.Type6)
                    return true;
                if (type == 7 && attr.AttributeType == MaterialAttributeType.Type7)
                    return true;
                if (type == 8 && attr.AttributeType == MaterialAttributeType.Type8)
                    return true;
            }

            return false;

        }
    }

    class Texcoord
    {
        internal uint Raw { get; }
        private byte Diffuse { get; }
        private byte Normal { get; }
        private byte Specular { get; }
        private byte Reflection { get; }
        private byte Highlight { get; }
        private byte Glow { get; }
        private byte Night { get; }
        private byte Detail { get; }
        private byte Shadow { get; }

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

        internal bool TestTexcoord(Texcoord inputTexcoord, uint accuracy)
        {
            int matchingCoords = 0;

            static int texCompare(byte op1, byte op2)
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
                return false;
            matchingCoords += cDiffuse;

            var cNormal = texCompare(this.Normal, inputTexcoord.Normal);
            if (cNormal == 2)
                return false;
            matchingCoords += cNormal;

            var cSpecular = texCompare(this.Specular, inputTexcoord.Specular);
            if (cSpecular == 2)
                return false;
            matchingCoords += cSpecular;

            var cReflection = texCompare(this.Reflection, inputTexcoord.Reflection);
            if (cReflection == 2)
                return false;
            matchingCoords += cReflection;

            var cHighlight = texCompare(this.Highlight, inputTexcoord.Highlight);
            if (cHighlight == 2)
                return false;
            matchingCoords += cHighlight;

            var cGlow = texCompare(this.Glow, inputTexcoord.Glow);
            if (cGlow == 2)
                return false;
            matchingCoords += cGlow;

            var cNight = texCompare(this.Night, inputTexcoord.Night);
            if (cNight == 2)
                return false;
            matchingCoords += cNight;

            var cDetail = texCompare(this.Detail, inputTexcoord.Detail);
            if (cDetail == 2)
                return false;
            matchingCoords += cDetail;

            var cShadow = texCompare(this.Shadow, inputTexcoord.Shadow);
            if (cShadow == 2)
                return false;
            matchingCoords += cShadow;

            return matchingCoords >= accuracy;
        }

        internal bool TestTexcoord(uint value, uint accuracy)
        {
            Texcoord inputTexcoord = new(value);

            return TestTexcoord(inputTexcoord, accuracy);
        }
    }
}

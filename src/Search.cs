using GFDLibrary.Materials;
using static P5MatValidator.Program;
using static P5MatValidator.Utils;

namespace P5MatValidator
{
    public static class Search
    {
        public async static Task<List<string>> SearchForMaterial(string[] args)
        {
            string dumpPath = args[0];
            List<string> matches = new();

            string[] fileExtenstions = { "*.gmtd", "*.gmt", "*.GFS", "*.GMD" };
            List<string> matFileNames = GetFiles($"{dumpPath}", fileExtenstions, SearchOption.AllDirectories);

            List<MaterialInfo> referenceMaterials = new();

            foreach (string matFile in matFileNames)
            {
                try
                {
                    referenceMaterials.Add(await GenerateMaterialList(matFile, dumpPath));
                }
                catch
                {
                    FailedMaterialFiles.Add(Path.GetRelativePath(dumpPath, matFile));
                }
            }

            foreach (MaterialInfo materials in referenceMaterials)
            {
                bool doesMaterialHaveMatchingMember = false;

                foreach (Material material in materials.materials)
                {
                    for (int i = 2; i < args.Length; i += 2)
                    {
                        bool parsable = false;
                        byte texcoordSearch = 0;

                        try
                        {
                            parsable = Byte.TryParse(args[i + 2], out texcoordSearch);
                        }
                        catch (IndexOutOfRangeException)
                        {

                        }

                        if (parsable)
                        {
                            doesMaterialHaveMatchingMember = CompareMaterialMemberValue(args[i], args[i + 1], material, texcoordSearch);
                            i++;
                        }
                        else
                        {
                            doesMaterialHaveMatchingMember = CompareMaterialMemberValue(args[i], args[i + 1], material);
                        }

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

            return matches;
        }

        public static void PrintSearchResults(List<string> matches)
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

        static bool CompareMaterialMemberValue(string materialMember, string value, Material material)
        {
            return CompareMaterialMemberValue(materialMember, value, material, 0);
        }

        static bool CompareMaterialMemberValue(string materialMember, string value, Material material, byte texcoordSearchNum)
        {
            //Material Flags
            if (materialMember.ToLower() == "name")
                return material.Name.ToLower() == value.ToLower();
            if (materialMember.ToLower() == "bit0")
                return material.Flags.HasFlag(MaterialFlags.Bit0) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit1")
                return material.Flags.HasFlag(MaterialFlags.Bit1) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit2")
                return material.Flags.HasFlag(MaterialFlags.Bit2) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit3")
                return material.Flags.HasFlag(MaterialFlags.Bit3) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "enablevertcolors" || materialMember.ToLower() == "bit4")
                return material.Flags.HasFlag(MaterialFlags.EnableVertColors) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "opaquealpha1" || materialMember.ToLower() == "bit5")
                return material.Flags.HasFlag(MaterialFlags.OpaqueAlpha1) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit6")
                return material.Flags.HasFlag(MaterialFlags.Bit6) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "enablelight" || materialMember.ToLower() == "bit7")
                return material.Flags.HasFlag(MaterialFlags.EnableLight) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit8")
                return material.Flags.HasFlag(MaterialFlags.Bit8) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit9")
                return material.Flags.HasFlag(MaterialFlags.Bit9) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit10")
                return material.Flags.HasFlag(MaterialFlags.Bit10) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "enablelight2" || materialMember.ToLower() == "bit11")
                return material.Flags.HasFlag(MaterialFlags.EnableLight2) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "purplewireframe" || materialMember.ToLower() == "bit12")
                return material.Flags.HasFlag(MaterialFlags.PurpleWireframe) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "opaquealpha2" || materialMember.ToLower() == "bit13")
                return material.Flags.HasFlag(MaterialFlags.OpaqueAlpha2) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "receiveshadow" || materialMember.ToLower() == "bit14")
                return material.Flags.HasFlag(MaterialFlags.ReceiveShadow) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "castshadow" || materialMember.ToLower() == "bit15")
                return material.Flags.HasFlag(MaterialFlags.CastShadow) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hasattributes" || materialMember.ToLower() == "bit16")
                return CheckAttribute(material, Int32.Parse(value));
            if (materialMember.ToLower() == "hasoutline" || materialMember.ToLower() == "bit17")
                return material.Flags.HasFlag(MaterialFlags.HasOutline) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit18")
                return material.Flags.HasFlag(MaterialFlags.Bit18) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "disablebloom" || materialMember.ToLower() == "bit19")
                return material.Flags.HasFlag(MaterialFlags.DisableBloom) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hasdiffusemap" || materialMember.ToLower() == "bit20")
                return material.Flags.HasFlag(MaterialFlags.HasDiffuseMap) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hasnormalmap" || materialMember.ToLower() == "bit21")
                return material.Flags.HasFlag(MaterialFlags.HasNormalMap) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hasspecularmap" || materialMember.ToLower() == "bit22")
                return material.Flags.HasFlag(MaterialFlags.HasSpecularMap) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hasreflectionmap" || materialMember.ToLower() == "bit23")
                return material.Flags.HasFlag(MaterialFlags.HasReflectionMap) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hashighlightmap" || materialMember.ToLower() == "bit24")
                return material.Flags.HasFlag(MaterialFlags.HasHighlightMap) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hasglowmap" || materialMember.ToLower() == "bit25")
                return material.Flags.HasFlag(MaterialFlags.HasGlowMap) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hasnightmap" || materialMember.ToLower() == "bit26")
                return material.Flags.HasFlag(MaterialFlags.HasNightMap) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hasdetailmap" || materialMember.ToLower() == "bit27")
                return material.Flags.HasFlag(MaterialFlags.HasDetailMap) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "hasshadowmap" || materialMember.ToLower() == "bit28")
                return material.Flags.HasFlag(MaterialFlags.HasShadowMap) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit29")
                return material.Flags.HasFlag(MaterialFlags.Bit29) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit30")
                return material.Flags.HasFlag(MaterialFlags.Bit30) == (Int32.Parse(value) != 0);
            if (materialMember.ToLower() == "bit31")
                return material.Flags.HasFlag(MaterialFlags.Bit31) == (Int32.Parse(value) != 0);

            //other values
            if (materialMember.ToLower() == "reflectivity" || materialMember.ToLower() == "field40")
                return material.Field40 == float.Parse(value);
            if (materialMember.ToLower() == "outlineindex" || materialMember.ToLower() == "field44")
                return material.Field44 == float.Parse(value);
            if (materialMember.ToLower() == "drawmethod" || materialMember.ToLower() == "field48")
                return material.DrawMethod == (MaterialDrawMethod)UInt32.Parse(value);
            if (materialMember.ToLower() == "field49")
                return material.Field49 == UInt32.Parse(value);
            if (materialMember.ToLower() == "field4a")
                return material.Field4A == UInt32.Parse(value);
            if (materialMember.ToLower() == "field4b")
                return material.Field4B == UInt32.Parse(value);
            if (materialMember.ToLower() == "field4c")
                return material.Field4C == UInt32.Parse(value);
            if (materialMember.ToLower() == "hightlightmapblendmode" || materialMember.ToLower() == "field4d")
                return material.Field4D == UInt32.Parse(value);
            if (materialMember.ToLower() == "field90")
                return material.Field90 == UInt32.Parse(value);
            if (materialMember.ToLower() == "field94")
                return material.Field94 == ParseTexcoord(value);
            if (materialMember.ToLower() == "field96")
                return material.Field96 == UInt32.Parse(value);
            if (materialMember.ToLower() == "field5c")
                return material.Field5C == UInt32.Parse(value);
            if (materialMember.ToLower() == "texcoord1" || materialMember.ToLower() == "field6c")
            {
                if (texcoordSearchNum != 0)
                {
                    return FindReducedTexcoord(material.Field6C, ParseTexcoord(value), texcoordSearchNum);
                }
                else
                {
                    return material.Field6C == ParseTexcoord(value);
                }
            }
            if (materialMember.ToLower() == "texcoord2" || materialMember.ToLower() == "field70")
            {
                if (texcoordSearchNum != 0)
                {
                    return FindReducedTexcoord(material.Field70, ParseTexcoord(value), texcoordSearchNum);
                }
                else
                {
                    return material.Field70 == ParseTexcoord(value);
                }
            }
            if (materialMember.ToLower() == "disablebackfaceculling")
                return material.DisableBackfaceCulling == UInt32.Parse(value);
            if (materialMember.ToLower() == "field98")
                return material.Field98 == UInt32.Parse(value);

            Console.WriteLine($"Material member \"{materialMember}\" is not a valid member");
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
                hexString = hexString.Substring(2);
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

            Func<byte, byte, int> texCompare = (byte op1, byte op2) =>
            {
                if (op1 == op2 && op2 != 7)
                    return 1;
                else if (op1 == 7)
                    return 0;
                else
                    return 2;
            };

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
            Texcoord inputTexcoord = new Texcoord(value);

            return TestTexcoord(inputTexcoord, accuracy);
        }
    }
}

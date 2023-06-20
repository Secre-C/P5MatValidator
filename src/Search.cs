using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P5MatValidator.Program;
using static P5MatValidator.Utils;
using static GFDLibrary.Api.FlatApi;
using GFDLibrary.Materials;
using GFDLibrary;
using System.Text.RegularExpressions;

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
                        doesMaterialHaveMatchingMember = CompareMaterialMemberValue(args[i], args[i + 1], material);

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
                return material.Field6C == ParseTexcoord(value);
            if (materialMember.ToLower() == "texcoord2" || materialMember.ToLower() == "field70")
                return material.Field70 == ParseTexcoord(value);
            if (materialMember.ToLower() == "disablebackfaceculling")
                return material.DisableBackfaceCulling == UInt32.Parse(value);
            if (materialMember.ToLower() == "field98")
                return material.Field98 == UInt32.Parse(value);

            Console.WriteLine($"Material member \"{materialMember}\" is not a valid member");
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
}

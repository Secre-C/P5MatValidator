using GFDLibrary.Materials;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using static P5MatValidator.Program;

namespace P5MatValidator
{
    public class MaterialComparer : IComparable<MaterialComparer>
    {
        public readonly string materialFilename;
        public readonly Material material;
        public readonly int points;
        
        public MaterialComparer(Material material, int points, string materialFilename)
        {
            this.material = material;
            this.points = points;
            this.materialFilename = materialFilename;
        }

        public int CompareTo(MaterialComparer? a) //IComparable
        {
            if (a == null) return 1;

            return points.CompareTo(a.points);
        }


        internal static int CompareMaterial(Material material, Material royalMaterial, bool useStrictCompare = false, uint texcoordAccuracy = 0)
        {
            int points = 0; //add points for each non matching mat

            if (useStrictCompare) //check these in strict mode
            {
                if (!AreColorsEqual(material.AmbientColor, royalMaterial.AmbientColor))
                    points++;
                if (!AreColorsEqual(material.DiffuseColor, royalMaterial.DiffuseColor))
                    points++;
                if (!AreColorsEqual(material.SpecularColor, royalMaterial.SpecularColor))
                    points++;
                if (!AreColorsEqual(material.EmissiveColor, royalMaterial.EmissiveColor))
                    points++;
                if (!AreEqual(material.Field40, royalMaterial.Field40)) //reflectivity
                    points++;
                if (!AreEqual(material.Field44, royalMaterial.Field44)) //outline index
                    points++;
                if (!AreEqual(material.Field4A, royalMaterial.Field4A))
                    points++;
                if (!AreEqual(material.Field4C, royalMaterial.Field4C))
                    points++;
                if (!AreEqual(material.DisableBackfaceCulling, royalMaterial.DisableBackfaceCulling))
                    points++;
                if (!AreEqual(material.Field98, royalMaterial.Field98))
                    points++;
            }

            int flagCompare = CompareMaterialFlags(material.Flags, royalMaterial.Flags);
            if (flagCompare == -1)
            {
                return -1;
            }
            else
                points += flagCompare * 2;

            if (!AreEqual((byte)material.DrawMethod, (byte)royalMaterial.DrawMethod))
                return -1;
            if (!AreEqual(material.Field49, royalMaterial.Field49))
                points++;
            if (!AreEqual(material.Field4B, royalMaterial.Field4B))
                points++;
            if (!AreEqual(material.Field4D, royalMaterial.Field4D)) //highlight blend mode
                points += 2;
            if (!AreEqual(material.Field90, royalMaterial.Field90))
                points++;
            if (!AreEqual(material.Field92, royalMaterial.Field92))
                points++;
            if (!AreEqual(material.Field94, royalMaterial.Field94))
                points += 2;
            if (!AreEqual(material.Field96, royalMaterial.Field96))
                points++;
            if (!AreEqual(material.Field5C, royalMaterial.Field5C))
                points++;
            if (!AreEqual(material.Field6C, royalMaterial.Field6C)) //texcoord1
            {
                int texcoordAccuracyResult = FindReducedTexcoord(material.Field6C, royalMaterial.Field6C, texcoordAccuracy);

                if (texcoordAccuracyResult == -1)
                    return -1;

                points += texcoordAccuracyResult * 2;
            }
            if (!AreEqual(material.Field70, royalMaterial.Field70)) //texcoord2
            {
                int texcoordAccuracyResult = FindReducedTexcoord(material.Field70, royalMaterial.Field70, texcoordAccuracy);
               
                if (texcoordAccuracyResult == -1)
                    return -1;
               
                points += texcoordAccuracyResult * 2;
            }
            if (!AreAttributesEqual(material, royalMaterial, useStrictCompare))
                return -1;

            return points;
        }
        internal static bool AreEqual(object a, object b)
        {
            return Equals(a, b);
        }
        internal static bool AreColorsEqual(Vector4 a, Vector4 b)
        {
            if (!AreEqual(a.X, b.X)) return false;
            if (!AreEqual(a.Y, b.Y)) return false;
            if (!AreEqual(a.Z, b.Z)) return false;
            if (!AreEqual(a.W, b.W)) return false;

            return true;
        }

        internal static bool AreMatFlagsEqual(MaterialFlags a, MaterialFlags b)
        {
            if (!AreEqual((uint)a, (uint)b)) return false;

            return true;
        }

        public static bool AreAttributesEqual(Material a, Material b, bool strictMode = false)
        {
            if (!a.Flags.HasFlag(MaterialFlags.HasAttributes) && !b.Flags.HasFlag(MaterialFlags.HasAttributes))
                return true;
            else if (!a.Flags.HasFlag(MaterialFlags.HasAttributes) || !b.Flags.HasFlag(MaterialFlags.HasAttributes))
                return false;

            if (a.Attributes.Count != b.Attributes.Count)
                return false;

            bool typeMatchFound = false;

            foreach (var attr in a.Attributes)
            {
                foreach (var attr2 in b.Attributes)
                {
                    if (attr.AttributeType == attr2.AttributeType)
                    {
                        typeMatchFound = true;
                        break;
                    }
                }

                if (!typeMatchFound)
                    return false;

                typeMatchFound = false;
            }

            foreach (var attr in a.Attributes)
            {
                if (attr.AttributeType == MaterialAttributeType.Type1)
                {
                    foreach (var attr2 in b.Attributes)
                    {
                        if (attr2.AttributeType == MaterialAttributeType.Type1)
                        {
                            var type1a = (MaterialAttributeType1)attr;
                            var type1b = (MaterialAttributeType1)attr2;

                            if (type1a.RawFlags != type1b.RawFlags && strictMode) return false;
                            if (((ushort)type1a.Type1Flags) != ((ushort)type1b.Type1Flags) && strictMode) return false;
                            if (type1a.Field1C != type1b.Field1C && strictMode) return false;
                            if (type1a.Field20 != type1b.Field20 && strictMode) return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool CheckAttributeTypes(Material material, Int32 type)
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

        public static int FindReducedTexcoord(uint compareValue, uint inputValue, uint accuracy)
        {
            if (compareValue == inputValue)
            {
                return 0;
            }
            else
            {
                if (accuracy == 0)
                    return -1;
            }


            var compareTexcoord = new Texcoord(compareValue);

            return compareTexcoord.TestTexcoord(inputValue, accuracy);
        }

        internal static int CompareMaterialFlags(MaterialFlags a, MaterialFlags b)
        {
            static int mats_have_flag(MaterialFlags a, MaterialFlags b, MaterialFlags flag)
            {
                return (a.HasFlag(flag) == b.HasFlag(flag)) ? 0 : 1;
            }

            if (((int)a) == (int)b)
                return 0;

            int points = 0;

            points += mats_have_flag(a, b, MaterialFlags.Bit0);
            points += mats_have_flag(a, b, MaterialFlags.Bit1);
            points += mats_have_flag(a, b, MaterialFlags.Bit2);
            points += mats_have_flag(a, b, MaterialFlags.Bit3);
            //points += mats_have_flag(a, b, MaterialFlags.EnableVertColors);

            if (mats_have_flag(a, b, MaterialFlags.EnableVertColors) != 0)
                return -1;

            points += mats_have_flag(a, b, MaterialFlags.OpaqueAlpha1);
            points += mats_have_flag(a, b, MaterialFlags.Bit6);
            //points += mats_have_flag(a, b, MaterialFlags.EnableLight);

            if (mats_have_flag(a, b, MaterialFlags.EnableLight) != 0)
                return -1;

            points += mats_have_flag(a, b, MaterialFlags.Bit8);
            points += mats_have_flag(a, b, MaterialFlags.Bit9);
            points += mats_have_flag(a, b, MaterialFlags.Bit10);
            //points += mats_have_flag(a, b, MaterialFlags.EnableLight2);

            if (mats_have_flag(a, b, MaterialFlags.EnableLight2) != 0)
                return -1;

            //points += mats_have_flag(a, b, MaterialFlags.OpaqueAlpha2);

            if (mats_have_flag(a, b, MaterialFlags.OpaqueAlpha2) != 0)
                return -1;

            points += mats_have_flag(a, b, MaterialFlags.ReceiveShadow);
            points += mats_have_flag(a, b, MaterialFlags.CastShadow);
            //points += mats_have_flag(a, b, MaterialFlags.HasAttributes);

            if (mats_have_flag(a, b, MaterialFlags.HasAttributes) != 0)
                return -1;

            //points += mats_have_flag(a, b, MaterialFlags.HasOutline);

            if (mats_have_flag(a, b, MaterialFlags.HasOutline) != 0)
                return -1;

            //points += mats_have_flag(a, b, MaterialFlags.Bit18);

            if (mats_have_flag(a, b, MaterialFlags.Bit18) != 0)
                return -1;

            points += mats_have_flag(a, b, MaterialFlags.DisableBloom);
            //points += mats_have_flag(a, b, MaterialFlags.HasDiffuseMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasNormalMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasSpecularMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasReflectionMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasHighlightMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasGlowMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasNightMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasDetailMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasShadowMap);
            points += mats_have_flag(a, b, MaterialFlags.Bit29);
            //points += mats_have_flag(a, b, MaterialFlags.Bit30);

            if (mats_have_flag(a, b, MaterialFlags.Bit30) != 0)
                return -1;

            points += mats_have_flag(a, b, MaterialFlags.Bit31);

            return points;
        }
    }
}

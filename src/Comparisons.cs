using GFDLibrary.Materials;
using System.Numerics;
using System.Text.Json;

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


        internal static int CompareMaterial(Material material, Material royalMaterial, MaterialPoints materialPoints, bool useStrictCompare = false, uint texcoordAccuracy = 0)
        {
            int points = 0; //add points for each non matching mat

            if (useStrictCompare) //check these in strict mode
            {
                if (!AreColorsEqual(material.AmbientColor, royalMaterial.AmbientColor))
                    points += materialPoints.AmbientColor;
                if (!AreColorsEqual(material.DiffuseColor, royalMaterial.DiffuseColor))
                    points += materialPoints.DiffuseColor;
                if (!AreColorsEqual(material.SpecularColor, royalMaterial.SpecularColor))
                    points += materialPoints.SpecularColor;
                if (!AreColorsEqual(material.EmissiveColor, royalMaterial.EmissiveColor))
                    points += materialPoints.EmissiveColor;
                if (!AreEqual(material.Field40, royalMaterial.Field40)) //reflectivity
                    points += materialPoints.Field40_Reflectivity;
                if (!AreEqual(material.Field44, royalMaterial.Field44)) //outline index
                    points += materialPoints.Field44_Outline_Index;
                if (!AreEqual(material.Field4A, royalMaterial.Field4A))
                    points += materialPoints.Field4A;
                if (!AreEqual(material.Field4C, royalMaterial.Field4C))
                    points += materialPoints.Field4C;
                if (!AreEqual(material.DisableBackfaceCulling, royalMaterial.DisableBackfaceCulling))
                    points += materialPoints.DisableBackfaceCulling;
                if (!AreEqual(material.Field98, royalMaterial.Field98))
                    points += materialPoints.Field98;
            }

            int flagCompare = CompareMaterialFlags(material.Flags, royalMaterial.Flags, materialPoints);
            points += flagCompare * 2;

            if (!AreEqual((byte)material.DrawMethod, (byte)royalMaterial.DrawMethod))
                points += materialPoints.DrawMethod;
            if (!AreEqual(material.Field49, royalMaterial.Field49))
                points += materialPoints.Field49;
            if (!AreEqual(material.Field4B, royalMaterial.Field4B))
                points += materialPoints.Field4B;
            if (!AreEqual(material.Field4D, royalMaterial.Field4D)) //highlight blend mode
                points += materialPoints.Field4D;
            if (!AreEqual(material.Field90, royalMaterial.Field90))
                points += materialPoints.Field90;
            if (!AreEqual(material.Field92, royalMaterial.Field92))
                points += materialPoints.Field92;
            if (!AreEqual(material.Field94, royalMaterial.Field94))
                points += materialPoints.Field94;
            if (!AreEqual(material.Field96, royalMaterial.Field96))
                points += materialPoints.Field96;
            if (!AreEqual(material.Field5C, royalMaterial.Field5C))
                points += materialPoints.Field5C;
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

        internal static int CompareMaterialFlags(MaterialFlags a, MaterialFlags b, MaterialPoints materialPoints)
        {
            static int mat_flags_match(MaterialFlags a, MaterialFlags b, MaterialFlags flag)
            {
                return (a.HasFlag(flag) == b.HasFlag(flag)) ? 0 : 1;
            }

            if (((int)a) == (int)b)
                return 0;

            int points = 0;

            points += mat_flags_match(a, b, MaterialFlags.Bit0) * materialPoints.Bit0;
            points += mat_flags_match(a, b, MaterialFlags.Bit1) * materialPoints.Bit1;
            points += mat_flags_match(a, b, MaterialFlags.Bit2) * materialPoints.Bit2;
            points += mat_flags_match(a, b, MaterialFlags.Bit3) * materialPoints.Bit3;
            points += mat_flags_match(a, b, MaterialFlags.EnableVertColors) * materialPoints.EnableVertColors;
            points += mat_flags_match(a, b, MaterialFlags.OpaqueAlpha1) * materialPoints.OpaqueAlpha1;
            points += mat_flags_match(a, b, MaterialFlags.Bit6) * materialPoints.Bit6;
            points += mat_flags_match(a, b, MaterialFlags.EnableLight) * materialPoints.EnableLight;
            points += mat_flags_match(a, b, MaterialFlags.Bit8) * materialPoints.Bit8;
            points += mat_flags_match(a, b, MaterialFlags.Bit9) * materialPoints.Bit9;
            points += mat_flags_match(a, b, MaterialFlags.Bit10) * materialPoints.Bit10;
            points += mat_flags_match(a, b, MaterialFlags.EnableLight2) * materialPoints.EnableLight2;
            points += mat_flags_match(a, b, MaterialFlags.OpaqueAlpha2) * materialPoints.OpaqueAlpha2;
            points += mat_flags_match(a, b, MaterialFlags.ReceiveShadow) * materialPoints.ReceiveShadow;
            points += mat_flags_match(a, b, MaterialFlags.CastShadow) * materialPoints.CastShadow;
            points += mat_flags_match(a, b, MaterialFlags.HasAttributes) * materialPoints.HasAttributes;
            points += mat_flags_match(a, b, MaterialFlags.HasOutline) * materialPoints.HasOutline;
            points += mat_flags_match(a, b, MaterialFlags.Bit18) * materialPoints.Bit18;
            points += mat_flags_match(a, b, MaterialFlags.DisableBloom) * materialPoints.DisableBloom;
            //points += mats_have_flag(a, b, MaterialFlags.HasDiffuseMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasNormalMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasSpecularMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasReflectionMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasHighlightMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasGlowMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasNightMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasDetailMap);
            //points += mats_have_flag(a, b, MaterialFlags.HasShadowMap);
            points += mat_flags_match(a, b, MaterialFlags.Bit29) * materialPoints.Bit29;
            points += mat_flags_match(a, b, MaterialFlags.Bit30) * materialPoints.Bit30;
            points += mat_flags_match(a, b, MaterialFlags.Bit31) * materialPoints.Bit31;

            return points;
        }
    }

    public class MaterialPoints
    {
        public int AmbientColor { get; set; } = 1;
        public int DiffuseColor { get; set; } = 1;
        public int SpecularColor { get; set; } = 1;
        public int EmissiveColor { get; set; } = 1;

        //Material Flags
        public int Bit0 { get; set; } = 1;
        public int Bit1 { get; set; } = 1;
        public int Bit2 { get; set; } = 1;
        public int Bit3 { get; set; } = 1;
        public int EnableVertColors { get; set; } = 1000;
        public int OpaqueAlpha1 { get; set; } = 1;
        public int Bit6 { get; set; } = 1;
        public int EnableLight { get; set; } = 1;
        public int Bit8 { get; set; } = 1;
        public int Bit9 { get; set; } = 1;
        public int Bit10 { get; set; } = 1;
        public int EnableLight2 { get; set; } = 1000;
        public int PurpleWireframe { get; set; } = 1000;
        public int OpaqueAlpha2 { get; set; } = 1000;
        public int ReceiveShadow { get; set; } = 1;
        public int CastShadow { get; set; } = 1;
        public int HasAttributes { get; set; } = 1000;
        public int HasOutline { get; set; } = 1000;
        public int Bit18 { get; set; } = 1000;
        public int DisableBloom { get; set; } = 1;
        public int Bit29 { get; set; } = 1;
        public int Bit30 { get; set; } = 1000;
        public int Bit31 { get; set; } = 1;

        public int DrawMethod { get; set; } = 1000;
        public int Field49 { get; set; } = 1;
        public int Field4B { get; set; } = 1;
        public int Field4D { get; set; } = 2; //highlight map blend mode
        public int Field90 { get; set; } = 0;
        public int Field92 { get; set; } = 1;
        public int Field94 { get; set; } = 2;
        public int Field96 { get; set; } = 1;
        public int Field5C { get; set; } = 1;

        //Strict Mode Values
        public int Field40_Reflectivity { get; set; } = 1;
        public int Field44_Outline_Index { get; set; } = 1;
        public int Field4A { get; set; } = 1;
        public int Field4C { get; set; } = 1;
        public int DisableBackfaceCulling { get; set; } = 0;
        public int Field98 { get; set; } = 1;

        public static MaterialPoints GetMaterialPoints()
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string? strWorkPath = Path.GetDirectoryName(strExeFilePath);
            string matPointPath = strWorkPath + "/MaterialPoints.json";

            if (File.Exists(matPointPath))
            {
                MaterialPoints? pointsjson = JsonSerializer.Deserialize<MaterialPoints>(File.ReadAllText(matPointPath));

                if (pointsjson != null)
                {
                    Utils.DebugLog("Loaded External Point Values");
                    return pointsjson;
                }
            }

            MaterialPoints points = new();
            WriteMatPointJson(points, matPointPath);
            return points;
        }

        private static void WriteMatPointJson(MaterialPoints points, string path)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(points, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}

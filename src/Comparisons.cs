using GFDLibrary.Materials;
using System.Numerics;
using static P5MatValidator.Program;

namespace P5MatValidator
{
    internal static class Comparisons
    {
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

        internal static bool AreAttributesEqual(Material a, Material b, bool strictMode = false)
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

                            if (type1a.RawFlags != type1b.RawFlags) return false;
                            if (((ushort)type1a.Type1Flags) != ((ushort)type1b.Type1Flags)) return false;
                            if (type1a.Field1C != type1b.Field1C && strictMode) return false;
                            if (type1a.Field20 != type1b.Field20 && strictMode) return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}

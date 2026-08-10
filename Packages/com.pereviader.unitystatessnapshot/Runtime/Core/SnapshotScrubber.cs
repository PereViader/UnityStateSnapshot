using System;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Utility for scrubbing, sanitizing, and rounding Unity engine types and values for snapshot determinism.
    /// </summary>
    public static class SnapshotScrubber
    {
        private const int NegativeZeroFloatBits = unchecked((int)0x80000000);
        private const long NegativeZeroDoubleBits = unchecked((long)0x8000000000000000L);

        /// <summary>
        /// Rounds a float to the specified precision and normalizes -0.0f to 0.0f.
        /// </summary>
        public static float RoundFloat(float value, int precision)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return value;

            float rounded = (float)Math.Round((double)value, precision, MidpointRounding.AwayFromZero);
            if (rounded == 0.0f)
            {
                return 0.0f;
            }

            if (BitConverter.SingleToInt32Bits(rounded) == NegativeZeroFloatBits || (rounded == 0f && 1f / rounded < 0f))
            {
                return 0.0f;
            }

            return rounded;
        }

        /// <summary>
        /// Rounds a double to the specified precision and normalizes -0.0d to 0.0d.
        /// </summary>
        public static double RoundDouble(double value, int precision)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return value;

            double rounded = Math.Round(value, precision, MidpointRounding.AwayFromZero);
            if (rounded == 0.0d)
            {
                return 0.0d;
            }

            if (BitConverter.DoubleToInt64Bits(rounded) == NegativeZeroDoubleBits || (rounded == 0d && 1d / rounded < 0d))
            {
                return 0.0d;
            }

            return rounded;
        }

        public static Vector2DTO ScrubVector2(Vector2 vec, int precision)
        {
            return new Vector2DTO(
                RoundFloat(vec.x, precision),
                RoundFloat(vec.y, precision)
            );
        }

        public static Vector3DTO ScrubVector3(Vector3 vec, int precision)
        {
            return new Vector3DTO(
                RoundFloat(vec.x, precision),
                RoundFloat(vec.y, precision),
                RoundFloat(vec.z, precision)
            );
        }

        public static Vector4DTO ScrubVector4(Vector4 vec, int precision)
        {
            return new Vector4DTO(
                RoundFloat(vec.x, precision),
                RoundFloat(vec.y, precision),
                RoundFloat(vec.z, precision),
                RoundFloat(vec.w, precision)
            );
        }

        public static QuaternionDTO ScrubQuaternion(Quaternion quat, int precision)
        {
            return new QuaternionDTO(
                RoundFloat(quat.x, precision),
                RoundFloat(quat.y, precision),
                RoundFloat(quat.z, precision),
                RoundFloat(quat.w, precision)
            );
        }

        public static string ScrubColor(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        }

        public static string ScrubColor32(Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";
        }

        public static RectDTO ScrubRect(Rect rect, int precision)
        {
            return new RectDTO(
                RoundFloat(rect.x, precision),
                RoundFloat(rect.y, precision),
                RoundFloat(rect.width, precision),
                RoundFloat(rect.height, precision)
            );
        }

        public static BoundsDTO ScrubBounds(Bounds bounds, int precision)
        {
            return new BoundsDTO(
                ScrubVector3(bounds.center, precision),
                ScrubVector3(bounds.size, precision)
            );
        }

        public static float[,] ScrubMatrix4x4(Matrix4x4 matrix, int precision)
        {
            var result = new float[4, 4];
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    result[r, c] = RoundFloat(matrix[r, c], precision);
                }
            }
            return result;
        }

        /// <summary>
        /// Scrubs a GameObject or asset name by stripping '(Clone)' and '(Instance)' suffixes.
        /// </summary>
        public static string ScrubName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            return name
                .Replace("(Clone)", "")
                .Replace("(Instance)", "")
                .Trim();
        }

        /// <summary>
        /// Scrubs a UnityEngine.Object reference into a deterministic reference string.
        /// </summary>
        public static string ScrubUnityObject(UnityEngine.Object uObj)
        {
            if (uObj == null) return null;
            return $"Ref<{uObj.GetType().Name}>({ScrubName(uObj.name)})";
        }

        /// <summary>
        /// Scrubs a value given the active VerifySettings.
        /// </summary>
        public static object ScrubValue(string memberName, object value, VerifySettings settings)
        {
            if (value == null) return null;

            // Apply custom scrubbers
            if (settings?.CustomScrubbers != null && settings.CustomScrubbers.Count > 0)
            {
                foreach (var scrubber in settings.CustomScrubbers)
                {
                    value = scrubber(memberName, value);
                    if (value == null) return null;
                }
            }

            int precision = settings?.FloatPrecision ?? 3;
            return ScrubKnownTypes(value, precision);
        }

        /// <summary>
        /// Scrubs a value given the active SnapshotOptions (backward compatibility).
        /// </summary>
        public static object ScrubValue(string memberName, object value, SnapshotOptions options)
        {
            if (value == null) return null;

            if (options?.CustomScrubbers != null && options.CustomScrubbers.Count > 0)
            {
                foreach (var scrubber in options.CustomScrubbers)
                {
                    value = scrubber(memberName, value);
                    if (value == null) return null;
                }
            }

            int precision = options?.FloatPrecision ?? 3;
            return ScrubKnownTypes(value, precision);
        }

        private static object ScrubKnownTypes(object value, int precision)
        {
            if (value is float f)
            {
                return RoundFloat(f, precision);
            }
            if (value is double d)
            {
                return RoundDouble(d, precision);
            }
            if (value is decimal dec)
            {
                return Math.Round(dec, precision, MidpointRounding.AwayFromZero);
            }
            if (value is Vector2 v2)
            {
                return ScrubVector2(v2, precision);
            }
            if (value is Vector3 v3)
            {
                return ScrubVector3(v3, precision);
            }
            if (value is Vector4 v4)
            {
                return ScrubVector4(v4, precision);
            }
            if (value is Quaternion q)
            {
                return ScrubQuaternion(q, precision);
            }
            if (value is Color color)
            {
                return ScrubColor(color);
            }
            if (value is Color32 color32)
            {
                return ScrubColor32(color32);
            }
            if (value is Rect rect)
            {
                return ScrubRect(rect, precision);
            }
            if (value is Bounds bounds)
            {
                return ScrubBounds(bounds, precision);
            }
            if (value is Matrix4x4 matrix)
            {
                return ScrubMatrix4x4(matrix, precision);
            }
            if (value is UnityEngine.Object uObj)
            {
                return ScrubUnityObject(uObj);
            }

            return value;
        }
    }
}

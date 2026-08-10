using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Configuration options for snapshot extraction.
    /// Note: For new code, prefer using VerifySettings.
    /// </summary>
    public class SnapshotOptions
    {
        /// <summary>
        /// Decimal places to round floating-point numbers to prevent floating-point non-determinism.
        /// </summary>
        public int FloatPrecision { get; set; } = 3;

        /// <summary>
        /// Whether to extract child GameObjects that are disabled.
        /// </summary>
        public bool IncludeDisabledObjects { get; set; } = true;

        /// <summary>
        /// Whether to extract Transform/RectTransform state.
        /// </summary>
        public bool IncludeTransform { get; set; } = true;

        /// <summary>
        /// Whether to extract private/protected fields marked with [SerializeField].
        /// </summary>
        public bool IncludeSerializedFields { get; set; } = true;

        /// <summary>
        /// Component types to exclude from snapshot extraction.
        /// </summary>
        public HashSet<Type> IgnoredComponentTypes { get; set; } = new HashSet<Type>
        {
            typeof(Transform),
            typeof(RectTransform),
            typeof(CanvasRenderer)
        };

        /// <summary>
        /// Member names (fields/properties) to exclude from extraction.
        /// </summary>
        public HashSet<string> IgnoredPropertyNames { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "m_CachedPtr",
            "name",
            "tag",
            "hideFlags",
            "useGUILayout",
            "enabled",
            "isActiveAndEnabled",
            "gameObject",
            "transform",
            "runInEditMode"
        };

        /// <summary>
        /// Custom scrubber callbacks: (memberName, rawValue) => scrubbedValue.
        /// </summary>
        public List<Func<string, object, object>> CustomScrubbers { get; set; } = new List<Func<string, object, object>>();

        public static SnapshotOptions Default => new SnapshotOptions();

        public VerifySettings ToVerifySettings()
        {
            return VerifySettings.FromSnapshotOptions(this);
        }

        public static implicit operator VerifySettings(SnapshotOptions options)
        {
            return VerifySettings.FromSnapshotOptions(options);
        }

        public static implicit operator SnapshotOptions(VerifySettings settings)
        {
            return settings?.ToSnapshotOptions();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Configuration settings for snapshot extraction, serialization, and verification.
    /// Provides fluent configuration methods and maintains custom scrubbers and component handlers.
    /// </summary>
    public class VerifySettings
    {
        private static readonly object GlobalLock = new object();
        private static VerifySettings _global;

        /// <summary>
        /// Global shared default settings applied to all new VerifySettings instances.
        /// </summary>
        public static VerifySettings Global
        {
            get
            {
                if (_global == null)
                {
                    lock (GlobalLock)
                    {
                        if (_global == null)
                        {
                            _global = CreateDefaultInstance();
                        }
                    }
                }
                return _global;
            }
        }

        /// <summary>
        /// Initializes or customizes global snapshot verification settings.
        /// </summary>
        public static void InitializeGlobal(Action<VerifySettings> configure = null)
        {
            lock (GlobalLock)
            {
                _global = CreateDefaultInstance();
                configure?.Invoke(_global);
            }
        }

        /// <summary>
        /// Registers a component handler globally.
        /// </summary>
        public static void RegisterGlobalHandler(ISnapshotComponentHandler handler)
        {
            Global.RegisterComponentHandler(handler);
        }

        /// <summary>
        /// Modifies global serialization settings.
        /// </summary>
        public static void ModifySerialization(Action<VerifySettings> configure)
        {
            configure?.Invoke(Global);
        }

        /// <summary>
        /// Custom snapshot output directory.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Custom snapshot file name (without extension).
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Custom type name or category name for snapshot grouping.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Decimal places to round floating-point numbers. Defaults to 3.
        /// </summary>
        public int FloatPrecision { get; set; } = 3;

        /// <summary>
        /// Whether to extract child GameObjects that are inactive. Defaults to true.
        /// </summary>
        public bool ExtractDisabledObjects { get; set; } = true;

        /// <summary>
        /// Whether to extract Transform and RectTransform hierarchy and layout data. Defaults to true.
        /// </summary>
        public bool ExtractTransforms { get; set; } = true;

        /// <summary>
        /// Whether to extract private/protected fields marked with [SerializeField]. Defaults to true.
        /// </summary>
        public bool ExtractSerializedFields { get; set; } = true;

        /// <summary>
        /// Whether to automatically accept and overwrite verified snapshot files when mismatches occur.
        /// </summary>
        public bool IsAutoVerify { get; set; } = false;

        /// <summary>
        /// Component types excluded from snapshot extraction.
        /// </summary>
        public HashSet<Type> IgnoredComponentTypes { get; set; } = new HashSet<Type>();

        /// <summary>
        /// Member names excluded globally across all components.
        /// </summary>
        public HashSet<string> IgnoredMemberNames { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Type-specific ignored member names.
        /// </summary>
        public Dictionary<Type, HashSet<string>> IgnoredTypeMembers { get; set; } = new Dictionary<Type, HashSet<string>>();

        /// <summary>
        /// Custom scrubber callbacks: (memberName, rawValue) => scrubbedValue.
        /// </summary>
        public List<Func<string, object, object>> CustomScrubbers { get; set; } = new List<Func<string, object, object>>();

        /// <summary>
        /// Registered component handlers ordered by Priority descending.
        /// </summary>
        public List<ISnapshotComponentHandler> ComponentHandlers { get; set; } = new List<ISnapshotComponentHandler>();

        public VerifySettings()
        {
            if (_global != null)
            {
                CopyFrom(_global);
            }
            else
            {
                PopulateDefaults();
            }
        }

        public VerifySettings(VerifySettings other)
        {
            if (other != null)
            {
                CopyFrom(other);
            }
            else
            {
                PopulateDefaults();
            }
        }

        private static VerifySettings CreateDefaultInstance()
        {
            var settings = new VerifySettings(null);
            settings.PopulateDefaults();
            return settings;
        }

        private void PopulateDefaults()
        {
            FloatPrecision = 3;
            ExtractDisabledObjects = true;
            ExtractTransforms = true;
            ExtractSerializedFields = true;
            IsAutoVerify = false;

            IgnoredComponentTypes = new HashSet<Type>
            {
                typeof(Transform),
                typeof(RectTransform),
                typeof(CanvasRenderer)
            };

            IgnoredMemberNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

            IgnoredTypeMembers = new Dictionary<Type, HashSet<string>>();
            CustomScrubbers = new List<Func<string, object, object>>();

            ComponentHandlers = new List<ISnapshotComponentHandler>
            {
                // Priority 60 (Specific UI Controls)
                new ButtonSnapshotHandler(),
                new ToggleSnapshotHandler(),
                new SliderSnapshotHandler(),
                new ScrollbarSnapshotHandler(),

                // Priority 50 (Rendering Handlers)
                new SpriteRendererSnapshotHandler(),
                new MeshFilterSnapshotHandler(),
                new MeshRendererSnapshotHandler(),
                new SkinnedMeshRendererSnapshotHandler(),
                new CameraSnapshotHandler(),
                new LightSnapshotHandler(),
                new LineRendererSnapshotHandler(),
                new TrailRendererSnapshotHandler(),
                new ParticleSystemRendererSnapshotHandler(),

                // Priority 50 (Physics Handlers)
                new RigidbodySnapshotHandler(),
                new CharacterControllerSnapshotHandler(),
                new ColliderSnapshotHandler(),
                new Rigidbody2DSnapshotHandler(),
                new Collider2DSnapshotHandler(),

                // Priority 50 (Animation & Audio Handlers)
                new AnimatorSnapshotHandler(),
                new AnimationSnapshotHandler(),
                new ParticleSystemSnapshotHandler(),
                new AudioSourceSnapshotHandler(),
                new AudioListenerSnapshotHandler(),

                // Priority 50 (UI Graphics, Canvas, & Layout/Component Handlers)
                new SelectableSnapshotHandler(),
                new ImageSnapshotHandler(),
                new RawImageSnapshotHandler(),
                new TextSnapshotHandler(),
                new CanvasSnapshotHandler(),
                new CanvasGroupSnapshotHandler(),
                new ScrollRectSnapshotHandler(),
                new LayoutGroupSnapshotHandler(),
                new ContentSizeFitterSnapshotHandler(),
                new AspectRatioFitterSnapshotHandler(),

                // Priority 50 (Tilemap Handlers)
                new TilemapSnapshotHandler(),
                new GridSnapshotHandler(),
                new TilemapRendererSnapshotHandler(),

                // Priority 50 (Navigation Handlers)
                new NavMeshAgentSnapshotHandler(),
                new NavMeshObstacleSnapshotHandler(),
                new OffMeshLinkSnapshotHandler(),

                // Priority 50 (Video Handlers)
                new VideoPlayerSnapshotHandler(),

                // Priority 50 (UI Toolkit Handlers)
                new UIDocumentSnapshotHandler(),

                // Priority 0 (Fallback for custom MonoBehaviours)
                new CustomBehaviourHandler()
            };

#if UNITY_TMP_PRESENT
            ComponentHandlers.Add(new TMPInputFieldSnapshotHandler());
            ComponentHandlers.Add(new TMPDropdownSnapshotHandler());
            ComponentHandlers.Add(new TMPTextSnapshotHandler());
#endif

            SortHandlers();
        }

        private void CopyFrom(VerifySettings other)
        {
            Directory = other.Directory;
            FileName = other.FileName;
            TypeName = other.TypeName;
            FloatPrecision = other.FloatPrecision;
            ExtractDisabledObjects = other.ExtractDisabledObjects;
            ExtractTransforms = other.ExtractTransforms;
            ExtractSerializedFields = other.ExtractSerializedFields;
            IsAutoVerify = other.IsAutoVerify;

            IgnoredComponentTypes = new HashSet<Type>(other.IgnoredComponentTypes);
            IgnoredMemberNames = new HashSet<string>(other.IgnoredMemberNames, StringComparer.OrdinalIgnoreCase);

            IgnoredTypeMembers = new Dictionary<Type, HashSet<string>>();
            foreach (var kvp in other.IgnoredTypeMembers)
            {
                IgnoredTypeMembers[kvp.Key] = new HashSet<string>(kvp.Value, StringComparer.OrdinalIgnoreCase);
            }

            CustomScrubbers = new List<Func<string, object, object>>(other.CustomScrubbers);
            ComponentHandlers = new List<ISnapshotComponentHandler>(other.ComponentHandlers);
        }

        public VerifySettings Clone()
        {
            return new VerifySettings(this);
        }

        #region Fluent Configuration

        public VerifySettings UseDirectory(string directory)
        {
            Directory = directory;
            return this;
        }

        public VerifySettings UseFileName(string fileName)
        {
            FileName = fileName;
            return this;
        }

        public VerifySettings UseTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }

        public VerifySettings WithFloatPrecision(int precision)
        {
            FloatPrecision = precision;
            return this;
        }

        public VerifySettings IncludeDisabledObjects(bool include = true)
        {
            ExtractDisabledObjects = include;
            return this;
        }

        public VerifySettings IncludeTransform(bool include = true)
        {
            ExtractTransforms = include;
            return this;
        }

        public VerifySettings IncludeSerializedFields(bool include = true)
        {
            ExtractSerializedFields = include;
            return this;
        }

        public VerifySettings AutoVerify(bool autoVerify = true)
        {
            IsAutoVerify = autoVerify;
            return this;
        }

        public VerifySettings ScrubMember(string memberName, Func<string, object, object> scrubber)
        {
            if (scrubber == null) return this;
            CustomScrubbers.Add((name, val) =>
            {
                if (string.Equals(name, memberName, StringComparison.OrdinalIgnoreCase))
                {
                    return scrubber(name, val);
                }
                return val;
            });
            return this;
        }

        public VerifySettings ScrubMember(string memberName, Func<object, object> scrubber)
        {
            if (scrubber == null) return this;
            CustomScrubbers.Add((name, val) =>
            {
                if (string.Equals(name, memberName, StringComparison.OrdinalIgnoreCase))
                {
                    return scrubber(val);
                }
                return val;
            });
            return this;
        }

        public VerifySettings ScrubMember(Func<string, object, object> scrubber)
        {
            if (scrubber != null)
            {
                CustomScrubbers.Add(scrubber);
            }
            return this;
        }

        public VerifySettings IgnoreMember(string memberName)
        {
            if (!string.IsNullOrEmpty(memberName))
            {
                IgnoredMemberNames.Add(memberName);
            }
            return this;
        }

        public VerifySettings IgnoreMember<TComponent>(string memberName) where TComponent : Component
        {
            return IgnoreMember(typeof(TComponent), memberName);
        }

        public VerifySettings IgnoreMember(Type declaringType, string memberName)
        {
            if (declaringType == null || string.IsNullOrEmpty(memberName)) return this;

            if (!IgnoredTypeMembers.TryGetValue(declaringType, out var memberSet))
            {
                memberSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                IgnoredTypeMembers[declaringType] = memberSet;
            }
            memberSet.Add(memberName);
            return this;
        }

        public VerifySettings IgnoreComponent<TComponent>() where TComponent : Component
        {
            return IgnoreComponent(typeof(TComponent));
        }

        public VerifySettings IgnoreComponent(Type componentType)
        {
            if (componentType != null)
            {
                IgnoredComponentTypes.Add(componentType);
            }
            return this;
        }

        public VerifySettings RegisterComponentHandler(ISnapshotComponentHandler handler)
        {
            if (handler != null)
            {
                ComponentHandlers.RemoveAll(h => h.GetType() == handler.GetType());
                ComponentHandlers.Add(handler);
                SortHandlers();
            }
            return this;
        }

        public VerifySettings UnregisterComponentHandler(ISnapshotComponentHandler handler)
        {
            if (handler != null)
            {
                ComponentHandlers.Remove(handler);
            }
            return this;
        }

        private void SortHandlers()
        {
            ComponentHandlers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        #endregion

        #region Helpers

        public bool IsComponentIgnored(Type componentType)
        {
            if (componentType == null) return false;
            foreach (var ignoredType in IgnoredComponentTypes)
            {
                if (ignoredType.IsAssignableFrom(componentType))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsMemberIgnored(Type declaringType, string memberName)
        {
            if (string.IsNullOrEmpty(memberName)) return true;

            if (IgnoredMemberNames.Contains(memberName)) return true;

            if (declaringType != null)
            {
                foreach (var kvp in IgnoredTypeMembers)
                {
                    if (kvp.Key.IsAssignableFrom(declaringType) && kvp.Value.Contains(memberName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public ISnapshotComponentHandler FindHandler(Type componentType)
        {
            if (componentType == null) return null;
            for (int i = 0; i < ComponentHandlers.Count; i++)
            {
                if (ComponentHandlers[i].CanHandle(componentType))
                {
                    return ComponentHandlers[i];
                }
            }
            return null;
        }

        #endregion

        #region Interoperability with SnapshotOptions

        public static implicit operator VerifySettings(SnapshotOptions options)
        {
            return FromSnapshotOptions(options);
        }

        public static implicit operator SnapshotOptions(VerifySettings settings)
        {
            return settings?.ToSnapshotOptions();
        }

        public static VerifySettings FromSnapshotOptions(SnapshotOptions options)
        {
            if (options == null) return new VerifySettings();

            var settings = new VerifySettings
            {
                FloatPrecision = options.FloatPrecision,
                ExtractDisabledObjects = options.IncludeDisabledObjects,
                ExtractTransforms = options.IncludeTransform,
                ExtractSerializedFields = options.IncludeSerializedFields
            };

            if (options.IgnoredComponentTypes != null)
            {
                settings.IgnoredComponentTypes = new HashSet<Type>(options.IgnoredComponentTypes);
            }

            if (options.IgnoredPropertyNames != null)
            {
                settings.IgnoredMemberNames = new HashSet<string>(options.IgnoredPropertyNames, StringComparer.OrdinalIgnoreCase);
            }

            if (options.CustomScrubbers != null)
            {
                settings.CustomScrubbers = new List<Func<string, object, object>>(options.CustomScrubbers);
            }

            return settings;
        }

        public SnapshotOptions ToSnapshotOptions()
        {
            return new SnapshotOptions
            {
                FloatPrecision = FloatPrecision,
                IncludeDisabledObjects = ExtractDisabledObjects,
                IncludeTransform = ExtractTransforms,
                IncludeSerializedFields = ExtractSerializedFields,
                IgnoredComponentTypes = new HashSet<Type>(IgnoredComponentTypes),
                IgnoredPropertyNames = new HashSet<string>(IgnoredMemberNames, StringComparer.OrdinalIgnoreCase),
                CustomScrubbers = new List<Func<string, object, object>>(CustomScrubbers)
            };
        }

        #endregion
    }
}

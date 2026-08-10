using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Core state extraction engine for Unity GameObjects, Components, and Scenes.
    /// Extracts component states deterministically with alphabetical sorting, circular reference detection,
    /// deep collection inspection, and [SerializeField] reflection.
    /// </summary>
    public class SnapshotExtractor
    {
        private readonly VerifySettings _settings;

        public SnapshotExtractor(VerifySettings settings = null)
        {
            _settings = settings ?? VerifySettings.Global;
        }

        public SnapshotExtractor(SnapshotOptions options)
        {
            _settings = options != null ? VerifySettings.FromSnapshotOptions(options) : VerifySettings.Global;
        }

        /// <summary>
        /// Extracts the state of a single GameObject and its hierarchy.
        /// </summary>
        public GameObjectSnapshotDTO Extract(GameObject gameObject)
        {
            if (gameObject == null) return null;

            var dto = new GameObjectSnapshotDTO
            {
                Name = SnapshotScrubber.ScrubName(gameObject.name),
                ActiveSelf = gameObject.activeSelf,
                ActiveInHierarchy = gameObject.activeInHierarchy,
                Tag = gameObject.tag,
                Layer = LayerMask.LayerToName(gameObject.layer)
            };

            // Transform
            if (_settings.ExtractTransforms && gameObject.transform != null)
            {
                dto.Transform = ExtractTransform(gameObject.transform);
            }

            // Components
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp == null) continue;
                Type compType = comp.GetType();

                if (_settings.IsComponentIgnored(compType)) continue;

                ComponentSnapshotDTO compDto = ExtractComponent(comp);
                if (compDto != null)
                {
                    dto.Components.Add(compDto);
                }
            }

            // Children
            Transform transform = gameObject.transform;
            if (transform != null)
            {
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform childTransform = transform.GetChild(i);
                    if (childTransform == null) continue;

                    GameObject childGo = childTransform.gameObject;
                    if (childGo == null) continue;
                    if (!_settings.ExtractDisabledObjects && !childGo.activeSelf) continue;

                    GameObjectSnapshotDTO childDto = Extract(childGo);
                    if (childDto != null)
                    {
                        dto.Children.Add(childDto);
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Extracts states for a collection of GameObjects.
        /// </summary>
        public List<GameObjectSnapshotDTO> Extract(IEnumerable<GameObject> gameObjects)
        {
            var list = new List<GameObjectSnapshotDTO>();
            if (gameObjects == null) return list;

            foreach (var go in gameObjects)
            {
                if (go != null)
                {
                    list.Add(Extract(go));
                }
            }
            return list;
        }

        /// <summary>
        /// Extracts states for all root GameObjects in a Scene.
        /// </summary>
        public List<GameObjectSnapshotDTO> Extract(Scene scene)
        {
            var list = new List<GameObjectSnapshotDTO>();
            if (!scene.IsValid()) return list;

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var rootGo in rootObjects)
            {
                if (rootGo != null)
                {
                    list.Add(Extract(rootGo));
                }
            }
            return list;
        }

        public TransformSnapshotDTO ExtractTransform(Transform t)
        {
            if (t == null) return null;

            bool isRootOverlayCanvas = false;
            if (t is RectTransform rootRt)
            {
                isRootOverlayCanvas = CanvasSnapshotHandler.IsRootScreenSpaceOverlayCanvas(rootRt);
            }

            var dto = new TransformSnapshotDTO
            {
                LocalPosition = SnapshotScrubber.ScrubVector3(isRootOverlayCanvas ? Vector3.zero : t.localPosition, _settings.FloatPrecision),
                LocalEulerAngles = SnapshotScrubber.ScrubVector3(t.localEulerAngles, _settings.FloatPrecision),
                LocalScale = SnapshotScrubber.ScrubVector3(t.localScale, _settings.FloatPrecision)
            };

            if (t is RectTransform rt)
            {
                dto.RectTransform = new RectTransformSnapshotDTO
                {
                    AnchoredPosition = SnapshotScrubber.ScrubVector2(isRootOverlayCanvas ? Vector2.zero : rt.anchoredPosition, _settings.FloatPrecision),
                    SizeDelta = SnapshotScrubber.ScrubVector2(isRootOverlayCanvas ? Vector2.zero : rt.sizeDelta, _settings.FloatPrecision),
                    AnchorMin = SnapshotScrubber.ScrubVector2(rt.anchorMin, _settings.FloatPrecision),
                    AnchorMax = SnapshotScrubber.ScrubVector2(rt.anchorMax, _settings.FloatPrecision),
                    Pivot = SnapshotScrubber.ScrubVector2(rt.pivot, _settings.FloatPrecision)
                };
            }

            return dto;
        }

        public ComponentSnapshotDTO ExtractComponent(Component component)
        {
            if (component == null) return null;

            Type type = component.GetType();
            var dto = new ComponentSnapshotDTO
            {
                TypeName = type.Name
            };

            if (component is Behaviour behaviour)
            {
                dto.Enabled = behaviour.enabled;
            }

            var rawState = new Dictionary<string, object>(StringComparer.Ordinal);

            // Handler Registry lookup
            ISnapshotComponentHandler handler = _settings.FindHandler(type);
            if (handler != null)
            {
                handler.Extract(component, rawState, _settings);
            }
            else
            {
                ExtractGeneralComponentState(component, type, rawState);
            }

            // Deterministic sorting: sort keys alphabetically (Ordinal)
            foreach (var kvp in rawState.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                dto.StateEntries.Add(new ComponentStateEntry(kvp.Key, ConvertValueToString(kvp.Value)));
            }

            return dto;
        }

        public void ExtractGeneralComponentState(Component component, Type type, IDictionary<string, object> state)
        {
            CustomBehaviourHandler.ExtractComponentState(component, type, state, _settings);
        }

        /// <summary>
        /// Deeply extracts values, scrubbing primitives/Unity math types, traversing dictionaries/collections,
        /// detecting circular references, and sorting object keys.
        /// </summary>
        public object DeepExtractValue(string memberName, object value, HashSet<object> visited)
        {
            return CustomBehaviourHandler.DeepExtractValue(memberName, value, visited, _settings);
        }

        /// <summary>
        /// Formats an extracted object value into a clean, deterministic string for ComponentStateEntry.
        /// </summary>
        public string ConvertValueToString(object val)
        {
            if (val == null) return "null";
            if (val is string s) return s;
            if (val is bool b) return b ? "true" : "false";
            if (val is float f) return SnapshotScrubber.RoundFloat(f, _settings.FloatPrecision).ToString(CultureInfo.InvariantCulture);
            if (val is double d) return SnapshotScrubber.RoundDouble(d, _settings.FloatPrecision).ToString(CultureInfo.InvariantCulture);
            if (val is decimal m) return Math.Round(m, _settings.FloatPrecision, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);

            if (val is Vector2DTO v2) return $"({v2.X.ToString(CultureInfo.InvariantCulture)}, {v2.Y.ToString(CultureInfo.InvariantCulture)})";
            if (val is Vector3DTO v3) return $"({v3.X.ToString(CultureInfo.InvariantCulture)}, {v3.Y.ToString(CultureInfo.InvariantCulture)}, {v3.Z.ToString(CultureInfo.InvariantCulture)})";
            if (val is Vector4DTO v4) return $"({v4.X.ToString(CultureInfo.InvariantCulture)}, {v4.Y.ToString(CultureInfo.InvariantCulture)}, {v4.Z.ToString(CultureInfo.InvariantCulture)}, {v4.W.ToString(CultureInfo.InvariantCulture)})";
            if (val is QuaternionDTO q) return $"({q.X.ToString(CultureInfo.InvariantCulture)}, {q.Y.ToString(CultureInfo.InvariantCulture)}, {q.Z.ToString(CultureInfo.InvariantCulture)}, {q.W.ToString(CultureInfo.InvariantCulture)})";
            if (val is RectDTO r) return $"(x:{r.X.ToString(CultureInfo.InvariantCulture)}, y:{r.Y.ToString(CultureInfo.InvariantCulture)}, width:{r.Width.ToString(CultureInfo.InvariantCulture)}, height:{r.Height.ToString(CultureInfo.InvariantCulture)})";
            if (val is BoundsDTO bounds) return $"(Center: {ConvertValueToString(bounds.Center)}, Size: {ConvertValueToString(bounds.Size)})";

            if (val is IDictionary<string, object> dict)
            {
                var pairs = dict.OrderBy(kv => kv.Key, StringComparer.Ordinal)
                                .Select(kv => $"{kv.Key}: {ConvertValueToString(kv.Value)}");
                return $"{{{string.Join(", ", pairs)}}}";
            }

            if (val is IEnumerable<object> list)
            {
                var items = list.Select(ConvertValueToString);
                return $"[{string.Join(", ", items)}]";
            }

            return val.ToString();
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                return obj == null ? 0 : RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}

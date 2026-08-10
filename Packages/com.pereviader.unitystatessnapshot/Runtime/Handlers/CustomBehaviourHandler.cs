using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Fallback snapshot handler for custom MonoBehaviours and components.
    /// Inspects public fields, [SerializeField] fields, and public readable properties,
    /// respecting [SnapshotIgnore], circular reference detection, and deterministic sorting.
    /// </summary>
    public class CustomBehaviourHandler : ISnapshotComponentHandler
    {
        public int Priority => 0;

        public bool CanHandle(Type componentType)
        {
            return typeof(MonoBehaviour).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component == null) return;
            ExtractComponentState(component, component.GetType(), targetState, settings);
        }

        public static void ExtractComponentState(Component component, Type type, IDictionary<string, object> state, VerifySettings settings)
        {
            if (component == null || type == null || state == null) return;
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

            // 1. Public fields
            FieldInfo[] publicFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in publicFields)
            {
                if (IsBaseUnityType(field.DeclaringType)) continue;
                if (Attribute.IsDefined(field, typeof(ObsoleteAttribute))) continue;
                if (settings != null && settings.IsMemberIgnored(type, field.Name)) continue;
                if (Attribute.IsDefined(field, typeof(SnapshotIgnoreAttribute))) continue;

                try
                {
                    object rawVal = field.GetValue(component);
                    object scrubbedVal = DeepExtractValue(field.Name, rawVal, visited, settings);
                    state[field.Name] = scrubbedVal;
                }
                catch
                {
                    // Skip throwing fields
                }
            }

            // 2. Private/Protected [SerializeField] fields
            if (settings == null || settings.ExtractSerializedFields)
            {
                Type currentType = type;
                while (currentType != null && !IsBaseUnityType(currentType))
                {
                    FieldInfo[] nonPublicFields = currentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    foreach (FieldInfo field in nonPublicFields)
                    {
                        if (field.Name.Contains("k__BackingField")) continue;
                        if (!Attribute.IsDefined(field, typeof(SerializeField))) continue;
                        if (Attribute.IsDefined(field, typeof(ObsoleteAttribute))) continue;
                        if (settings != null && settings.IsMemberIgnored(type, field.Name)) continue;
                        if (Attribute.IsDefined(field, typeof(SnapshotIgnoreAttribute))) continue;
                        if (state.ContainsKey(field.Name)) continue;

                        try
                        {
                            object rawVal = field.GetValue(component);
                            object scrubbedVal = DeepExtractValue(field.Name, rawVal, visited, settings);
                            state[field.Name] = scrubbedVal;
                        }
                        catch
                        {
                            // Skip throwing fields
                        }
                    }
                    currentType = currentType.BaseType;
                }
            }

            // 3. Public readable properties
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanRead) continue;
                if (prop.GetIndexParameters().Length > 0) continue;
                if (IsBaseUnityType(prop.DeclaringType)) continue;
                if (Attribute.IsDefined(prop, typeof(ObsoleteAttribute))) continue;
                if (settings != null && settings.IsMemberIgnored(type, prop.Name)) continue;
                if (Attribute.IsDefined(prop, typeof(SnapshotIgnoreAttribute))) continue;

                try
                {
                    object rawVal = prop.GetValue(component, null);
                    object scrubbedVal = DeepExtractValue(prop.Name, rawVal, visited, settings);
                    state[prop.Name] = scrubbedVal;
                }
                catch
                {
                    // Skip throwing properties
                }
            }
        }

        private static bool IsBaseUnityType(Type t)
        {
            if (t == null) return true;
            return t == typeof(MonoBehaviour) ||
                   t == typeof(Behaviour) ||
                   t == typeof(Component) ||
                   t == typeof(Renderer) ||
                   t == typeof(Collider) ||
                   t == typeof(Collider2D) ||
                   t == typeof(CanvasRenderer) ||
                   t == typeof(UnityEngine.Object) ||
                   t == typeof(object);
        }

        public static object DeepExtractValue(string memberName, object value, HashSet<object> visited, VerifySettings settings)
        {
            if (value == null) return null;

            // Apply custom scrubbers and known engine type scrubbing
            object scrubbed = SnapshotScrubber.ScrubValue(memberName, value, settings);
            if (scrubbed == null) return null;

            // If value was scrubbed to a different type/string (e.g. Color to hex or UnityEngine.Object to Ref<T>), return it
            if (!ReferenceEquals(scrubbed, value) && !(scrubbed is ValueType && scrubbed.Equals(value)))
            {
                return scrubbed;
            }

            Type valType = value.GetType();

            // Simple / primitive types
            if (valType.IsPrimitive || valType.IsEnum || value is string || value is decimal || value is Guid || value is DateTime || value is TimeSpan)
            {
                return value;
            }

            // Circular reference protection for reference types
            if (!valType.IsValueType)
            {
                if (!visited.Add(value))
                {
                    return "[Circular Reference]";
                }
            }

            // Dictionaries
            if (value is IDictionary dict)
            {
                var sortedDict = new SortedDictionary<string, object>(StringComparer.Ordinal);
                foreach (DictionaryEntry entry in dict)
                {
                    string keyStr = entry.Key?.ToString() ?? "null";
                    sortedDict[keyStr] = DeepExtractValue(keyStr, entry.Value, visited, settings);
                }
                return sortedDict;
            }

            // Collections / Arrays / IEnumerable
            if (value is IEnumerable enumerable && !(value is string))
            {
                var list = new List<object>();
                int index = 0;
                foreach (var item in enumerable)
                {
                    list.Add(DeepExtractValue($"{memberName}[{index}]", item, visited, settings));
                    index++;
                }
                return list;
            }

            // Complex custom class / struct
            if (valType.IsClass || (valType.IsValueType && !valType.IsPrimitive && !valType.IsEnum))
            {
                // If it's a DTO already, return as is
                if (value is Vector2DTO || value is Vector3DTO || value is Vector4DTO || value is QuaternionDTO || value is RectDTO || value is BoundsDTO)
                {
                    return value;
                }

                var objectState = new SortedDictionary<string, object>(StringComparer.Ordinal);

                // Fields
                FieldInfo[] fields = valType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                foreach (FieldInfo field in fields)
                {
                    if (Attribute.IsDefined(field, typeof(SnapshotIgnoreAttribute))) continue;
                    try
                    {
                        object fVal = field.GetValue(value);
                        objectState[field.Name] = DeepExtractValue(field.Name, fVal, visited, settings);
                    }
                    catch { }
                }

                // Properties
                PropertyInfo[] props = valType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo prop in props)
                {
                    if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;
                    if (Attribute.IsDefined(prop, typeof(SnapshotIgnoreAttribute))) continue;
                    try
                    {
                        object pVal = prop.GetValue(value, null);
                        objectState[prop.Name] = DeepExtractValue(prop.Name, pVal, visited, settings);
                    }
                    catch { }
                }

                if (objectState.Count > 0)
                {
                    return objectState;
                }
            }

            return value;
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

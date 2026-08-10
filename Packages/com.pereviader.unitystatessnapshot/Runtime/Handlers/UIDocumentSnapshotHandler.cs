using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UIElements.UIDocument components.
    /// Extracts visualTreeAsset name, panelSettings name, sortingOrder, and recursive visual tree of rootVisualElement.
    /// </summary>
    public class UIDocumentSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(UIDocument).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not UIDocument doc) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["VisualTreeAsset"] = doc.visualTreeAsset != null ? SnapshotScrubber.ScrubName(doc.visualTreeAsset.name) : "null";
            targetState["PanelSettings"] = doc.panelSettings != null ? SnapshotScrubber.ScrubName(doc.panelSettings.name) : "null";
            targetState["SortingOrder"] = SnapshotScrubber.RoundFloat(doc.sortingOrder, precision);

            if (doc.rootVisualElement != null)
            {
                targetState["RootVisualElement"] = ExtractVisualElement(doc.rootVisualElement, precision);
            }
            else
            {
                targetState["RootVisualElement"] = "null";
            }
        }

        public static Dictionary<string, object> ExtractVisualElement(VisualElement element, int precision)
        {
            if (element == null) return null;

            var node = new Dictionary<string, object>(StringComparer.Ordinal);
            node["Name"] = string.IsNullOrEmpty(element.name) ? "" : SnapshotScrubber.ScrubName(element.name);
            node["TypeName"] = element.GetType().Name;

            // Extract text/value based on specific VisualElement types
            if (element is Button btn)
            {
                node["Text"] = btn.text ?? string.Empty;
            }
            else if (element is Label lbl)
            {
                node["Text"] = lbl.text ?? string.Empty;
            }
            else if (element is TextField tf)
            {
                node["Value"] = tf.value ?? string.Empty;
            }
            else if (element is Toggle tog)
            {
                node["Value"] = tog.value;
            }
            else if (element is TextElement te)
            {
                node["Text"] = te.text ?? string.Empty;
            }
            else if (element is INotifyValueChanged<string> strNotify)
            {
                node["Value"] = strNotify.value ?? string.Empty;
            }
            else if (element is INotifyValueChanged<bool> boolNotify)
            {
                node["Value"] = boolNotify.value;
            }

            node["Visible"] = element.visible;
            node["EnabledSelf"] = element.enabledSelf;

            var classes = element.GetClasses()?.ToList();
            if (classes != null && classes.Count > 0)
            {
                classes.Sort(StringComparer.Ordinal);
                node["Classes"] = classes;
            }

            node["Layout"] = SnapshotScrubber.ScrubRect(element.layout, precision);

            if (element.childCount > 0)
            {
                var children = new List<object>();
                for (int i = 0; i < element.childCount; i++)
                {
                    var childNode = ExtractVisualElement(element[i], precision);
                    if (childNode != null)
                    {
                        children.Add(childNode);
                    }
                }
                if (children.Count > 0)
                {
                    node["Children"] = children;
                }
            }

            return node;
        }
    }
}

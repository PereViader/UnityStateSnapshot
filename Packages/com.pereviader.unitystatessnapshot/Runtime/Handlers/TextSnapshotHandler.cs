using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.Text components.
    /// </summary>
    public class TextSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Text).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Text text) return;

            targetState["Text"] = text.text;
            targetState["Color"] = SnapshotScrubber.ScrubColor(text.color);
            targetState["FontSize"] = text.fontSize;
            targetState["Alignment"] = text.alignment.ToString();
        }
    }
}

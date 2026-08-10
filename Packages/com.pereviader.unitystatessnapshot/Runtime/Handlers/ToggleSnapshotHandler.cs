using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.Toggle components.
    /// Extracts IsOn, ToggleTransition, and Group reference.
    /// </summary>
    public class ToggleSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 60;

        public bool CanHandle(Type componentType)
        {
            return typeof(Toggle).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Toggle toggle) return;

            // Extract selectable base state
            SelectableSnapshotHandler.ExtractSelectableState(toggle, targetState);

            targetState["IsOn"] = toggle.isOn;
            targetState["ToggleTransition"] = toggle.toggleTransition.ToString();
            targetState["Group"] = toggle.group != null ? SnapshotScrubber.ScrubName(toggle.group.name) : "null";
        }
    }
}

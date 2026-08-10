#if UNITY_TMP_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for TMPro.TMP_Dropdown components.
    /// </summary>
    public class TMPDropdownSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 60;

        public bool CanHandle(Type componentType)
        {
            return typeof(TMP_Dropdown).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not TMP_Dropdown dropdown) return;

            SelectableSnapshotHandler.ExtractSelectableState(dropdown, targetState);

            targetState["Value"] = dropdown.value;
            targetState["CaptionText"] = dropdown.captionText != null ? dropdown.captionText.text : string.Empty;

            var options = new List<string>();
            if (dropdown.options != null)
            {
                foreach (var opt in dropdown.options)
                {
                    options.Add(opt != null ? opt.text : string.Empty);
                }
            }

            targetState["OptionsCount"] = options.Count;
            targetState["Options"] = options;
        }
    }
}
#endif

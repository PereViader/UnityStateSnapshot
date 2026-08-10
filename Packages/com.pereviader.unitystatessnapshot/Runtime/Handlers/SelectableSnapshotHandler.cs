using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.Selectable components.
    /// </summary>
    public class SelectableSnapshotHandler : ISnapshotComponentHandler
    {
        public virtual int Priority => 50;

        public virtual bool CanHandle(Type componentType)
        {
            return typeof(Selectable).IsAssignableFrom(componentType);
        }

        public virtual void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Selectable selectable) return;
            ExtractSelectableState(selectable, targetState);
        }

        internal static void ExtractSelectableState(Selectable selectable, IDictionary<string, object> targetState)
        {
            targetState["Interactable"] = selectable.interactable;

            bool isSelected = false;
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == selectable.gameObject)
            {
                isSelected = true;
            }
            targetState["IsSelected"] = isSelected;

            PropertyInfo propState = typeof(Selectable).GetProperty("currentSelectionState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (propState != null)
            {
                targetState["SelectionState"] = propState.GetValue(selectable)?.ToString() ?? "Normal";
            }
            else
            {
                targetState["SelectionState"] = isSelected ? "Selected" : (selectable.interactable ? "Normal" : "Disabled");
            }
        }
    }
}

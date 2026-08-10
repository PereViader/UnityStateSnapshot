using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.Button components.
    /// </summary>
    public class ButtonSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 60;

        public bool CanHandle(Type componentType)
        {
            return typeof(Button).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Button button) return;

            SelectableSnapshotHandler.ExtractSelectableState(button, targetState);
            targetState["PersistentListenerCount"] = button.onClick.GetPersistentEventCount();
        }
    }
}

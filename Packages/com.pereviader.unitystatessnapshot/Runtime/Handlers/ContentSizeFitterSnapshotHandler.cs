using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.ContentSizeFitter components.
    /// </summary>
    public class ContentSizeFitterSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(ContentSizeFitter).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not ContentSizeFitter csf) return;

            targetState["HorizontalFit"] = csf.horizontalFit.ToString();
            targetState["VerticalFit"] = csf.verticalFit.ToString();
        }
    }
}

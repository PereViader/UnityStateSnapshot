using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.Image components.
    /// </summary>
    public class ImageSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Image).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Image image) return;

            targetState["Sprite"] = image.sprite != null ? image.sprite.name : "null";
            targetState["Color"] = SnapshotScrubber.ScrubColor(image.color);
            targetState["Type"] = image.type.ToString();
            targetState["RaycastTarget"] = image.raycastTarget;
        }
    }
}

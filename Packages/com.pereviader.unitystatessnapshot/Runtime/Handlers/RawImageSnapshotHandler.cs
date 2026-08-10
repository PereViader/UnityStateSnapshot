using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.RawImage components.
    /// </summary>
    public class RawImageSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(RawImage).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not RawImage rawImage) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Texture"] = rawImage.texture != null ? SnapshotScrubber.ScrubName(rawImage.texture.name) : "null";
            targetState["Color"] = SnapshotScrubber.ScrubColor(rawImage.color);
            targetState["RaycastTarget"] = rawImage.raycastTarget;
            targetState["UVRect"] = SnapshotScrubber.ScrubRect(rawImage.uvRect, precision);
        }
    }
}

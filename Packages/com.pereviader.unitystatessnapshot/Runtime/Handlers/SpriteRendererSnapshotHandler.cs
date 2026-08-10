using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.SpriteRenderer components.
    /// Extracts sprite name, color hex, flipX, flipY, drawMode, size, sortingLayerName, sortingOrder, and maskInteraction.
    /// </summary>
    public class SpriteRendererSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(SpriteRenderer).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not SpriteRenderer sr) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Sprite"] = sr.sprite != null ? SnapshotScrubber.ScrubName(sr.sprite.name) : "null";
            targetState["Color"] = SnapshotScrubber.ScrubColor(sr.color);
            targetState["FlipX"] = sr.flipX;
            targetState["FlipY"] = sr.flipY;
            targetState["DrawMode"] = sr.drawMode.ToString();
            targetState["Size"] = SnapshotScrubber.ScrubVector2(sr.size, precision);
            targetState["SortingLayerName"] = sr.sortingLayerName;
            targetState["SortingOrder"] = sr.sortingOrder;
            targetState["MaskInteraction"] = sr.maskInteraction.ToString();
        }
    }
}

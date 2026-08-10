using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Canvas components.
    /// Normalizes Canvas and ScreenSpaceOverlay properties while excluding dynamic viewport-dependent properties
    /// such as pixelRect, renderingDisplaySize, worldCamera, scaleFactor, and cachedTargetSize.
    /// </summary>
    public class CanvasSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Canvas).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Canvas canvas) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["renderMode"] = canvas.renderMode.ToString();
            targetState["isRootCanvas"] = canvas.isRootCanvas;
            targetState["overrideSorting"] = canvas.overrideSorting;
            targetState["sortingOrder"] = canvas.sortingOrder;
            targetState["targetDisplay"] = canvas.targetDisplay;
            targetState["sortingLayerID"] = canvas.sortingLayerID;
            targetState["sortingLayerName"] = canvas.sortingLayerName;
            targetState["additionalShaderChannels"] = canvas.additionalShaderChannels.ToString();
            targetState["pixelPerfect"] = canvas.pixelPerfect;
            targetState["overridePixelPerfect"] = canvas.overridePixelPerfect;
            targetState["planeDistance"] = SnapshotScrubber.RoundFloat(canvas.planeDistance, precision);
            targetState["referencePixelsPerUnit"] = SnapshotScrubber.RoundFloat(canvas.referencePixelsPerUnit, precision);
        }

        /// <summary>
        /// Checks if the given RectTransform belongs to a root Canvas configured for ScreenSpaceOverlay.
        /// </summary>
        public static bool IsRootScreenSpaceOverlayCanvas(RectTransform rt)
        {
            if (rt == null) return false;
            if (rt.TryGetComponent<Canvas>(out var canvas) && canvas != null)
            {
                return canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay;
            }
            return false;
        }
    }
}

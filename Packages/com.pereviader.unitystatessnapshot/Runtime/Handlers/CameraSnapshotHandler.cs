using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Camera components.
    /// Extracts clearFlags, backgroundColor hex, cullingMask, orthographic, orthographicSize, fieldOfView,
    /// nearClipPlane, farClipPlane, depth, rect, allowHDR, and allowMSAA without accessing dynamic viewport pixel properties.
    /// </summary>
    public class CameraSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Camera).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Camera cam) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["ClearFlags"] = cam.clearFlags.ToString();
            targetState["BackgroundColor"] = SnapshotScrubber.ScrubColor(cam.backgroundColor);
            targetState["CullingMask"] = cam.cullingMask;
            targetState["Orthographic"] = cam.orthographic;
            targetState["OrthographicSize"] = SnapshotScrubber.RoundFloat(cam.orthographicSize, precision);
            targetState["FieldOfView"] = SnapshotScrubber.RoundFloat(cam.fieldOfView, precision);
            targetState["NearClipPlane"] = SnapshotScrubber.RoundFloat(cam.nearClipPlane, precision);
            targetState["FarClipPlane"] = SnapshotScrubber.RoundFloat(cam.farClipPlane, precision);
            targetState["Depth"] = SnapshotScrubber.RoundFloat(cam.depth, precision);
            targetState["Rect"] = SnapshotScrubber.ScrubRect(cam.rect, precision);
            targetState["AllowHDR"] = cam.allowHDR;
            targetState["AllowMSAA"] = cam.allowMSAA;
        }
    }
}

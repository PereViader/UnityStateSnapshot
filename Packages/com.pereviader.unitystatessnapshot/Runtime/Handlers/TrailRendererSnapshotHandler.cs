using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.TrailRenderer components.
    /// Extracts time, startWidth, endWidth, startColor hex, endColor hex, minVertexDistance, and autodestruct.
    /// </summary>
    public class TrailRendererSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(TrailRenderer).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not TrailRenderer tr) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Time"] = SnapshotScrubber.RoundFloat(tr.time, precision);
            targetState["StartWidth"] = SnapshotScrubber.RoundFloat(tr.startWidth, precision);
            targetState["EndWidth"] = SnapshotScrubber.RoundFloat(tr.endWidth, precision);
            targetState["StartColor"] = SnapshotScrubber.ScrubColor(tr.startColor);
            targetState["EndColor"] = SnapshotScrubber.ScrubColor(tr.endColor);
            targetState["MinVertexDistance"] = SnapshotScrubber.RoundFloat(tr.minVertexDistance, precision);
            targetState["Autodestruct"] = tr.autodestruct;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.LineRenderer components.
    /// Extracts positionCount, startWidth, endWidth, startColor hex, endColor hex, loop, and alignment.
    /// </summary>
    public class LineRendererSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(LineRenderer).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not LineRenderer lr) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["PositionCount"] = lr.positionCount;
            targetState["StartWidth"] = SnapshotScrubber.RoundFloat(lr.startWidth, precision);
            targetState["EndWidth"] = SnapshotScrubber.RoundFloat(lr.endWidth, precision);
            targetState["StartColor"] = SnapshotScrubber.ScrubColor(lr.startColor);
            targetState["EndColor"] = SnapshotScrubber.ScrubColor(lr.endColor);
            targetState["Loop"] = lr.loop;
            targetState["Alignment"] = lr.alignment.ToString();
        }
    }
}

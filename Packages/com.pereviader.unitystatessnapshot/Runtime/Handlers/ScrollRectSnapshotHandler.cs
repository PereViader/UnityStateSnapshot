using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.ScrollRect components.
    /// Extracts Horizontal, Vertical, MovementType, Elasticity, NormalizedPosition.
    /// </summary>
    public class ScrollRectSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(ScrollRect).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not ScrollRect sr) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Horizontal"] = sr.horizontal;
            targetState["Vertical"] = sr.vertical;
            targetState["MovementType"] = sr.movementType.ToString();
            targetState["Elasticity"] = SnapshotScrubber.RoundFloat(sr.elasticity, precision);
            targetState["Inertia"] = sr.inertia;
            targetState["DecelerationRate"] = SnapshotScrubber.RoundFloat(sr.decelerationRate, precision);
            targetState["ScrollSensitivity"] = SnapshotScrubber.RoundFloat(sr.scrollSensitivity, precision);
            targetState["HorizontalNormalizedPosition"] = SnapshotScrubber.RoundFloat(sr.horizontalNormalizedPosition, precision);
            targetState["VerticalNormalizedPosition"] = SnapshotScrubber.RoundFloat(sr.verticalNormalizedPosition, precision);
        }
    }
}

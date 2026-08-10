using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.AspectRatioFitter components.
    /// </summary>
    public class AspectRatioFitterSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(AspectRatioFitter).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not AspectRatioFitter arf) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["AspectMode"] = arf.aspectMode.ToString();
            targetState["AspectRatio"] = SnapshotScrubber.RoundFloat(arf.aspectRatio, precision);
        }
    }
}

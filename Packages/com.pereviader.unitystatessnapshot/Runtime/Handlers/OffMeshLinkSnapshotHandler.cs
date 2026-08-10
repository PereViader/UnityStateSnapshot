#pragma warning disable CS0618

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.AI.OffMeshLink components.
    /// Extracts costOverride, biDirectional, activated, startTransform name, and endTransform name.
    /// </summary>
    public class OffMeshLinkSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(OffMeshLink).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not OffMeshLink link) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["CostOverride"] = SnapshotScrubber.RoundFloat(link.costOverride, precision);
            targetState["BiDirectional"] = link.biDirectional;
            targetState["Activated"] = link.activated;
            targetState["StartTransform"] = link.startTransform != null ? SnapshotScrubber.ScrubName(link.startTransform.name) : "null";
            targetState["EndTransform"] = link.endTransform != null ? SnapshotScrubber.ScrubName(link.endTransform.name) : "null";
        }
    }
}

#pragma warning restore CS0618

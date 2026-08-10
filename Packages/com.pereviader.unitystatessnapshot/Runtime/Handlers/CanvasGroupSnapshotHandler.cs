using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.CanvasGroup components.
    /// Extracts Alpha, Interactable, BlocksRaycasts, and IgnoreParentGroups.
    /// </summary>
    public class CanvasGroupSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(CanvasGroup).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not CanvasGroup cg) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Alpha"] = SnapshotScrubber.RoundFloat(cg.alpha, precision);
            targetState["Interactable"] = cg.interactable;
            targetState["BlocksRaycasts"] = cg.blocksRaycasts;
            targetState["IgnoreParentGroups"] = cg.ignoreParentGroups;
        }
    }
}

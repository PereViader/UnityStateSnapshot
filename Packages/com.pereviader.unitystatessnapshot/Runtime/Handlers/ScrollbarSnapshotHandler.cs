using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.Scrollbar components.
    /// Extracts Value, Size, NumberOfSteps, and Direction.
    /// </summary>
    public class ScrollbarSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 60;

        public bool CanHandle(Type componentType)
        {
            return typeof(Scrollbar).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Scrollbar scrollbar) return;

            int precision = settings?.FloatPrecision ?? 3;

            // Extract selectable base state
            SelectableSnapshotHandler.ExtractSelectableState(scrollbar, targetState);

            targetState["Value"] = SnapshotScrubber.RoundFloat(scrollbar.value, precision);
            targetState["Size"] = SnapshotScrubber.RoundFloat(scrollbar.size, precision);
            targetState["NumberOfSteps"] = scrollbar.numberOfSteps;
            targetState["Direction"] = scrollbar.direction.ToString();
        }
    }
}

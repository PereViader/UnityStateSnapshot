using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.Slider components.
    /// Extracts Value, MinValue, MaxValue, WholeNumbers, Direction, and NormalizedValue.
    /// </summary>
    public class SliderSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 60;

        public bool CanHandle(Type componentType)
        {
            return typeof(Slider).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Slider slider) return;

            int precision = settings?.FloatPrecision ?? 3;

            // Extract selectable base state
            SelectableSnapshotHandler.ExtractSelectableState(slider, targetState);

            targetState["Value"] = SnapshotScrubber.RoundFloat(slider.value, precision);
            targetState["MinValue"] = SnapshotScrubber.RoundFloat(slider.minValue, precision);
            targetState["MaxValue"] = SnapshotScrubber.RoundFloat(slider.maxValue, precision);
            targetState["WholeNumbers"] = slider.wholeNumbers;
            targetState["Direction"] = slider.direction.ToString();
            targetState["NormalizedValue"] = SnapshotScrubber.RoundFloat(slider.normalizedValue, precision);
        }
    }
}

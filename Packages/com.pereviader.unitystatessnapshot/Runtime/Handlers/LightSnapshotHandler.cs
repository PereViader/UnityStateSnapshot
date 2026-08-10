using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Light components.
    /// Extracts type, color hex, intensity, range, spotAngle, shadows, and shadowStrength.
    /// </summary>
    public class LightSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Light).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Light light) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Type"] = light.type.ToString();
            targetState["Color"] = SnapshotScrubber.ScrubColor(light.color);
            targetState["Intensity"] = SnapshotScrubber.RoundFloat(light.intensity, precision);
            targetState["Range"] = SnapshotScrubber.RoundFloat(light.range, precision);
            targetState["SpotAngle"] = SnapshotScrubber.RoundFloat(light.spotAngle, precision);
            targetState["Shadows"] = light.shadows.ToString();
            targetState["ShadowStrength"] = SnapshotScrubber.RoundFloat(light.shadowStrength, precision);
        }
    }
}

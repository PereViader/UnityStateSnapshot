using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.AudioListener components.
    /// Extracts volume and pause states.
    /// </summary>
    public class AudioListenerSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(AudioListener).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not AudioListener) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Volume"] = SnapshotScrubber.RoundFloat(AudioListener.volume, precision);
            targetState["Pause"] = AudioListener.pause;
        }
    }
}

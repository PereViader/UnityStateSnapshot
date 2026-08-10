using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.AudioSource components.
    /// Extracts clip name, volume, pitch, loop, playOnAwake, mute, spatialBlend, priority, and rolloff configuration.
    /// </summary>
    public class AudioSourceSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(AudioSource).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not AudioSource audio) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Clip"] = audio.clip != null ? SnapshotScrubber.ScrubName(audio.clip.name) : "null";
            targetState["Volume"] = SnapshotScrubber.RoundFloat(audio.volume, precision);
            targetState["Pitch"] = SnapshotScrubber.RoundFloat(audio.pitch, precision);
            targetState["Loop"] = audio.loop;
            targetState["PlayOnAwake"] = audio.playOnAwake;
            targetState["Mute"] = audio.mute;
            targetState["SpatialBlend"] = SnapshotScrubber.RoundFloat(audio.spatialBlend, precision);
            targetState["Priority"] = audio.priority;
            targetState["IsPlaying"] = audio.isPlaying;
            targetState["RolloffMode"] = audio.rolloffMode.ToString();
            targetState["MinDistance"] = SnapshotScrubber.RoundFloat(audio.minDistance, precision);
            targetState["MaxDistance"] = SnapshotScrubber.RoundFloat(audio.maxDistance, precision);
            targetState["OutputAudioMixerGroup"] = audio.outputAudioMixerGroup != null ? SnapshotScrubber.ScrubName(audio.outputAudioMixerGroup.name) : "null";
        }
    }
}

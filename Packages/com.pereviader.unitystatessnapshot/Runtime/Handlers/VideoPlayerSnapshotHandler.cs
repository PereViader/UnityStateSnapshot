using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Video.VideoPlayer components.
    /// Extracts clip name, url, isLooping, playOnAwake, renderMode, playbackSpeed, isPlaying, and aspectRatio.
    /// </summary>
    public class VideoPlayerSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(VideoPlayer).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not VideoPlayer player) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Clip"] = player.clip != null ? SnapshotScrubber.ScrubName(player.clip.name) : "null";
            targetState["Url"] = player.url ?? string.Empty;
            targetState["IsLooping"] = player.isLooping;
            targetState["PlayOnAwake"] = player.playOnAwake;
            targetState["RenderMode"] = player.renderMode.ToString();
            targetState["PlaybackSpeed"] = SnapshotScrubber.RoundFloat((float)player.playbackSpeed, precision);
            targetState["IsPlaying"] = player.isPlaying;
            targetState["AspectRatio"] = player.aspectRatio.ToString();
        }
    }
}

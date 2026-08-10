using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for legacy UnityEngine.Animation components.
    /// Extracts clip name, isPlaying, playAutomatically, wrapMode, and animatePhysics.
    /// </summary>
    public class AnimationSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Animation).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Animation anim) return;

            targetState["Clip"] = anim.clip != null ? SnapshotScrubber.ScrubName(anim.clip.name) : "null";
            targetState["IsPlaying"] = anim.isPlaying;
            targetState["PlayAutomatically"] = anim.playAutomatically;
            targetState["WrapMode"] = anim.wrapMode.ToString();
            targetState["AnimatePhysics"] = anim.animatePhysics;
        }
    }
}

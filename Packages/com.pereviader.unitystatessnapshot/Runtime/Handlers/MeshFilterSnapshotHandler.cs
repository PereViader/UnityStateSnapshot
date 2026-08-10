using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.MeshFilter components.
    /// Extracts sharedMesh name without instantiating duplicate mesh copies.
    /// </summary>
    public class MeshFilterSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(MeshFilter).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not MeshFilter mf) return;

            targetState["SharedMesh"] = mf.sharedMesh != null ? SnapshotScrubber.ScrubName(mf.sharedMesh.name) : "null";
        }
    }
}

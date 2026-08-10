using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.SkinnedMeshRenderer components.
    /// Extracts sharedMesh name, rootBone name, sharedMaterials names, quality, and updateWhenOffscreen.
    /// </summary>
    public class SkinnedMeshRendererSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(SkinnedMeshRenderer).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not SkinnedMeshRenderer smr) return;

            targetState["SharedMesh"] = smr.sharedMesh != null ? SnapshotScrubber.ScrubName(smr.sharedMesh.name) : "null";
            targetState["RootBone"] = smr.rootBone != null ? SnapshotScrubber.ScrubName(smr.rootBone.name) : "null";

            var matNames = new List<string>();
            if (smr.sharedMaterials != null)
            {
                foreach (var mat in smr.sharedMaterials)
                {
                    matNames.Add(mat != null ? SnapshotScrubber.ScrubName(mat.name) : "null");
                }
            }

            targetState["SharedMaterials"] = matNames;
            targetState["Quality"] = smr.quality.ToString();
            targetState["UpdateWhenOffscreen"] = smr.updateWhenOffscreen;
        }
    }
}

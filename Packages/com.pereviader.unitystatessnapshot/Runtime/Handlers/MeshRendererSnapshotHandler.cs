using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.MeshRenderer components.
    /// Extracts sharedMaterials names, shadowCastingMode, receiveShadows, sortingLayerName, sortingOrder, lightProbeUsage, and reflectionProbeUsage.
    /// </summary>
    public class MeshRendererSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(MeshRenderer).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not MeshRenderer mr) return;

            var matNames = new List<string>();
            if (mr.sharedMaterials != null)
            {
                foreach (var mat in mr.sharedMaterials)
                {
                    matNames.Add(mat != null ? SnapshotScrubber.ScrubName(mat.name) : "null");
                }
            }

            targetState["SharedMaterials"] = matNames;
            targetState["ShadowCastingMode"] = mr.shadowCastingMode.ToString();
            targetState["ReceiveShadows"] = mr.receiveShadows;
            targetState["SortingLayerName"] = mr.sortingLayerName;
            targetState["SortingOrder"] = mr.sortingOrder;
            targetState["LightProbeUsage"] = mr.lightProbeUsage.ToString();
            targetState["ReflectionProbeUsage"] = mr.reflectionProbeUsage.ToString();
        }
    }
}

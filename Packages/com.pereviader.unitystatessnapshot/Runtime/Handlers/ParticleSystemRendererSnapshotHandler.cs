using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.ParticleSystemRenderer components.
    /// Extracts renderMode, alignment, sortingLayerName, sortingOrder, and sharedMaterials without instantiating material clones.
    /// </summary>
    public class ParticleSystemRendererSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(ParticleSystemRenderer).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not ParticleSystemRenderer psr) return;

            var matNames = new List<string>();
            if (psr.sharedMaterials != null)
            {
                foreach (var mat in psr.sharedMaterials)
                {
                    matNames.Add(mat != null ? SnapshotScrubber.ScrubName(mat.name) : "null");
                }
            }

            targetState["RenderMode"] = psr.renderMode.ToString();
            targetState["Alignment"] = psr.alignment.ToString();
            targetState["SortingLayerName"] = psr.sortingLayerName;
            targetState["SortingOrder"] = psr.sortingOrder;
            targetState["SharedMaterials"] = matNames;
        }
    }
}

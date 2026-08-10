using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Tilemaps.TilemapRenderer components.
    /// Extracts sortingLayerName, sortingOrder, mode, detectChunkCullingBounds, and maskInteraction.
    /// </summary>
    public class TilemapRendererSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(TilemapRenderer).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not TilemapRenderer tr) return;

            targetState["SortingLayerName"] = tr.sortingLayerName;
            targetState["SortingOrder"] = tr.sortingOrder;
            targetState["Mode"] = tr.mode.ToString();
            targetState["DetectChunkCullingBounds"] = tr.detectChunkCullingBounds.ToString();
            targetState["MaskInteraction"] = tr.maskInteraction.ToString();
        }
    }
}

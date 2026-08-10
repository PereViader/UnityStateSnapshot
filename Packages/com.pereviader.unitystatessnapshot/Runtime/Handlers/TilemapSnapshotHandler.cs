using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Tilemaps.Tilemap components.
    /// Extracts cellBounds, size, color hex, tileAnchor, orientation, and origin.
    /// </summary>
    public class TilemapSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Tilemap).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Tilemap tilemap) return;

            int precision = settings?.FloatPrecision ?? 3;

            var bounds = tilemap.cellBounds;
            targetState["CellBounds"] = $"Position: ({bounds.xMin}, {bounds.yMin}, {bounds.zMin}), Size: ({bounds.size.x}, {bounds.size.y}, {bounds.size.z})";
            targetState["Size"] = $"({tilemap.size.x}, {tilemap.size.y}, {tilemap.size.z})";
            targetState["Color"] = SnapshotScrubber.ScrubColor(tilemap.color);
            targetState["TileAnchor"] = SnapshotScrubber.ScrubVector3(tilemap.tileAnchor, precision);
            targetState["Orientation"] = tilemap.orientation.ToString();
            targetState["Origin"] = $"({tilemap.origin.x}, {tilemap.origin.y}, {tilemap.origin.z})";
        }
    }
}

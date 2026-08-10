using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Grid components.
    /// Extracts cellSize, cellGap, cellLayout, and cellSwizzle.
    /// </summary>
    public class GridSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Grid).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Grid grid) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["CellSize"] = SnapshotScrubber.ScrubVector3(grid.cellSize, precision);
            targetState["CellGap"] = SnapshotScrubber.ScrubVector3(grid.cellGap, precision);
            targetState["CellLayout"] = grid.cellLayout.ToString();
            targetState["CellSwizzle"] = grid.cellSwizzle.ToString();
        }
    }
}

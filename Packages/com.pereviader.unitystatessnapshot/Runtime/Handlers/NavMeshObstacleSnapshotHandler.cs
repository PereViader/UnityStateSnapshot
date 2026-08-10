using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.AI.NavMeshObstacle components.
    /// Extracts shape, center, size, radius, height, carving, and carveOnlyStationary.
    /// </summary>
    public class NavMeshObstacleSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(NavMeshObstacle).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not NavMeshObstacle obstacle) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Shape"] = obstacle.shape.ToString();
            targetState["Center"] = SnapshotScrubber.ScrubVector3(obstacle.center, precision);
            targetState["Size"] = SnapshotScrubber.ScrubVector3(obstacle.size, precision);
            targetState["Radius"] = SnapshotScrubber.RoundFloat(obstacle.radius, precision);
            targetState["Height"] = SnapshotScrubber.RoundFloat(obstacle.height, precision);
            targetState["Carving"] = obstacle.carving;
            targetState["CarveOnlyStationary"] = obstacle.carveOnlyStationary;
        }
    }
}

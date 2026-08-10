using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Collider2D components (BoxCollider2D, CircleCollider2D, CapsuleCollider2D, PolygonCollider2D).
    /// Extracts isTrigger, offset, size, radius, sharedMaterial name, density, usedByEffector, and usedByComposite.
    /// </summary>
    public class Collider2DSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Collider2D).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Collider2D col) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["IsTrigger"] = col.isTrigger;
            targetState["Offset"] = SnapshotScrubber.ScrubVector2(col.offset, precision);
            targetState["Density"] = SnapshotScrubber.RoundFloat(col.density, precision);
            targetState["UsedByEffector"] = col.usedByEffector;
            targetState["UsedByComposite"] = col.usedByComposite;
            targetState["SharedMaterial"] = col.sharedMaterial != null ? SnapshotScrubber.ScrubName(col.sharedMaterial.name) : "null";

            if (col is BoxCollider2D box)
            {
                targetState["Size"] = SnapshotScrubber.ScrubVector2(box.size, precision);
            }
            else if (col is CircleCollider2D circle)
            {
                targetState["Radius"] = SnapshotScrubber.RoundFloat(circle.radius, precision);
            }
            else if (col is CapsuleCollider2D capsule)
            {
                targetState["Size"] = SnapshotScrubber.ScrubVector2(capsule.size, precision);
                targetState["Direction"] = capsule.direction.ToString();
            }
            else if (col is PolygonCollider2D poly)
            {
                targetState["PathCount"] = poly.pathCount;
                targetState["PointsCount"] = poly.points != null ? poly.points.Length : 0;
            }
        }
    }
}

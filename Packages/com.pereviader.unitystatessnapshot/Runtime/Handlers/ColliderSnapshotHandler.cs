using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Collider components (BoxCollider, SphereCollider, CapsuleCollider, MeshCollider).
    /// Extracts sharedMaterial name, isTrigger, center, size, radius, height, direction, convex, and sharedMesh.
    /// </summary>
    public class ColliderSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Collider).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Collider col) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["IsTrigger"] = col.isTrigger;
            targetState["SharedMaterial"] = col.sharedMaterial != null ? SnapshotScrubber.ScrubName(col.sharedMaterial.name) : "null";

            if (col is BoxCollider box)
            {
                targetState["Center"] = SnapshotScrubber.ScrubVector3(box.center, precision);
                targetState["Size"] = SnapshotScrubber.ScrubVector3(box.size, precision);
            }
            else if (col is SphereCollider sphere)
            {
                targetState["Center"] = SnapshotScrubber.ScrubVector3(sphere.center, precision);
                targetState["Radius"] = SnapshotScrubber.RoundFloat(sphere.radius, precision);
            }
            else if (col is CapsuleCollider capsule)
            {
                targetState["Center"] = SnapshotScrubber.ScrubVector3(capsule.center, precision);
                targetState["Radius"] = SnapshotScrubber.RoundFloat(capsule.radius, precision);
                targetState["Height"] = SnapshotScrubber.RoundFloat(capsule.height, precision);
                targetState["Direction"] = capsule.direction;
            }
            else if (col is MeshCollider mesh)
            {
                targetState["Convex"] = mesh.convex;
                targetState["SharedMesh"] = mesh.sharedMesh != null ? SnapshotScrubber.ScrubName(mesh.sharedMesh.name) : "null";
            }
        }
    }
}

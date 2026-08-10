using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Rigidbody components.
    /// Extracts mass, drag, angularDrag, useGravity, isKinematic, interpolation, collisionDetectionMode, constraints, and isSleeping.
    /// </summary>
    public class RigidbodySnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Rigidbody).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Rigidbody rb) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Mass"] = SnapshotScrubber.RoundFloat(rb.mass, precision);
            targetState["Drag"] = SnapshotScrubber.RoundFloat(rb.linearDamping, precision);
            targetState["AngularDrag"] = SnapshotScrubber.RoundFloat(rb.angularDamping, precision);
            targetState["UseGravity"] = rb.useGravity;
            targetState["IsKinematic"] = rb.isKinematic;
            targetState["Interpolation"] = rb.interpolation.ToString();
            targetState["CollisionDetectionMode"] = rb.collisionDetectionMode.ToString();
            targetState["Constraints"] = rb.constraints.ToString();
            targetState["IsSleeping"] = rb.IsSleeping();
        }
    }
}

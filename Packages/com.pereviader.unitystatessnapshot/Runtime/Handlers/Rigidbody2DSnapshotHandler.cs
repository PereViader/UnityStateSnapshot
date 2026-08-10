using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Rigidbody2D components.
    /// Extracts bodyType, mass, linearDamping, angularDamping, gravityScale, simulated, interpolation, collisionDetectionMode, and constraints.
    /// </summary>
    public class Rigidbody2DSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Rigidbody2D).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Rigidbody2D rb) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["BodyType"] = rb.bodyType.ToString();
            targetState["Mass"] = SnapshotScrubber.RoundFloat(rb.mass, precision);
            targetState["LinearDamping"] = SnapshotScrubber.RoundFloat(rb.linearDamping, precision);
            targetState["AngularDamping"] = SnapshotScrubber.RoundFloat(rb.angularDamping, precision);
            targetState["GravityScale"] = SnapshotScrubber.RoundFloat(rb.gravityScale, precision);
            targetState["Simulated"] = rb.simulated;
            targetState["Interpolation"] = rb.interpolation.ToString();
            targetState["CollisionDetectionMode"] = rb.collisionDetectionMode.ToString();
            targetState["Constraints"] = rb.constraints.ToString();
        }
    }
}

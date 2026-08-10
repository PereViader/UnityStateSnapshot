using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.CharacterController components.
    /// Extracts isGrounded, radius, height, center, slopeLimit, stepOffset, skinWidth, and minMoveDistance.
    /// </summary>
    public class CharacterControllerSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(CharacterController).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not CharacterController cc) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["IsGrounded"] = cc.isGrounded;
            targetState["Radius"] = SnapshotScrubber.RoundFloat(cc.radius, precision);
            targetState["Height"] = SnapshotScrubber.RoundFloat(cc.height, precision);
            targetState["Center"] = SnapshotScrubber.ScrubVector3(cc.center, precision);
            targetState["SlopeLimit"] = SnapshotScrubber.RoundFloat(cc.slopeLimit, precision);
            targetState["StepOffset"] = SnapshotScrubber.RoundFloat(cc.stepOffset, precision);
            targetState["SkinWidth"] = SnapshotScrubber.RoundFloat(cc.skinWidth, precision);
            targetState["MinMoveDistance"] = SnapshotScrubber.RoundFloat(cc.minMoveDistance, precision);
        }
    }
}

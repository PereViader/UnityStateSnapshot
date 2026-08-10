using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.AI.NavMeshAgent components.
    /// Extracts speed, angularSpeed, acceleration, stoppingDistance, autoBraking, radius, height,
    /// baseOffset, isStopped, updatePosition, updateRotation, areaMask, and avoidancePriority.
    /// </summary>
    public class NavMeshAgentSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(NavMeshAgent).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not NavMeshAgent agent) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Speed"] = SnapshotScrubber.RoundFloat(agent.speed, precision);
            targetState["AngularSpeed"] = SnapshotScrubber.RoundFloat(agent.angularSpeed, precision);
            targetState["Acceleration"] = SnapshotScrubber.RoundFloat(agent.acceleration, precision);
            targetState["StoppingDistance"] = SnapshotScrubber.RoundFloat(agent.stoppingDistance, precision);
            targetState["AutoBraking"] = agent.autoBraking;
            targetState["Radius"] = SnapshotScrubber.RoundFloat(agent.radius, precision);
            targetState["Height"] = SnapshotScrubber.RoundFloat(agent.height, precision);
            targetState["BaseOffset"] = SnapshotScrubber.RoundFloat(agent.baseOffset, precision);

            bool isStopped = false;
            try
            {
                if (agent.isOnNavMesh)
                {
                    isStopped = agent.isStopped;
                }
            }
            catch
            {
                // Fallback when agent is not placed on NavMesh
            }
            targetState["IsStopped"] = isStopped;

            targetState["UpdatePosition"] = agent.updatePosition;
            targetState["UpdateRotation"] = agent.updateRotation;
            targetState["AreaMask"] = agent.areaMask;
            targetState["AvoidancePriority"] = agent.avoidancePriority;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.ParticleSystem components.
    /// Extracts deterministic playback status, duration, loop, emission rate, and main module properties while ignoring fluctuating particle counts and seeds.
    /// </summary>
    public class ParticleSystemSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(ParticleSystem).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not ParticleSystem ps) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["IsPlaying"] = ps.isPlaying;
            targetState["IsPaused"] = ps.isPaused;
            targetState["IsStopped"] = ps.isStopped;
            targetState["IsEmitting"] = ps.isEmitting;

            var main = ps.main;
            targetState["Duration"] = SnapshotScrubber.RoundFloat(main.duration, precision);
            targetState["Loop"] = main.loop;
            targetState["StartLifetime"] = SnapshotScrubber.RoundFloat(main.startLifetime.constant, precision);
            targetState["StartSpeed"] = SnapshotScrubber.RoundFloat(main.startSpeed.constant, precision);
            targetState["StartSize"] = SnapshotScrubber.RoundFloat(main.startSize.constant, precision);
            targetState["StartColor"] = SnapshotScrubber.ScrubColor(main.startColor.color);
            targetState["SimulationSpace"] = main.simulationSpace.ToString();
            targetState["MaxParticles"] = main.maxParticles;

            var emission = ps.emission;
            targetState["EmissionEnabled"] = emission.enabled;
            targetState["EmissionRate"] = SnapshotScrubber.RoundFloat(emission.rateOverTime.constant, precision);

            var shape = ps.shape;
            targetState["ShapeEnabled"] = shape.enabled;
            targetState["ShapeType"] = shape.shapeType.ToString();
        }
    }
}

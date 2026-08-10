using System;
using System.Collections.Generic;
using UnityEngine;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.Animator components.
    /// Extracts controller name, speed, applyRootMotion, updateMode, cullingMode, current clip name, and animator parameters.
    /// </summary>
    public class AnimatorSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(Animator).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not Animator animator) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Controller"] = animator.runtimeAnimatorController != null ? SnapshotScrubber.ScrubName(animator.runtimeAnimatorController.name) : "null";
            targetState["Speed"] = SnapshotScrubber.RoundFloat(animator.speed, precision);
            targetState["ApplyRootMotion"] = animator.applyRootMotion;
            targetState["UpdateMode"] = animator.updateMode.ToString();
            targetState["CullingMode"] = animator.cullingMode.ToString();

            // Extract current active animation clip if available
            try
            {
                if (animator.isInitialized && animator.runtimeAnimatorController != null)
                {
                    AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfos != null && clipInfos.Length > 0 && clipInfos[0].clip != null)
                    {
                        targetState["CurrentClip"] = SnapshotScrubber.ScrubName(clipInfos[0].clip.name);
                    }
                }
            }
            catch
            {
                // Clip info may not be ready in certain edit-mode states
            }

            // Extract Animator parameters
            try
            {
                if (animator.parameterCount > 0)
                {
                    var paramDict = new SortedDictionary<string, object>(StringComparer.Ordinal);
                    foreach (AnimatorControllerParameter p in animator.parameters)
                    {
                        if (p == null) continue;

                        switch (p.type)
                        {
                            case AnimatorControllerParameterType.Float:
                                paramDict[p.name] = SnapshotScrubber.RoundFloat(animator.GetFloat(p.nameHash), precision);
                                break;
                            case AnimatorControllerParameterType.Int:
                                paramDict[p.name] = animator.GetInteger(p.nameHash);
                                break;
                            case AnimatorControllerParameterType.Bool:
                                paramDict[p.name] = animator.GetBool(p.nameHash);
                                break;
                            case AnimatorControllerParameterType.Trigger:
                                paramDict[p.name] = animator.GetBool(p.nameHash);
                                break;
                        }
                    }

                    if (paramDict.Count > 0)
                    {
                        targetState["Parameters"] = paramDict;
                    }
                }
            }
            catch
            {
                // Ignore parameter extraction exceptions
            }
        }
    }
}

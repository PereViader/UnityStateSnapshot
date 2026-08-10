using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for UnityEngine.UI.LayoutGroup components (Horizontal, Vertical, Grid).
    /// </summary>
    public class LayoutGroupSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(LayoutGroup).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not LayoutGroup lg) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["ChildAlignment"] = lg.childAlignment.ToString();
            targetState["Padding"] = $"{lg.padding.left},{lg.padding.right},{lg.padding.top},{lg.padding.bottom}";

            if (lg is HorizontalOrVerticalLayoutGroup hvl)
            {
                targetState["Spacing"] = SnapshotScrubber.RoundFloat(hvl.spacing, precision);
                targetState["ChildControlWidth"] = hvl.childControlWidth;
                targetState["ChildControlHeight"] = hvl.childControlHeight;
                targetState["ChildScaleWidth"] = hvl.childScaleWidth;
                targetState["ChildScaleHeight"] = hvl.childScaleHeight;
                targetState["ChildForceExpandWidth"] = hvl.childForceExpandWidth;
                targetState["ChildForceExpandHeight"] = hvl.childForceExpandHeight;
                targetState["ReverseArrangement"] = hvl.reverseArrangement;
            }
            else if (lg is GridLayoutGroup glg)
            {
                targetState["CellSize"] = SnapshotScrubber.ScrubVector2(glg.cellSize, precision);
                targetState["Spacing"] = SnapshotScrubber.ScrubVector2(glg.spacing, precision);
                targetState["StartCorner"] = glg.startCorner.ToString();
                targetState["StartAxis"] = glg.startAxis.ToString();
                targetState["Constraint"] = glg.constraint.ToString();
                targetState["ConstraintCount"] = glg.constraintCount;
            }
        }
    }
}

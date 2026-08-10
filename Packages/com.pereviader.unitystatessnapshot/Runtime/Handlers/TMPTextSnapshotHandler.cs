#if UNITY_TMP_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for TextMeshPro / TextMeshProUGUI / TMP_Text components.
    /// </summary>
    public class TMPTextSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 50;

        public bool CanHandle(Type componentType)
        {
            return typeof(TMP_Text).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not TMP_Text tmp) return;

            int precision = settings?.FloatPrecision ?? 3;

            targetState["Text"] = tmp.text ?? string.Empty;
            targetState["Color"] = SnapshotScrubber.ScrubColor(tmp.color);
            targetState["FontSize"] = SnapshotScrubber.RoundFloat(tmp.fontSize, precision);
            targetState["Alignment"] = tmp.alignment.ToString();
        }
    }
}
#endif

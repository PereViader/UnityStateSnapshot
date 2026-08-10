#if UNITY_TMP_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PereViader.UnityStateSnapshot
{
    /// <summary>
    /// Snapshot handler for TMPro.TMP_InputField components.
    /// </summary>
    public class TMPInputFieldSnapshotHandler : ISnapshotComponentHandler
    {
        public int Priority => 60;

        public bool CanHandle(Type componentType)
        {
            return typeof(TMP_InputField).IsAssignableFrom(componentType);
        }

        public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
        {
            if (component is not TMP_InputField inputField) return;

            SelectableSnapshotHandler.ExtractSelectableState(inputField, targetState);

            targetState["Text"] = inputField.text ?? string.Empty;
            targetState["ContentType"] = inputField.contentType.ToString();
            targetState["CharacterLimit"] = inputField.characterLimit;
            targetState["IsFocused"] = inputField.isFocused;
            targetState["LineType"] = inputField.lineType.ToString();
            targetState["InputType"] = inputField.inputType.ToString();
            targetState["KeyboardType"] = inputField.keyboardType.ToString();
            targetState["ReadOnly"] = inputField.readOnly;
            targetState["CaretPosition"] = inputField.caretPosition;

            if (inputField.placeholder != null)
            {
                if (inputField.placeholder is TMP_Text placeholderText)
                {
                    targetState["Placeholder"] = placeholderText.text;
                }
                else
                {
                    targetState["Placeholder"] = SnapshotScrubber.ScrubName(inputField.placeholder.name);
                }
            }
        }
    }
}
#endif

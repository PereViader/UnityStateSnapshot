using System;
using System.Collections.Generic;

namespace PereViader.UnityStateSnapshot
{
    [Serializable]
    public class SelectableSnapshotDTO
    {
        public bool Interactable { get; set; }
        public string SelectionState { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHighlighted { get; set; }
    }

    [Serializable]
    public class ButtonSnapshotDTO
    {
        public bool Interactable { get; set; }
        public string SelectionState { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHighlighted { get; set; }
        public int OnClickListenerCount { get; set; }
    }

    [Serializable]
    public class ImageSnapshotDTO
    {
        public string SpriteName { get; set; }
        public string Color { get; set; }
        public string ImageType { get; set; }
        public bool RaycastTarget { get; set; }
    }

    [Serializable]
    public class RawImageSnapshotDTO
    {
        public string TextureName { get; set; }
        public string Color { get; set; }
        public bool RaycastTarget { get; set; }
        public RectDTO UVRect { get; set; }
    }

    [Serializable]
    public class TextSnapshotDTO
    {
        public string Text { get; set; }
        public string Color { get; set; }
        public float FontSize { get; set; }
        public string Alignment { get; set; }
    }

    [Serializable]
    public class CanvasSnapshotDTO
    {
        public string RenderMode { get; set; }
        public bool IsRootCanvas { get; set; }
        public int SortingOrder { get; set; }
        public int TargetDisplay { get; set; }
        public string AdditionalShaderChannels { get; set; }
    }

    [Serializable]
    public class ToggleSnapshotDTO
    {
        public bool Interactable { get; set; }
        public bool IsOn { get; set; }
        public string ToggleTransition { get; set; }
        public string Group { get; set; }
    }

    [Serializable]
    public class SliderSnapshotDTO
    {
        public bool Interactable { get; set; }
        public float Value { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public bool WholeNumbers { get; set; }
        public string Direction { get; set; }
    }

    [Serializable]
    public class ScrollbarSnapshotDTO
    {
        public bool Interactable { get; set; }
        public float Value { get; set; }
        public float Size { get; set; }
        public int NumberOfSteps { get; set; }
        public string Direction { get; set; }
    }

    [Serializable]
    public class ScrollRectSnapshotDTO
    {
        public bool Horizontal { get; set; }
        public bool Vertical { get; set; }
        public string MovementType { get; set; }
        public float HorizontalNormalizedPosition { get; set; }
        public float VerticalNormalizedPosition { get; set; }
    }

    [Serializable]
    public class CanvasGroupSnapshotDTO
    {
        public float Alpha { get; set; }
        public bool Interactable { get; set; }
        public bool BlocksRaycasts { get; set; }
        public bool IgnoreParentGroups { get; set; }
    }

    [Serializable]
    public class TMPTextSnapshotDTO
    {
        public string Text { get; set; }
        public string Color { get; set; }
        public float FontSize { get; set; }
        public string Alignment { get; set; }
    }

    [Serializable]
    public class TMPInputFieldSnapshotDTO
    {
        public string Text { get; set; }
        public string ContentType { get; set; }
        public int CharacterLimit { get; set; }
        public bool Interactable { get; set; }
        public bool IsFocused { get; set; }
        public string Placeholder { get; set; }
    }

    [Serializable]
    public class TMPDropdownSnapshotDTO
    {
        public int Value { get; set; }
        public string CaptionText { get; set; }
        public int OptionsCount { get; set; }
        public List<string> Options { get; set; }
    }
}

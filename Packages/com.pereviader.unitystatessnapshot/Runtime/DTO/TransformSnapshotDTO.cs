using System;

namespace PereViader.UnityStateSnapshot
{
    [Serializable]
    public class Vector3DTO
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3DTO() { }

        public Vector3DTO(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    [Serializable]
    public class Vector2DTO
    {
        public float X;
        public float Y;

        public Vector2DTO() { }

        public Vector2DTO(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    [Serializable]
    public class TransformSnapshotDTO
    {
        public Vector3DTO LocalPosition;
        public Vector3DTO LocalEulerAngles;
        public Vector3DTO LocalScale;
        public RectTransformSnapshotDTO RectTransform;
    }

    [Serializable]
    public class RectTransformSnapshotDTO
    {
        public Vector2DTO AnchoredPosition;
        public Vector2DTO SizeDelta;
        public Vector2DTO AnchorMin;
        public Vector2DTO AnchorMax;
        public Vector2DTO Pivot;
    }
}

using System;

namespace PereViader.UnityStateSnapshot
{
    [Serializable]
    public class Vector4DTO
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4DTO() { }

        public Vector4DTO(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    [Serializable]
    public class QuaternionDTO
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public QuaternionDTO() { }

        public QuaternionDTO(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    [Serializable]
    public class RectDTO
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public RectDTO() { }

        public RectDTO(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    [Serializable]
    public class BoundsDTO
    {
        public Vector3DTO Center;
        public Vector3DTO Size;

        public BoundsDTO() { }

        public BoundsDTO(Vector3DTO center, Vector3DTO size)
        {
            Center = center;
            Size = size;
        }
    }
}

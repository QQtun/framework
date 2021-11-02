using UnityEngine;

namespace Core.Framework.Utility
{
    public static class VectorEx
    {
        public static Vector2Int Lerp(Vector2Int a, Vector2Int b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Vector2Int((int)(a.x + (b.x - a.x) * t), (int)(a.y + (b.y - a.y) * t));
        }

        public static void Lerp(Vector2Int a, Vector2Int b, float t, out Vector2Int c)
        {
            t = Mathf.Clamp01(t);
            c = Lerp(a, b, t);
        }

        public static int SqrDistance(Vector2Int a, Vector2Int b)
        {
            return (int)(a - b).sqrMagnitude;
        }

        public static Vector3 ToVector3f(this Vector2Int v, float y = 0)
        {
            return new Vector3(v.x, y, v.y);
        }

        public static Vector2 ToVector2f(this Vector2Int v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector3 ToVector3(this Vector2 v, float y = 0)
        {
            return new Vector3(v.x, y, v.y);
        }

        public static Vector2 ToVector2IgnoreY(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

    }
}